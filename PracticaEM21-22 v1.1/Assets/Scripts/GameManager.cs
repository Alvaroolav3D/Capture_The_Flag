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
        if (IsOwnedByServer) //cambiar en un futuro
        {
            if (startTimer == false)
            {
                print(playersReady);
                playersReady = 0;
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject p in players)
                {
                    if (p.GetComponent<Player>().isReady.Value == true)
                    {
                        playersReady += 1;
                    }
                }
                if(playersReady == maxPlayers)
                {
                    foreach (GameObject player in players)
                    {
                        player.GetComponent<Player>().UpdateGameReadyServerRpc(true);
                    }
                    startTimer = true;
                }
            }
        }

        if (startTimer == true && timer > 0)
        {
            timer = uIManager.UpdateTimeCounter(timer);
        }
        else
        {

        }
    }
}


public enum GameMode
{
    DeadMatch = 0,
    TeamDeathMatch = 1,
    CaptureTheFlag = 2
}