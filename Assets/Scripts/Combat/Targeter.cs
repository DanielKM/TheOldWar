﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Entirely server-side

public class Targeter : NetworkBehaviour
{
    public Targetable target;
    public Targetable resourceTarget;
    public RTSPlayer player = null;
    private GameobjectLists gameObjectLists;

    public void Start()
    {        
        player = connectionToClient != null ? connectionToClient.identity.GetComponent<RTSPlayer>() : null;

        gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();
    }

    public Targetable GetTarget()
    {
        return target;
    }

    public Targetable GetResourceTarget()
    {
        return resourceTarget;
    }

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Server]
    public void TargetClosestDropOff() 
    {
        target = GetClosestDropOff();
    }
    
    public Targetable GetClosestDropOff()
    {
        Targetable closestDropOff = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        resourceTarget = target;

        ClearTarget(); 

        foreach(Building building in player.GetMyBuildings())
        {
            if(building.TryGetComponent<ResourceDropOff>(out ResourceDropOff resourceDropOff)) 
            {
                Vector3 direction = building.transform.position - position;

                float distance = direction.sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    closestDropOff = building.GetComponent<Targetable>();
                }
            }
        }

        return closestDropOff;
    }

    [Server]
    public void TargetClosestCorpse() 
    {
        target = GetClosestCorpse();
    }

    [Server]
    public void TargetClosestFoundation() 
    {
        target = GetClosestFoundation();
    }

    public Targetable GetClosestCorpse()
    {
        Targetable closestCorpse = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        ClearTarget(); 

        foreach(Unit unit in gameObjectLists.GetAllActiveUnitGameobjects())
        {
            if(unit.TryGetComponent<Corpse>(out Corpse corpse)) 
            {
                Vector3 direction = unit.transform.position - position;

                float distance = direction.sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    closestCorpse = unit.GetComponent<Targetable>();
                }
            }
        }

        return closestCorpse;
    }

    public Targetable GetClosestFoundation()
    {
        Targetable closestFoundation = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        ClearTarget(); 

        foreach(Building building in player.GetMyBuildings())
        {
            if(building.TryGetComponent<Foundation>(out Foundation foundation)) 
            {
                Vector3 direction = building.transform.position - position;

                float distance = direction.sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    closestFoundation = building.GetComponent<Targetable>();
                }
            }
        }

        return closestFoundation;
    }

    [Server]
    public void TargetClosestRepairBuilding() 
    {
        target = GetClosestDamagedBuilding();
    }

    [Server]
    public void TargetClosestResource(int resourceID) 
    {
        target = GetClosestResource(resourceID);
    }

    public Targetable GetClosestDamagedBuilding()
    {
        Targetable closestDamagedBuilding = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        ClearTarget(); 

        foreach(Building building in player.GetMyBuildings())
        {
            if(building.TryGetComponent<Health>(out Health health)) 
            {
                if(health.currentHealth < health.maxHealth) 
                {
                    Vector3 direction = building.transform.position - position;

                    float distance = direction.sqrMagnitude;

                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        
                        closestDamagedBuilding = building.GetComponent<Targetable>();
                    }
                }
            }
        }

        return closestDamagedBuilding;
    }

    public Targetable GetClosestResource(int resourceID)
    {
        Targetable closestResourceNode = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        ClearTarget(); 

        foreach(ResourceNode node in gameObjectLists.GetAllActiveResourceNodes())
        {
            if(node.TryGetComponent<ResourceNode>(out ResourceNode resourceNode)) 
            {
                if(resourceNode.enabled) 
                {
                    int nodeID = GetResourceID(resourceNode.GetResourceType());

                    if(nodeID != resourceID) { continue; }

                    Vector3 direction = node.transform.position - position;

                    float distance = direction.sqrMagnitude;

                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        
                        closestResourceNode = node.GetComponent<Targetable>();
                    }
                }
            }
        }

        return closestResourceNode;
    }
    
    public int GetResourceID(Resource selectedResource)
    {
        switch (selectedResource)
        {
            case Resource.Gold:
                return 0;
            case Resource.Iron:
                return 1;
            case Resource.Steel:
                return 2;
            case Resource.Skymetal:
                return 3;
            case Resource.Wood:
                return 4;
            case Resource.Stone:
                return 5;
            case Resource.Food:
                return 6;
            case Resource.Population:
                return 7;
            default:
                break;
        }
        return 0;
    }

    // [Server]
    public void ClearTarget() 
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }
    
    [Server]
    public void ServerSetTarget(GameObject targetGameObject)
    {   
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }   

    [Server]
    public void ServerSetResourceTarget(GameObject targetGameObject)
    {   
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;

        gameObject.GetComponent<UnitTask>().SetTask(ActionList.Gathering);
    }

    [Server]
    public void ServerSetResourceResourceDropOffTarget(GameObject targetGameObject)
    {   
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }

    // [Command(ignoreAuthority = true)]
    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetCorpseTarget() 
    {
        TargetClosestCorpse();
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetFoundationTarget() 
    {
        TargetClosestFoundation();
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetRepairTarget() 
    {
        TargetClosestRepairBuilding();
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetClosestResourceTarget(int targetResource) 
    {
        TargetClosestResource(targetResource);
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject) 
    {
        ServerSetTarget(targetGameObject);
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetResourceTarget(GameObject targetGameObject) 
    {
        ServerSetResourceTarget(targetGameObject);
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdSetResourceDropOffTarget(GameObject targetGameObject) 
    {
        ServerSetResourceResourceDropOffTarget(targetGameObject);
    }
}
