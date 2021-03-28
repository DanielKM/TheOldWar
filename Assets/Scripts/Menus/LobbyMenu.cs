using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private Text[] playerNameTexts = new Text[6];
    [SerializeField] private Team team = null;
    [SerializeField] private Material[] teamMaterials = null;

    [SerializeField] private string levelSelected = null;
    [SerializeField] private Text levelText = null;

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;

        CheckForPlayersAndUpdateLobby();
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    public void ClientHandleInfoUpdated()
    {
        CheckForPlayersAndUpdateLobby();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    private void CheckForPlayersAndUpdateLobby()
    {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;

        // CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(
        //                 MainMenu.LobbyId, 
        //                 players.Count - 1
        //             );

        // string steamName = SteamFriends.GetFriendPersonaName(steamID);

        for(int i = 0; i < players.Count; i++)
        {    
            players[i].SetTeam(team);
            players[i].teamMaterial = teamMaterials[i];

            CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(
                            MainMenu.LobbyId, 
                            i
                        );

            string steamName = SteamFriends.GetFriendPersonaName(steamID);

            // players[i].SetDisplayName(steamName); change

            // playerNameTexts[i].text = steamName; change 
            playerNameTexts[i].text = players[i].GetDisplayName(); 
        }
        
        for(int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
        }

        startGameButton.interactable = players.Count >= 1;
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected) 
        {
            NetworkManager.singleton.StopHost();
        }
        else 
        {
            NetworkManager.singleton.StopClient();
        }
    }

    public void SelectLevel(string level)
    {
        switch (level)
        {
            case "Hindegarde":
                levelSelected = "Scene_Map_01";
                break;
            case "Frozen Tundra":
                levelSelected = "Scene_Map_01";
                break;
        }
        levelText.text = level;

        GameObject.Find("NetworkManager").GetComponent<RTSNetworkManager>().SelectLevel(levelSelected);
    }
}
