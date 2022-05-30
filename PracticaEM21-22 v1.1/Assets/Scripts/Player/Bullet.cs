using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    [SerializeField] private Rigidbody2D bulletRigidbody;

    [HideInInspector] public Vector2 mouseposition;

    [HideInInspector] public Player player;

    public float speed;
    public int damage;
    private byte teamId;

    public void Start()
    {
        //el color es el mismo que el color del personaje. En caso de que este en algun equipo, la bala sera del color de ese equipo
        bulletRenderer.color = player.GetComponent<SpriteRenderer>().color;
        teamId = player.GetComponent<TeamPlayer>().teamId.Value;

        Physics2D.IgnoreCollision(player.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());

        var direction = (mouseposition - (Vector2)transform.position).normalized;
        bulletRigidbody.velocity = direction * speed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer) //lo hago en el servidor ya que una networkVariable no se puede modificar desde el cliente
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject;
                if (player.GetComponent<TeamPlayer>().teamId.Value != teamId || player.GetComponent<TeamPlayer>().teamId.Value == 0)
                {
                    var currentHitPoints = player.GetComponent<PlayerController>().hitPoints.Value;
                    player.GetComponent<PlayerController>().OnHitPointsValueChanged(currentHitPoints, currentHitPoints - damage);
                }
            }
        gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
