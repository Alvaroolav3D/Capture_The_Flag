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

    public void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == playerId)
            {
                //el color es el mismo que el color del personaje. En caso de que este en algun equipo, la bala sera del color de ese equipo
                bulletRenderer.color = p.GetComponent<SpriteRenderer>().color;
            }
        }

        var direction = (mouseposition - (Vector2)transform.position).normalized;
        bulletRigidbody.velocity = direction * speed;
    }
}
