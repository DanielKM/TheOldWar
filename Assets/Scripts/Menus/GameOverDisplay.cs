using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
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

            // SteamStorage = SaveLoadFile.Load();

            SteamStorage.gold += 34;
            SteamStorage.iron += 34;
            SteamStorage.steel += 34;
            SteamStorage.skymetal += 34;
            SteamStorage.wood += 34;
            SteamStorage.stone += 34;
            SteamStorage.food += 34;
            SteamStorage.armySize += 34;

            SteamStorage.wins += 1;
            SteamStorage.kills += 34;

            // PlayerPrefs.SetString ("name", SteamStorage.name);
            // PlayerPrefs.SetInt ("wins", SteamStorage.wins);
            // PlayerPrefs.SetInt ("losses", SteamStorage.losses);
            // PlayerPrefs.SetInt ("kills", SteamStorage.kills);

            // PlayerPrefs.SetInt ("gold", SteamStorage.gold);
            // PlayerPrefs.SetInt ("iron", SteamStorage.iron);
            // PlayerPrefs.SetInt ("steel", SteamStorage.steel);
            // PlayerPrefs.SetInt ("skymetal", SteamStorage.skymetal);
            // PlayerPrefs.SetInt ("wood", SteamStorage.wood);
            // PlayerPrefs.SetInt ("stone", SteamStorage.stone);
            // PlayerPrefs.SetInt ("food", SteamStorage.food);
            // PlayerPrefs.SetInt ("armySize", SteamStorage.armySize);

            // PlayerPrefs.SetString ("relics", SteamStorage.relics);
            // PlayerPrefs.SetString ("unlocks", SteamStorage.unlocks);
            // PlayerPrefs.SetString ("rank", SteamStorage.rank);

            // PlayerPrefs.Save();

            SaveLoadFile.Save(SteamStorage);
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        gameOverDisplayParent.SetActive(true);
    }
}
