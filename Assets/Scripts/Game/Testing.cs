using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] public bool testing = false;
    [SerializeField] GameObject networkManagerGameObject = null;

    RTSNetworkManager networkManager = null;

    void Start()
    {
        if(testing)
        {
            InstantiateNewNetworkManager();
        }
    }

    void InstantiateNewNetworkManager()
    {
        Instantiate(networkManagerGameObject);

        networkManagerGameObject.GetComponent<RTSNetworkManager>().StartHost();
    }
}
