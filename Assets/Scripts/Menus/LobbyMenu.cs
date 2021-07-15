using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : NetworkBehaviour
{
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private Text[] playerNameTexts = new Text[6];
    [SerializeField] private Team team = null;
    [SerializeField] private Material[] teamMaterials = null;

    [SerializeField] private string levelSelected = null;
    [SerializeField] private Text levelText = null;
    [SerializeField] private Text levelTextExplanation = null;
    
    [SerializeField] private Button hindegardeStartButton = null;
    [SerializeField] private Button hindegardeDefenceButton = null;
    [SerializeField] private Button hadrigalButton = null;
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
        // lobbyUI.SetActive(true);
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
            hadrigalButton.interactable = true;
        }
        if(steamStorage.hadrigal)
        {
            hindegardeStartButton.interactable = false;
            hadrigalButton.interactable = true;
            hindegardeDefenceButton.interactable = true;
        }
        if(steamStorage.hindegardeDefence)
        {
            hindegardeStartButton.interactable = true;
            hindegardeDefenceButton.interactable = false;
            hindegardeDefenceButton.gameObject.SetActive(false);
            hadrigalButton.interactable = true;
            ruunCityButton.interactable = true;
        }
        if(steamStorage.ruunCity)
        {
            hindegardeStartButton.interactable = true;
            hadrigalButton.interactable = true;
            ruunCityButton.interactable = true;
            swampRunButton.interactable = true;
        }
        if(steamStorage.swampRun)
        {
            hindegardeStartButton.interactable = true;
            hadrigalButton.interactable = true;
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
        string levelExplanation = "";
        switch (level)
        {
            case "Hindegarde":
                levelSelected = "Scene_Map_Starting";
                levelExplanation = "Hindegarde is the last lonely town between Ruun and the World Spine. Bolster your forces by gathering allies around town. Also, rid the town of Gavrol, the man-eating boar that has been terrorizing the citizens.";
                break;
            case "Hadrigal":
                levelSelected = "Scene_Map_Hadrigal";
                levelExplanation = "There have been rumours of magic users in Hadrigal, the town nearest to Hindegarde. They may have something to do with the risen dead that attacked our town. Go talk to Mavis, their town elder and find out if the rumours are true";
                break;
            case "HindegardeDefence":
                levelSelected = "Scene_Map_Hindegarde";
                levelExplanation = "Hindegarde has been overrun by the undead! Many have already been slain! Go and take back the town. Slay their leader, the giant brute Etgas.";
                break;
            case "Ruun":
                levelSelected = "Scene_Map_Ruun";
                levelExplanation = "You must go to Ruun City, the seat of the Andarian Empire. There you must warn them of the attacks by the undead.";
                break;
        }
        levelText.text = level;
        levelTextExplanation.text = levelExplanation;

        GameObject.Find("NetworkManager").GetComponent<RTSNetworkManager>().SelectLevel(levelSelected);
    }
}
