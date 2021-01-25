using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RemoveComputerPlayerButton : MonoBehaviour, IPointerDownHandler
{    
    [SerializeField]
    private RTSNetworkManager RTSNetworkManager = null;
    
    [SerializeField]
    private LobbyMenu lobby = null;

    [SerializeField]
    private GameObject computerPlayer;

    private RTSPlayer player;

    // SYSTEM VARIABLES
    public bool testing;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        RemoveComputerPlayer();
    }

    public void RemoveComputerPlayer()
    {        
        
        foreach(RTSPlayer player in RTSNetworkManager.Players)  
        {
            if(player.GetDisplayName() == "Computer AI")
            {
                RTSNetworkManager.Players.Remove(player);
                Destroy(player.gameObject);
                lobby.ClientHandleInfoUpdated();
            }
        }

        // DontDestroyOnLoad(instantiatedComputerPlayer);

    }
}
