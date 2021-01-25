using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceGatherer : NetworkBehaviour
{
    public Resource[] allowedResources;
    public Resource heldResourcesType;
    public int maxHeldResources;
    public int heldResources;
    public float gatherSpeed;

    private UnitSelectionHandler unitSelection = null;

    [Server]
    public void AddResources(int resourceAmount, Resource resourceType) 
    {
        unitSelection = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();

        if(heldResourcesType != resourceType) 
        { 
            heldResources = 0; 
        }

        heldResourcesType = resourceType;

        if(heldResources == maxHeldResources) { return; }

        heldResources = heldResources + resourceAmount;

        if(heldResources <= 0) { heldResources = 0; }

        if(gameObject.GetComponent<UnitInformation>().selected == false) { return; }
        
        unitSelection.UpdateUnitPanel(gameObject.GetComponent<Unit>());
    }
    
    [Server]
    public int ReturnResources() 
    {
        return heldResources;
    }
}