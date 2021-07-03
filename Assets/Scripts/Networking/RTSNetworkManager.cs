using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    // [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;
    [SerializeField] public string selectedLevel = "Scene_Map_Starting";
    public GameobjectLists gameObjectLists;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public bool isGameInProgress = false;

    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame(string sceneName)
    {
        if(Players.Count <1 ) { return; }

        isGameInProgress = true;

        ServerChangeScene(sceneName);
    }

    // [Server]
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(MainMenu.LobbyId, numPlayers - 1);
        
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        PlayerInfoDisplay playerInfoDisplay = conn.identity.GetComponent<PlayerInfoDisplay>();

        playerInfoDisplay.SetSteamId(steamId.m_SteamID);

        Players.Add(player);

        string steamName = SteamFriends.GetFriendPersonaName(steamId);

        player.SetDisplayName(steamName);
        // player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        ));

        player.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            // GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            // NetworkServer.Spawn(gameOverHandlerInstance.gameObject); Dan Here

            gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();

            foreach(RTSPlayer player in Players) 
            {
                Transform startTransform = GetStartPosition();
                Vector3 startPos = startTransform.position;

                GameObject baseInstance = Instantiate(
                    unitBasePrefab, 
                    startPos, 
                    Quaternion.identity);

                baseInstance.GetComponent<UnitInformation>().owner = player;
                
                Vector3 baseOffset = new Vector3(0.0f, 0.0f, 20f);
                player.gameObject.transform.position = startPos - baseOffset;
                player.spawnPoint = startTransform.gameObject;
                

                Dictionary<Resource, int> newResourceDictionary = player.GetResources();

                ResourceGatherer gatherer = gameObject.GetComponent<ResourceGatherer>();

                SteamCloudPrefs SteamStorage = gameObject.GetComponent<SteamHandler>().SteamStorage;
                // PULL FROM PREFS
                newResourceDictionary[Resource.ArmySize] += SteamStorage.armySize;
                
                player.SetResources(newResourceDictionary);
                
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
                
                gameObjectLists.players.Add(player.gameObject);
            }
        }
    }

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    public void SelectLevel(string level)
    {
        selectedLevel = level;
    }

    #endregion
}
