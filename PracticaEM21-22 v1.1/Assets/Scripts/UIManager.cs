using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class UIManager : NetworkBehaviour
{

    #region Variables

    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameManager gameManager;
    UnityTransport transport;
    readonly ushort port = 7777;

    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button goHostMenu;
    [SerializeField] private Button goServerMenu;
    [SerializeField] private Button goClientMenu;

    [SerializeField] private GameObject backMenu;
    [SerializeField] private Button goBack;

    [Header("Host Menu")]
    [SerializeField] private GameObject hostMenu;
    [SerializeField] private InputField hostPasswordInputField;
    [SerializeField] private Button buttonHost;

    [Header("Server Menu")]
    [SerializeField] private GameObject serverMenu;
    [SerializeField] private InputField serverPasswordInputField;
    [SerializeField] private Button buttonServer;

    [Header("Client Menu")]
    [SerializeField] private GameObject clientMenu;
    [SerializeField] private InputField inputFieldIP;
    [SerializeField] private InputField clientPasswordInputField;
    [SerializeField] private Button buttonClient;

    [Header("Ready Menu")]
    [SerializeField] private GameObject readyMenu;
    [SerializeField] private InputField inputFieldPlayerName;
    [SerializeField] private Button buttonReady;

    [Header("In-Game HUD")]
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] RawImage[] heartsUI = new RawImage[3];

    [Header("Timer")]
    [SerializeField] TextMeshProUGUI timerText;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
    }

    private void Start()
    {
        goHostMenu.onClick.AddListener(() => ActivateHostMenu());
        goServerMenu.onClick.AddListener(() => ActivateServerMenu());
        goClientMenu.onClick.AddListener(() => ActivateClientMenu());
        goBack.onClick.AddListener(() => ActivateMainMenu());

        
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        buttonReady.onClick.AddListener(() => ActivateInGameHUD());

        ActivateMainMenu();
    }

    #endregion

    #region UI Related Methods

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(false);

        readyMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }
    private void ActivateHostMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(true);

        readyMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }
    private void ActivateServerMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(true);
        clientMenu.SetActive(false);
        backMenu.SetActive(true);

        readyMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }
    private void ActivateClientMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(true);
        backMenu.SetActive(true);

        readyMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }
    private void ActivateReadyMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(false);

        if(!IsServer) readyMenu.SetActive(true);
        inGameHUD.SetActive(false);
    }
    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(false);

        TryGameStart();
        
        readyMenu.SetActive(false);
        inGameHUD.SetActive(true);
    }
    public void TryGameStart()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        GameObject client;

        foreach (GameObject p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                client = p;
                client.GetComponent<Player>().UpdatePlayerIsReadyServerRpc(true);
                client.GetComponent<Player>().UpdatePlayerNameServerRpc(inputFieldPlayerName.text);

                //string name = client.GetComponent<Player>().playerName.Value.ToString();
                //client.GetComponent<PlayerController>().nameRenderer.text = name;
                //print(name);
            }
        }
    }

    //[ServerRpc]
    //public void GameIsReadyServerRpc()
    //{
    //    var players = GameObject.FindGameObjectsWithTag("Player");
    //    foreach (GameObject player in players)
    //    {
    //        print("hey");
    //        player.GetComponent<Player>().UpdateGameReadyServerRpc(true);
    //    }
    //    gameManager.startTimer = true;
    //}

    public void UpdateLifeUI(int hitpoints)
    {
        switch (hitpoints)
        {
            case 6: // 3 corazones completos
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
            case 5: // 2 corazones y medio
                heartsUI[0].texture = hearts[1].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
            case 4: // 2 corazones
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
            case 3: // 1 corazon y medio
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[1].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
            case 2: // 1 corazon
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
            case 1: // medio corazon
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[1].texture;
                break;
            case 0: // muerto
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
        }
    }

    public float UpdateTimeCounter(float timer)
    {
        timer -= Time.deltaTime;
        timerText.text = "" + timer.ToString("f0");
        return timer;
    }
    #endregion

    #region Netcode Related Methods

    private void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        ActivateReadyMenu();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(clientPasswordInputField.text);
        var ip = inputFieldIP.text;
        if (!string.IsNullOrEmpty(ip))
        {
            transport.SetConnectionData(ip, port);
        }
        NetworkManager.Singleton.StartClient();
        ActivateReadyMenu();
    }

    private void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartServer();
        ActivateReadyMenu();
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback) //aprovalcheck de MLAPI
    {
        string password = Encoding.ASCII.GetString(connectionData);
        bool approveConnection = (password == hostPasswordInputField.text) || (password == serverPasswordInputField.text); //determino si la contraseña introducida es la misma que la del servidor
        bool emptyPlaceInGame = NetworkManager.Singleton.ConnectedClients.Count < gameManager.maxPlayers; //determino si el numero de jugadores es menor que el maximo permitido

        Vector3 position = gameManager.spawnPoints[Random.Range(0, gameManager.spawnPoints.Count - 1)].position; //posicion aleatoria del array de spawnpoints del game manager

        callback(emptyPlaceInGame, null, approveConnection, position, null); //callback del delegado ConnectionApprovedDelegate
    }
    #endregion
}
