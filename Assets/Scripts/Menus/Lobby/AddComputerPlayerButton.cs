using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AddComputerPlayerButton : MonoBehaviour, IPointerDownHandler
{    
    [SerializeField]
    private RTSNetworkManager RTSNetworkManager = null;
    
    [SerializeField]
    private LobbyMenu lobby = null;

    [SerializeField]
    private GameObject computerPlayer;

    private RTSPlayer player;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }
        AddComputerPlayer();
    }

    public void AddComputerPlayer()
    {
        GameObject instantiatedComputerPlayer = Instantiate(computerPlayer);

        RTSNetworkManager.Players.Add(instantiatedComputerPlayer.GetComponent<RTSPlayer>());

        DontDestroyOnLoad(instantiatedComputerPlayer);

        lobby.ClientHandleInfoUpdated();
    }
}
