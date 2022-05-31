using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    [SerializeField] private Rigidbody2D bulletRigidbody;

    [HideInInspector] public Vector2 mouseposition;

    [HideInInspector] public Player player; //referencia al player que la genera

    [Header("Bullet Stats")]
    public float speed;
    public int damage;
    private byte teamId;

    public void Start()
    {
        //el color es el mismo que el color del personaje. En caso de que el jugador pertenezca a un equipo
        //en algun equipo, la bala sera del color de ese equipo

        bulletRenderer.color = player.GetComponent<SpriteRenderer>().color;
        teamId = player.GetComponent<TeamPlayer>().teamId.Value;

        //Impido que la bala colisione con el propio collider del jugador que la genera
        Physics2D.IgnoreCollision(player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());

        //calculo la direccion de la bala y le aplico la velocidad en ese eje
        var direction = (mouseposition - (Vector2)transform.position).normalized;
        bulletRigidbody.velocity = direction * speed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //lo hago en el servidor ya que una networkVariable no se puede modificar desde el cliente
        if (IsServer) 
        {
            //Si la bala colisiona con un jugador y ese jugador no pertenece al mismo equipo que la bala
            //o no tiene equipo (cuando el teamId == 0), le hace daño.
            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject;
                if (player.GetComponent<TeamPlayer>().teamId.Value != teamId || player.GetComponent<TeamPlayer>().teamId.Value == 0)
                {
                    var currentHitPoints = player.GetComponent<PlayerController>().hitPoints.Value;
                    player.GetComponent<PlayerController>().OnHitPointsValueChanged(currentHitPoints, currentHitPoints - damage);
                }
            }
        //elimino la bala en el servidor y por tanto en los clientes
        gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
