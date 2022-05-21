using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ballPrefab;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }
}
