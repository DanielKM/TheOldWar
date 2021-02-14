using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceNode : NetworkBehaviour
{
    public Resource resource = Resource.None;
    public int heldResources;

    [SerializeField]
    private Health health;
    
    public Resource GetResourceType()
    {
        return resource;
    }

    public void Start()
    {
        GameObject x = GameObject.Find("UnitHandlers");
        
        x.GetComponent<GameobjectLists>().resourceNodes.Add(this);
    }

    [Server]
    public void TakeResources(int resourceAmount) 
    {
        if(heldResources == 0) { return; }

        heldResources = heldResources - resourceAmount;

        heldResources = heldResources <= 0 ? 0 : heldResources;

        if(heldResources > 0) { return; }

        health.DealDamage(10000);
    }
}