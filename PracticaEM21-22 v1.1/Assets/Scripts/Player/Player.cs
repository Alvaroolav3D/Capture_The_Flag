using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    #region Variables

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    public NetworkVariable<PlayerState> State;
    public NetworkVariable<PlayerLiveState> LiveState;
    public NetworkVariable<FixedString64Bytes> playerName;
    public NetworkVariable<bool> isReady;
    public NetworkVariable<bool> gameReady;
    
    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        //obtengo el networkmanager por el que tengo que asignar cada vez que se conecte un jugador que ejecute algo.
        NetworkManager.OnClientConnectedCallback += ConfigurePlayer;

        State = new NetworkVariable<PlayerState>();
        LiveState = new NetworkVariable<PlayerLiveState>();
        playerName = new NetworkVariable<FixedString64Bytes>();
        isReady = new NetworkVariable<bool>();
        gameReady = new NetworkVariable<bool>();
    }

    private void OnEnable()
    {
        //Las networkvariables no se manejan de la misma manera.
        //Asigno un delegado y cada vez que cambio el valor a esta variable que me llega del servidor yo como cliente si ha cambiado hago algo

        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged += OnPlayerStateValueChanged;
        LiveState.OnValueChanged += OnPlayerLiveStateValueChanged;
        isReady.OnValueChanged += OnPlayerIsReadyValueChanged;
        gameReady.OnValueChanged += OnGameReadyValueChanged;
        playerName.OnValueChanged += OnPlayerNameValueChanged;
    }

    private void OnDisable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged -= OnPlayerStateValueChanged;
        LiveState.OnValueChanged -= OnPlayerLiveStateValueChanged;
        isReady.OnValueChanged -= OnPlayerIsReadyValueChanged;
        gameReady.OnValueChanged -= OnGameReadyValueChanged;
        playerName.OnValueChanged -= OnPlayerNameValueChanged;
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
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
        UpdatePlayerLiveStateServerRpc(PlayerLiveState.Alive);
        UpdatePlayerIsReadyServerRpc(false);
        UpdateGameReadyServerRpc(false);
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
    [ServerRpc]
    public void UpdatePlayerLiveStateServerRpc(PlayerLiveState state)
    {
        LiveState.Value = state;
    }
    [ServerRpc]
    public void UpdatePlayerIsReadyServerRpc(bool ready)
    {
        isReady.Value = ready;
    }
    [ServerRpc]
    public void UpdateGameReadyServerRpc(bool ready)
    {
        gameReady.Value = ready;
    }
    [ServerRpc]
    public void UpdatePlayerNameServerRpc(FixedString64Bytes name)
    {
        playerName.Value = name;
    }

    #endregion

    #endregion

    #region Netcode Related Methods

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
    }

    void OnPlayerLiveStateValueChanged(PlayerLiveState previous, PlayerLiveState current)
    {
        LiveState.Value = current;
    }

    void OnPlayerIsReadyValueChanged(bool previous, bool current)
    {
        isReady.Value = current;
    }
    void OnGameReadyValueChanged(bool previous, bool current)
    {
        gameReady.Value = current;
    }
    void OnPlayerNameValueChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        playerName.Value = current;
    }

    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}

public enum PlayerLiveState
{
    Alive = 0,
    Dead = 1
}
