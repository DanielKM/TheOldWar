using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private int maxResourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;

    [SerializeField] private Resource[] resourceTypes = null;
    [SerializeField] bool onSpawnResource = false;

    [SerializeField] private Resource[] maxResourceTypes = null;
    [SerializeField] bool onSpawnMaxResource = false;

    private float timer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = gameObject.GetComponent<UnitInformation>().owner;

        GameOverHandler.ServerOnGameOver += ServerHandlerGameOver;
    }
    
    void OnEnable()
    {        
        player = gameObject.GetComponent<UnitInformation>().owner;
    }

    public void Start()
    {
        player = gameObject.GetComponent<UnitInformation>().owner;
        if(onSpawnResource)
        {
            Dictionary<Resource, int> newResourceDictionary = player.GetResources();
            for(int i=0; i<resourceTypes.Length; i++) 
            {
                newResourceDictionary[resourceTypes[i]] += resourcesPerInterval;
            }
            player.SetResources(newResourceDictionary);
        }
        if(onSpawnMaxResource)
        {
            Dictionary<Resource, int> newResourceDictionary = player.GetMaxResources();
            for(int i=0; i<maxResourceTypes.Length; i++) 
            { 
                newResourceDictionary[maxResourceTypes[i]] += maxResourcesPerInterval;
            }
            player.SetMaxResources(newResourceDictionary);
        }
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandlerGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        if(!onSpawnResource)
        {
            timer -= Time.deltaTime;

            if(timer <= 0)
            {
                player = gameObject.GetComponent<UnitInformation>().owner;
                
                Dictionary<Resource, int> newResourceDictionary = player.GetResources();

                for(int i=0; i<resourceTypes.Length; i++) 
                {
                    newResourceDictionary[resourceTypes[i]] += resourcesPerInterval;
                }
                player.SetResources(newResourceDictionary);

                timer += interval;
            }
        }
    }

    public void ServerHandlerGameOver()
    {
        enabled = false;
    }
}
