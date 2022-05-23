using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : NetworkBehaviour
{
    [SerializeField] public NetworkObject bulletPrefab;
    InputHandler handler;
    Player player;

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
        NetworkObject ballInstance = Instantiate(bulletPrefab, player.transform.position, Quaternion.identity);

        ballInstance.GetComponent<Bullet>().mouseposition = mousePos;
        ballInstance.GetComponent<Bullet>().playerId = OwnerClientId;

        //spawneo la bala en los clientes
        ballInstance.SpawnWithOwnership(OwnerClientId);
    }
}
