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
    GameObject unitHandlers;

    [SerializeField]
    public Health health;
    
    public Resource GetResourceType()
    {
        return resource;
    }

    public void Start()
    {
        unitHandlers = GameObject.Find("UnitHandlers");
        
        unitHandlers.GetComponent<GameobjectLists>().resourceNodes.Add(this);
    }

    [Server]
    public void TakeResources(int resourceAmount) 
    {
        if(heldResources == 0) { return; }

        heldResources = heldResources - resourceAmount;

        heldResources = heldResources <= 0 ? 0 : heldResources;

        if(heldResources <= 6000 && gameObject.TryGetComponent<TreeFall>(out TreeFall treefall)) 
        { 
            treefall.ExplodeTree();
        }

        if(heldResources > 0) { return; }

        unitHandlers.GetComponent<GameobjectLists>().resourceNodes.Remove(this);

        NetworkServer.Destroy(gameObject);

        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        // unitHandlers.GetComponent<GameobjectLists>().resourceNodes.Remove(this);
    }
}