using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TeamPlayer : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer teamColourRenderer;
    [SerializeField] private Color[] teamColours;
    //el teamColours[0] queda reservado para un jugador sin equipo
    //los 4 indices siguientes son los equipos y el jugador cambiara de color

    private NetworkVariable<byte> teamId = new NetworkVariable<byte>(); //para que una nertworkvariable funciones necesito que herede de networkBehabiour

    private void OnEnable()
    {
        teamId.OnValueChanged += OnTeamChanged; //le añado al habilitarse que cada vez que se produzca un cambio se active la funcion
    }

    private void OnDisable()
    {
        teamId.OnValueChanged -= OnTeamChanged;
    }

    [ServerRpc]
    public void SetTeamServerRpc(byte newTeamId) //el motivo de pedir un byte es que es mas eficiente que un int y para decidir sobre los pocos equipos que hay no necesito mas (en este caso)
    {
        if(newTeamId > teamColours.Length - 1) { return; } //teamColours.Length - 1 = 4

        teamId.Value = newTeamId;
    }

    private void OnTeamChanged(byte previousTeamId, byte newTeamId)
    {
        teamColourRenderer.color = teamColours[newTeamId];
    }
}
