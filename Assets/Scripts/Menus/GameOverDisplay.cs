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
        
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
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

            SceneManager.LoadScene("Scene_Menu_Import");
        }
    }

    public void SteamSave()
    {
        if (SaveLoadFile.Load() != null)
        {
            SteamCloudPrefs SteamStorage = GameObject.Find("NetworkManager").GetComponent<SteamHandler>().SteamStorage;

            SteamStorage.gold += player.gold;
            SteamStorage.iron += player.iron;
            SteamStorage.steel += player.steel;
            SteamStorage.skymetal += player.skymetal;
            SteamStorage.wood += player.wood;
            SteamStorage.stone += player.stone;
            SteamStorage.food += player.food;
            SteamStorage.armySize += player.population;

            SteamStorage.wins += 1;
            // SteamStorage.kills += 34;

            SaveLoadFile.Save(SteamStorage);
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        // gameOverDisplayParent.SetActive(true);
    }
}
