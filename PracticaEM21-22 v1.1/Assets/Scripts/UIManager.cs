using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Text;
using TMPro;

public class UIManager : MonoBehaviour
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

    [Header("In-Game HUD")]
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] RawImage[] heartsUI = new RawImage[3];

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

        inGameHUD.SetActive(false);
    }

    private void ActivateHostMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(true);

        inGameHUD.SetActive(false);
    }

    private void ActivateServerMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(true);
        clientMenu.SetActive(false);
        backMenu.SetActive(true);

        inGameHUD.SetActive(false);
    }
    private void ActivateClientMenu()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(true);
        backMenu.SetActive(true);

        inGameHUD.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        serverMenu.SetActive(false);
        clientMenu.SetActive(false);
        backMenu.SetActive(false);

        inGameHUD.SetActive(true);

        // for test purposes
        UpdateLifeUI(Random.Range(1, 6)); //aqui tendria que meter la vida que tiene el player en cuestion
    }

    public void UpdateLifeUI(int hitpoints)
    {
        switch (hitpoints)
        {
            case 6:
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 5:
                heartsUI[0].texture = hearts[1].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 4:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 3:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[1].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 2:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 1:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[1].texture;
                break;
        }
    }

    #endregion

    #region Netcode Related Methods

    private void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        ActivateInGameHUD();
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
        ActivateInGameHUD();
    }

    private void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartServer();
        ActivateInGameHUD();
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
