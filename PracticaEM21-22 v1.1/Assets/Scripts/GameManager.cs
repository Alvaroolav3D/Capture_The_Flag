using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public int maxPlayers;
    public List<Transform> spawnPoints;

    public UIManager uiManager;

    public void Update()
    {
        uiManager.UpdateLifeUI(6);
    }

}

public enum GameMode
{
    DeadMatch = 0,
    TeamDeathMatch = 1,
    CaptureTheFlag = 2
}