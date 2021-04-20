using System;
using System.Collections;
using System.Collections.Generic;
using FoW;
using Mirror;
using UnityEngine;

public class Rescuable : MonoBehaviour
{
    UnitInformation unitInformation = null;
    Targeter targeter = null;


    public float period = 2f;
    private float checkTime = 0f;
    public float detectionRadius = 5;
    public LayerMask unitLayer;

    public bool rescued = false;
    // Start is called before the first frame update
    void Start()
    {
        // GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>().units.Add(this.gameObject.GetComponent<Unit>());

        unitInformation = gameObject.GetComponent<UnitInformation>();
        targeter = gameObject.GetComponent<Targeter>();
    }

    void OnDestroy()
    {
        // GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>().units.Remove(this.gameObject.GetComponent<Unit>());
    }

    public void Update()
    {
        if(rescued) { return; }

        AttemptToDetectFriendlies();
    }

    private void AttemptToDetectFriendlies()
    { 
        if(Time.time > checkTime) 
        {
            checkTime = Time.time + period;

            Vector3 center = gameObject.transform.position;

            Collider[] colliders = Physics.OverlapSphere(center, detectionRadius, unitLayer);
            
            for (int i = 0; i < colliders.Length; i++)
            {
                if(colliders[i].gameObject.GetComponent<UnitInformation>() == null) { continue; }

                UnitInformation targetUnitInformation = colliders[i].gameObject.GetComponent<UnitInformation>();

                // if(targetUnitInformation.team == unitInformation.team) { continue; }

                if(colliders[i].gameObject.GetComponent<UnitInformation>().connectionToClient != null)
                {
                    RTSPlayer targetPlayer = targetUnitInformation.owner;

                    Dictionary<Resource, int> newResourceDictionary = targetPlayer.GetResources();

                    // Add army size price
                    newResourceDictionary[Resource.ArmySize] += 1;

                    // Add population price
                    newResourceDictionary[Resource.Population] += 1;
                    
                    targetPlayer.SetResources(newResourceDictionary);

                    NetworkIdentity targetNetworkId = gameObject.GetComponent<NetworkIdentity>();

                    targetNetworkId.AssignClientAuthority(colliders[i].gameObject.GetComponent<UnitInformation>().connectionToClient);

                    unitInformation.owner = targetPlayer;

                    unitInformation.team = targetUnitInformation.team;

                    gameObject.GetComponent<Targeter>().player = targetPlayer;

                    gameObject.GetComponent<UnitMovement>().player = targetPlayer;

                    gameObject.GetComponent<FogOfWarUnit>().enabled = true;

                    targetPlayer.myUnits.Add(gameObject.GetComponent<Unit>());

                    rescued = true;
                   
                   targeter.ClearTarget();

                   this.enabled = false;
                }
            }
        }
    }
}
