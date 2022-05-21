using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private Renderer ballRenderer;
    private NetworkVariable<Color> ballColour = new NetworkVariable<Color>();

    private void OnEnable()
    {
        
    }

}
