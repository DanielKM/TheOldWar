using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamHandler : MonoBehaviour
{    
    public SteamCloudPrefs SteamStorage = new SteamCloudPrefs();
    
    public static Action<SteamCloudPrefs> SteamHandlerOnLoaded;
    private bool loaded = false;

    private void Start()
    {
        if (SaveLoadFile.Load() != null)
        {
            SteamStorage = SaveLoadFile.Load();
            PlayerPrefs.SetString ("name", SteamStorage.name);
            PlayerPrefs.SetInt ("wins", SteamStorage.wins);
            PlayerPrefs.SetInt ("losses", SteamStorage.losses);
            PlayerPrefs.SetInt ("kills", SteamStorage.kills);

            PlayerPrefs.SetInt ("gold", SteamStorage.gold);
            PlayerPrefs.SetInt ("iron", SteamStorage.iron);
            PlayerPrefs.SetInt ("steel", SteamStorage.steel);
            PlayerPrefs.SetInt ("skymetal", SteamStorage.skymetal);
            PlayerPrefs.SetInt ("wood", SteamStorage.wood);
            PlayerPrefs.SetInt ("stone", SteamStorage.stone);
            PlayerPrefs.SetInt ("food", SteamStorage.food);
            PlayerPrefs.SetInt ("population", SteamStorage.population);
            PlayerPrefs.SetInt ("armySize", SteamStorage.armySize);

            PlayerPrefs.SetString ("relics", SteamStorage.relics);
            PlayerPrefs.SetString ("unlocks", SteamStorage.unlocks);
            PlayerPrefs.SetString ("rank", SteamStorage.rank);

            PlayerPrefs.Save();
        }
    }

    public void Update()
    {
        if(!loaded) 
        {
            if(SteamHandlerOnLoaded != null)
            {
                SteamHandlerOnLoaded(SteamStorage);
                loaded = true;
            }
        }
    }

    private void OnDestroy()
    {
        // SteamStorage.ammo = LocalStorage.preBatammotleState;

        SaveLoadFile.Save(SteamStorage);
    
    }
}
