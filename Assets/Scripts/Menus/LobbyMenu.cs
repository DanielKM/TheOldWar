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
    
    [SerializeField] private Button hindegardeStartButton = null;
    [SerializeField] private Button hindegardeDefenceButton = null;
    [SerializeField] private Button mavisButton = null;
    [SerializeField] private Button ruunCityButton = null;
    [SerializeField] private Button swampRunButton = null;

    [SerializeField] private Button[] allButtons = null;

    SteamCloudPrefs steamStorage = null;

    private void Start()
    {
        steamStorage = GameObject.Find("NetworkManager").GetComponent<SteamHandler>().SteamStorage;
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;

        CheckForPlayersAndUpdateLobby();
        CheckForMapUnlocks();
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

        for(int i = 0; i < players.Count; i++)
        {    
            players[i].SetTeam(team);
            players[i].teamMaterial = teamMaterials[i];

            CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(
                            MainMenu.LobbyId, 
                            i
                        );

            string steamName = SteamFriends.GetFriendPersonaName(steamID);

            playerNameTexts[i].text = players[i].GetDisplayName(); 
        }
        
        for(int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
        }

        startGameButton.interactable = players.Count >= 1;

        // CHECK UNLOCKS
    }

    private void CheckForMapUnlocks()
    {
        foreach(Button button in allButtons)
        {
            button.interactable = false;
        }
        
        hindegardeStartButton.interactable = true;

        if(steamStorage.hindegardeStart)
        {
            hindegardeStartButton.interactable = true;
            mavisButton.interactable = true;
        }
        if(steamStorage.mavis)
        {
            hindegardeStartButton.interactable = false;
            mavisButton.interactable = true;
            hindegardeDefenceButton.interactable = true;
        }
        if(steamStorage.hindegardeDefence)
        {
            hindegardeStartButton.interactable = true;
            hindegardeDefenceButton.interactable = false;
            hindegardeDefenceButton.gameObject.SetActive(false);
            mavisButton.interactable = true;
            ruunCityButton.interactable = true;
        }
        if(steamStorage.ruunCity)
        {
            hindegardeStartButton.interactable = true;
            mavisButton.interactable = true;
            ruunCityButton.interactable = true;
            swampRunButton.interactable = true;
        }
        if(steamStorage.swampRun)
        {
            hindegardeStartButton.interactable = true;
            mavisButton.interactable = true;
            ruunCityButton.interactable = true;
            swampRunButton.interactable = true;
        }
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
                levelSelected = "Scene_Map_Starting";
                break;
            case "HindegardeDefence":
                levelSelected = "Scene_Map_Hindegarde";
                break;
            case "Mavis":
                levelSelected = "Scene_Map_Mavis";
                break;
            case "Ruun":
                levelSelected = "Scene_Map_Ruun";
                break;
        }
        levelText.text = level;

        GameObject.Find("NetworkManager").GetComponent<RTSNetworkManager>().SelectLevel(levelSelected);
    }
}
