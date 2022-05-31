using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public UIManager uIManager;

    public int maxPlayers;
    public List<Transform> spawnPoints;

    public float timer;
    public bool startTimer;

    public int playersReady;

    private void Update()
    {
        UpdateAllPlayersName();
        StartTheGame();

        if (startTimer == true && timer > 0)
        {
            timer = uIManager.UpdateTimeCounter(timer);
        }
        else
        {
            GameEnds();
        }
    }
    private void UpdateAllPlayersName()
    {
        //funcion encargada de actualizar el TMPro de los player para que tengan el mismo valor que el nombre del jugador
        if (startTimer == false)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                string name = player.GetComponent<Player>().playerName.Value.ToString();
                player.GetComponent<PlayerController>().nameRenderer.text = name;
            }
        }
    }

    private void StartTheGame()
    {
        //cambiar en un futuro
        //si la partida no ha empezado comprueba cuantos jugadores estan preparados,
        //en caso de que todos esten listos la partida comenzara asi como el timer
        if (IsOwnedByServer) 
        {
            if (startTimer == false)
            {
                playersReady = 0;
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject p in players)
                {
                    if (p.GetComponent<Player>().isReady.Value == true)
                    {
                        playersReady += 1;
                    }
                }
                if (playersReady == maxPlayers)
                {
                    foreach (GameObject player in players)
                    {
                        player.GetComponent<Player>().UpdateGameReadyServerRpc(true);
                    }
                    startTimer = true;
                }
            }
        }
    }

    private void GameEnds()
    {
        if (startTimer == true)
        {
            //actualizo a cada jugador que la partida ya no esta ready
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().UpdateGameReadyServerRpc(false);
            }
            uIManager.ActivateStatsMenu();

            //desconecto al cliente del servidor
            if (IsOwner)
            {
                NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
            }
        }
    }
}


public enum GameMode
{
    DeadMatch = 0,
    TeamDeathMatch = 1,
    CaptureTheFlag = 2
}