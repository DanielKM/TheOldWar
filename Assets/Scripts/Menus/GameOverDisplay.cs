using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;
    private RTSPlayer player = null;

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;

        if(connectionToClient == null ) { return; }
        
        // player = connectionToClient.identity.GetComponent<RTSPlayer>();
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {
        SteamSave();

        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else 
        {
            NetworkManager.singleton.StopClient();

            Application.Quit();
        }
    }

    public void SteamSave()
    {
        if (SaveLoadFile.Load() != null)
        {
            SteamCloudPrefs SteamStorage = GameObject.Find("NetworkManager").GetComponent<SteamHandler>().SteamStorage;

            // player = connectionToClient.identity.GetComponent<RTSPlayer>();

            SteamStorage.gold += 200;
            SteamStorage.iron += 200;
            SteamStorage.steel += 200;
            SteamStorage.skymetal += 200;
            SteamStorage.wood += 200;
            SteamStorage.stone += 200;
            SteamStorage.food += 200;
            SteamStorage.armySize += 200;

            SteamStorage.wins += 1;
            // SteamStorage.kills += 34;

            SaveLoadFile.Save(SteamStorage);
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        gameOverDisplayParent.SetActive(true);
    }
}
