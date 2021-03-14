using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuProfileInformation : MonoBehaviour
{
    [SerializeField] private Text profileName;
    [SerializeField] private Text profileRank;
    [SerializeField] private Text profileKills;
    [SerializeField] private Text profileWins;
    [SerializeField] private Text profileLosses;
    [SerializeField] private Text profileUnlocks;
    [SerializeField] private Text profileRelics;

    private void Start()
    {
        SteamHandler.SteamHandlerOnLoaded += HandlePrefsLoaded;
    }

    private void OnDestroy()
    {
        SteamHandler.SteamHandlerOnLoaded -= HandlePrefsLoaded;
    }

    // private void HandlePrefsSaved(SteamHandler steamHandler)
    // {
    //     Debug.Log("SAVED");
    //     profileName.text = steamHandler.SteamStorage.name;
    //     profileRank.text = steamHandler.SteamStorage.rank;
    //     profileKills.text = steamHandler.SteamStorage.kills.ToString();
    //     profileWins.text = steamHandler.SteamStorage.wins.ToString();
    //     profileLosses.text = steamHandler.SteamStorage.losses.ToString();
    //     profileUnlocks.text = steamHandler.SteamStorage.unlocks;
    //     profileRelics.text = steamHandler.SteamStorage.relics;
    // }

    private void HandlePrefsLoaded(SteamCloudPrefs steamStorage)
    {
        Debug.Log("Loaded");
        profileName.text = steamStorage.name;
        profileRank.text = steamStorage.rank;
        profileKills.text = steamStorage.kills.ToString();
        profileWins.text = steamStorage.wins.ToString();
        profileLosses.text = steamStorage.losses.ToString();
        profileUnlocks.text = steamStorage.unlocks;
        profileRelics.text = steamStorage.relics;
    }
}
