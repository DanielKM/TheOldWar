using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoadFile
{
    private const string FILENAME = "/SteamCloud_BattleForMurk.sav";

    public static void Save(SteamCloudPrefs steamCloudPrefs)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + FILENAME, FileMode.Create);

        bf.Serialize(stream, steamCloudPrefs);
        stream.Close();
    }

    public static SteamCloudPrefs Load()
    {
        if(File.Exists(Application.persistentDataPath + FILENAME))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + FILENAME, FileMode.Open);

            SteamCloudPrefs data = bf.Deserialize(stream) as SteamCloudPrefs;

            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("File not found.");
            return null;
        }
    }
}