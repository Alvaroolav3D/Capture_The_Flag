using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TeamPicker : MonoBehaviour
{
    public void SelectTeam(int teamId)
    {
        //variable utilizada para selecciona al cliente con esta id
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        var players = GameObject.FindGameObjectsWithTag("Player");

        GameObject client;

        foreach (GameObject p in players)
        {
            if (p.GetComponent<NetworkObject>().OwnerClientId == localClientId)
            {
                client = p;
                client.GetComponent<TeamPlayer>().SetTeamServerRpc((byte)teamId); //envio un mensaje al servidor con el equipo del jugador con esta id
            }
        }

        ////si no encuentra a ningun jugador con esta id devuelve false, sino true y obtengo el networkclient
        //if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out NetworkClient networkClient))  //problema no se puede acceder a connectedclients siendo cliente
        //{
        //    return;
        //}
        ////si fallo en conseguir este componente del personaje devuelvo false, sino true y pbyengo el teamPlayer
        //if (networkClient.PlayerObject.TryGetComponent<TeamPlayer>(out TeamPlayer teamPlayer)) 
        //{
        //    return;
        //}
    }
}
