using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : NetworkBehaviour
{

    #region Variables

    readonly float speed = 3.4f;
    readonly float jumpHeight = 6.5f;
    readonly float gravity = 1.5f;
    readonly int maxJumps = 2;
    readonly int maxHitPoints = 6;

    LayerMask _layer;
    int _jumpsLeft; //numero de saltos restantes

    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/ContactFilter2D.html
    ContactFilter2D filter; //permite definir como quiero y hacer contacto entre distintos colliders
    InputHandler handler;
    Player player;
    Rigidbody2D rb;
    new CapsuleCollider2D collider;
    Animator anim;
    SpriteRenderer spriteRenderer;
    UIManager uiManager;
    GameManager gameManager;

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    NetworkVariable<bool> FlipSprite; //determina a que direccion mira el jugador
    public NetworkVariable<int> hitPoints; //determina a que direccion mira el jugador
    public SpriteRenderer weaponSpriteRenderer;
    public TextMeshPro nameRenderer;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        //inicializo variables
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        FlipSprite = new NetworkVariable<bool>();
        hitPoints = new NetworkVariable<int>();

        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        //nos suscribimos a los metodos
        handler.OnMove.AddListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.AddListener(PerformJumpServerRpc);
        handler.OnDie.AddListener(UpdateDeadStateServerRpc);
        handler.OnMoveFixedUpdate.AddListener(UpdatePlayerPositionServerRpc);

        FlipSprite.OnValueChanged += OnFlipSpriteValueChanged;
        hitPoints.OnValueChanged += OnHitPointsValueChanged;
    }

    private void OnDisable()
    {
        handler.OnMove.RemoveListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.RemoveListener(PerformJumpServerRpc);
        handler.OnDie.RemoveListener(UpdateDeadStateServerRpc);
        handler.OnMoveFixedUpdate.RemoveListener(UpdatePlayerPositionServerRpc);

        FlipSprite.OnValueChanged -= OnFlipSpriteValueChanged;
        hitPoints.OnValueChanged -= OnHitPointsValueChanged;
    }

    void Start()
    {
        // Configure Rigidbody2D
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; //asi no se atraviesan los objetos
        rb.gravityScale = gravity;

        // Configure LayerMask
        _layer = LayerMask.GetMask("Obstacles"); //capa con la que quiero que mi personaje tenga colisiones.

        // Configure ContactFilter2D
        filter.minNormalAngle = 45;
        filter.maxNormalAngle = 135;
        filter.useNormalAngle = true;
        filter.layerMask = _layer; //el contact filter solo se va a aplicar a la capa layer que en este caso es obstacles

        //establezco la vida inicial
        hitPoints.Value = maxHitPoints;
        uiManager.UpdateLifeUI(hitPoints.Value);

        //referencia al spriterenderer del arma
        weaponSpriteRenderer = GetComponent<WeaponAim>().weaponRenderer;
    }

    private void Update()
    {
        if (player.LiveState.Value == PlayerLiveState.Dead)
        {
            spriteRenderer.enabled = false;
            weaponSpriteRenderer.enabled = false;
            nameRenderer.enabled = false;
            if(IsLocalPlayer) handler.enabled = false; //evito que el personaje pueda moverse quitandole el inputHandler
        }
        else
        {
            spriteRenderer.enabled = true;
            weaponSpriteRenderer.enabled = true;
            nameRenderer.enabled = true;
            if (IsLocalPlayer) handler.enabled = true; //evito que el personaje pueda moverse quitandole el inputHandler
        }
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdatePlayerVisualsServerRpc(Vector2 input)
    {
        //funcion que actualiza lo visual del jugador, como su animacion o su orientacion
        UpdateAnimatorStateServerRpc(); //no hay necesidad de que sea ServerRpc ya que la funcion que lo usa ya lo es
        UpdateSpriteOrientation(input);
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdateAnimatorStateServerRpc()
    {
        if (IsGrounded)
        {
            anim.SetBool("isGrounded", true);
            anim.SetBool("isJumping", false);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void PerformJumpServerRpc()
    {
        if (player.State.Value == PlayerState.Grounded)
        {
            _jumpsLeft = maxJumps;
        }
        else if (_jumpsLeft == 0)
        {
            return;
        }

        player.State.Value = PlayerState.Jumping;
        anim.SetBool("isJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
        _jumpsLeft--;
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdatePlayerPositionServerRpc(Vector2 input)
    {
        if (IsGrounded)
        {
            player.State.Value = PlayerState.Grounded;
        }

        if ((player.State.Value != PlayerState.Hooked))
        {
            rb.velocity = new Vector2(input.x * speed, rb.velocity.y);
        }
    }

    [ServerRpc]
    void UpdateDeadStateServerRpc()
    {
        UpdatePlayerDeadState();
    }

    #endregion

    #endregion

    #region Methods

    void UpdatePlayerDeadState()
    {
        if (hitPoints.Value <= 0 && player.LiveState.Value == PlayerLiveState.Alive)
        {
            //inicio cuenta atras de corutina
            StartCoroutine(TimeDeath());
        }
    }

    IEnumerator TimeDeath()
    {
        spriteRenderer.enabled = false;
        player.LiveState.Value = PlayerLiveState.Dead;
        print("muerto");

        yield return new WaitForSeconds(3);

        print("vivo");
        //inicializar posicion aleatoria. Gracias al networkTransform en cleinte y servidor se colocara en el mismo lugar
        gameObject.transform.position = gameManager.spawnPoints[Random.Range(0, gameManager.spawnPoints.Count - 1)].position;
        player.LiveState.Value = PlayerLiveState.Alive;
        hitPoints.Value = maxHitPoints;
        spriteRenderer.enabled = true;
    }

    void UpdateSpriteOrientation(Vector2 input)
    {
        if (input.x < 0)
        {
            FlipSprite.Value = false;
        }
        else if (input.x > 0)
        {
            FlipSprite.Value = true;
        }
    }

    void OnFlipSpriteValueChanged(bool previous, bool current)
    {
        spriteRenderer.flipX = current;
    }

    public void OnHitPointsValueChanged(int previous, int current)
    {
        hitPoints.Value = current;
        if (IsLocalPlayer) //cada vez que hay un cambio en la vida del jugador solo se modifique el Ui con el valor propio
        {
            uiManager.UpdateLifeUI(hitPoints.Value);
        }
    }

    bool IsGrounded => collider.IsTouching(filter) && rb.velocity.y == 0;

    #endregion

}
