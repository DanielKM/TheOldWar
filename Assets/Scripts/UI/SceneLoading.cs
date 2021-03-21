using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoading : MonoBehaviour
{
    [SerializeField]
    private Image progressBar;
    
    // Start is called before the first frame update
    void Start()
    {
        string level = GameObject.Find("NetworkManager").GetComponent<RTSNetworkManager>().selectedLevel;

        Debug.Log(level);
        
        ((RTSNetworkManager)NetworkManager.singleton).StartGame(level);
    }
}
