using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    [SerializeField] private Rigidbody2D bulletRigidbody;

    [HideInInspector] public Vector2 mouseposition;
    [HideInInspector] public ulong playerId;

    public float speed;
    public int damage;
    private byte teamId;

    public void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == playerId)
            {
                //el color es el mismo que el color del personaje. En caso de que este en algun equipo, la bala sera del color de ese equipo
                bulletRenderer.color = p.GetComponent<SpriteRenderer>().color;
                teamId = p.GetComponent<TeamPlayer>().teamId.Value;

                Physics2D.IgnoreCollision(p.GetComponent<CapsuleCollider2D>(), GetComponent<CircleCollider2D>());
            }
        }

        var direction = (mouseposition - (Vector2)transform.position).normalized;
        bulletRigidbody.velocity = direction * speed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!IsClient) //lo clientes no pueden modificar los valores de una networkvariable por eso es necesario restringir el acceso
            {
                var player = collision.gameObject;
                if (player.GetComponent<TeamPlayer>().teamId.Value == teamId || player.GetComponent<TeamPlayer>().teamId.Value == 0)
                {
                    var currentHitPoints = player.GetComponent<PlayerController>().hitPoints.Value;
                    player.GetComponent<PlayerController>().OnHitPointsValueChanged(currentHitPoints, currentHitPoints - damage);
                }
            }
        }
        if (IsOwner)
        {
            DestroyBulletServerRpc();
        }
    }

    [ServerRpc]
    private void DestroyBulletServerRpc()
    {
        //Destruyendo un networkObject en el servidor el objeto se destruye en todos los clientes
        Destroy(gameObject);
    }
}
