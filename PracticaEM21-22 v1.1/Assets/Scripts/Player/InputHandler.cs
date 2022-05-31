using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.Netcode;

public class InputHandler : NetworkBehaviour
{

    #region Variables

    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html
    [SerializeField] InputAction _move;
    [SerializeField] InputAction _jump;
    [SerializeField] InputAction _hook;
    [SerializeField] InputAction _fire;
    [SerializeField] InputAction _mousePosition;

    // https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html
    public UnityEvent<Vector2> OnMove;
    public UnityEvent<Vector2> OnMoveFixedUpdate;
    public UnityEvent<Vector2> OnMousePosition;
    public UnityEvent<Vector2> OnHook;
    public UnityEvent<Vector2> OnHookRender;
    public UnityEvent<Vector2> OnFire;
    public UnityEvent OnJump;
    public UnityEvent OnDie;

    Player player;

    //valor para la informacion del input cacheada para que cuando juegue con el jugador a eces necesitamos hacer actualizaciones en el update o en el fixedupdate
    Vector2 CachedMoveInput { get; set; } 

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        //Inicializo los bindings de los inputActions
        _move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Left", "<Keyboard>/a")
            .With("Down", "<Keyboard>/s")
            .With("Right", "<Keyboard>/d");

        _jump.AddBinding("<Keyboard>/space");
        _hook.AddBinding("<Mouse>/middleButton");
        _fire.AddBinding("<Mouse>/leftButton");
        _mousePosition.AddBinding("<Mouse>/position");

        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        _move.Enable();
        _jump.Enable();
        _hook.Enable();
        _fire.Enable();
        _mousePosition.Enable();
    }

    private void OnDisable()
    {
        _move.Disable();
        _jump.Disable();
        _hook.Disable();
        _fire.Disable();
        _mousePosition.Disable();
    }

    private void Update()
    {
        //si la partida no esta lista el jugador no se podra mover
        if (!player.gameReady.Value) return;
        if (IsLocalPlayer)
        {
            Die();
            CachedMoveInput = _move.ReadValue<Vector2>();
            var mousePosition = _mousePosition.ReadValue<Vector2>();

            var hookPerformed = _hook.WasPerformedThisFrame(); 
            var jumpPerformed = _jump.WasPerformedThisFrame();
            var firePerformed = _fire.WasPerformedThisFrame();

            Move(CachedMoveInput);
            MousePosition(mousePosition);

            // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Camera.ScreenToWorldPoint.html
            var screenPoint = Camera.main.ScreenToWorldPoint(mousePosition);

            if (hookPerformed) { Hook(screenPoint); }
            if (jumpPerformed) { Jump(); }
            if (firePerformed) { Fire(screenPoint); }

            HookRender(CachedMoveInput);
        }
    }

    private void FixedUpdate()
    {
        //solo se ejecuta este fixedUpdate en todo player
        MoveFixedUpdate(CachedMoveInput);
    }

    #endregion

    #region InputSystem Related Methods
    //todos estos delegados reciben los metodos en playercontrolles.cs. Aqui simplemente se estan llamando con el valor de movimiento.
    void Move(Vector2 input)
    {
        OnMove?.Invoke(input);
    }

    void MoveFixedUpdate(Vector2 input)
    {
        OnMoveFixedUpdate?.Invoke(input);
    }

    void Jump()
    {
        OnJump?.Invoke();
    }

    void Die()
    {
        OnDie?.Invoke();
    }

    void Hook(Vector2 input)
    {
        OnHook?.Invoke(input);
    }

    void HookRender(Vector2 input)
    {
        OnHookRender?.Invoke(input);
    }

    void Fire(Vector2 input)
    {
        OnFire?.Invoke(input);
    }

    void MousePosition(Vector2 input)
    {
        OnMousePosition?.Invoke(input);
    }

    #endregion

}