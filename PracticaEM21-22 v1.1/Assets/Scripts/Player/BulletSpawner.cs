using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : NetworkBehaviour
{
    [SerializeField] public NetworkObject bulletPrefab;
    InputHandler handler;
    Player player; //referencia al player que corresponde

    private void Awake()
    {
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();
    }
    private void OnEnable()
    {
        handler.OnFire.AddListener(SpawnBulletServerRpc);
    }

    private void OnDisable()
    {
        handler.OnFire.RemoveListener(SpawnBulletServerRpc);
    }

    [ServerRpc]
    void SpawnBulletServerRpc(Vector2 mousePos)
    {
        //en este punto solo el servidor seria consciente de que se ha spawneado una bala en spawnPos, no los clientes
        //opr tanto tengo que generar la bala en los clientes
        NetworkObject bulletInstance = Instantiate(bulletPrefab, player.transform.position, Quaternion.identity);

        bulletInstance.GetComponent<Bullet>().mouseposition = mousePos;
        bulletInstance.GetComponent<Bullet>().player = player;

        //genero la bala en los clientes
        bulletInstance.SpawnWithOwnership(OwnerClientId);
    }
}
