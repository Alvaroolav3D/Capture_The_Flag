using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    #region Variables

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    public NetworkVariable<PlayerState> State;
    List<Vector3> spawnPositions;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        //obtengo el networkmanager por el que tengo que asignar cada vez que se conecte un jugador que ejecute algo.
        NetworkManager.OnClientConnectedCallback += ConfigurePlayer;

        State = new NetworkVariable<PlayerState>();
        spawnPositions = new List<Vector3>();
    }

    private void OnEnable()
    {
        //Las networkvariables no se manejan de la misma manera.
        //Asigno un delegado y cada vez que cambio el valor a esta variable que me llega del servidor yo como cliente si ha cambiado hago algo

        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged += OnPlayerStateValueChanged;
    }

    private void OnDisable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged -= OnPlayerStateValueChanged;
    }

    #endregion

    #region Config Methods

    void ConfigurePlayer(ulong clientID)
    {
        //en esta funcion, si eres el jugador se ejecutaran los siguientes metodos
        if (IsLocalPlayer)
        {
            ConfigurePlayer();
            ConfigureCamera();
            ConfigureControls();
        }
    }

    void ConfigurePlayer()
    {
        //le doy un valor inicial al jugador de Grounded

        //spawnPositions.Add(new Vector3(-8.0f, -2.893f, transform.position.z));
        //spawnPositions.Add(new Vector3(-5.0f, 0.93f, transform.position.z));
        //spawnPositions.Add(new Vector3(-0.2804f, -0.4019f, transform.position.z));
        //spawnPositions.Add(new Vector3(10.4f, -0.15f, transform.position.z));

        //transform.position = spawnPositions[Random.Range(0, spawnPositions.Count - 1)];
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
    }

    void ConfigureCamera()
    {
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/CinemachineBrainProperties.html
        var virtualCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        virtualCam.LookAt = transform;
        virtualCam.Follow = transform;
    }

    void ConfigureControls()
    {
        //toda la logica de control reside en imputHandler.cs
        GetComponent<InputHandler>().enabled = true;
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        State.Value = state;
    }

    #endregion

    #endregion

    #region Netcode Related Methods

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
    }

    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}
