using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : MonoBehaviour
{
    public UIManager uIManager;

    public int maxPlayers;
    private bool gameReady = false;
    public List<Transform> spawnPoints;

    public float timer;

    private void Update()
    {
        if(timer > 0)
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