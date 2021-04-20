using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] public float damageToDeal = 2.0f;
    [SerializeField] public float savedDamage = 2.0f;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

    public UnitFiring projectileFirer = null;
    UnitType firerUnitType;
    ResourceGatherer resourceGatherer;
    RTSPlayer owner = null;
    Team team = null;

    // void OnEnable()
    // {        
    //     rb.velocity = transform.forward * launchForce;

    //     Invoke(nameof(DestroySelf), destroyAfterSeconds);
    // }

    void Start()
    {
        rb.velocity = transform.forward * launchForce;

        firerUnitType = projectileFirer.gameObject.GetComponent<UnitInformation>().unitType;

        if(firerUnitType == UnitType.Worker)
        {
            resourceGatherer = projectileFirer.gameObject.GetComponent<ResourceGatherer>();
        }

        owner = projectileFirer.gameObject.GetComponent<UnitInformation>().owner;
        team = projectileFirer.gameObject.GetComponent<UnitInformation>().team;

        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            // FOR CONSTRUCTION
            if(firerUnitType == UnitType.Worker && other.TryGetComponent<Foundation>(out Foundation foundation)) 
            { 
                foundation.SetProgress(5);

                return;
            };

            // FOR RESOURCES
            if(other.TryGetComponent<ResourceNode>(out ResourceNode nodeCheck) && nodeCheck.enabled) {

            } else {
                if(firerUnitType == UnitType.Worker && other.TryGetComponent<Building>(out Building building)) 
                { 
                    Health buildingHealth = other.GetComponent<Health>();
                    if(buildingHealth.currentHealth < buildingHealth.maxHealth)
                    {
                        buildingHealth.CmdHealDamage(10);
                    }
                    return;
                };
            }

            if(team == other.gameObject.GetComponent<UnitInformation>().team) { return; }
            // if(networkIdentity.connectionToClient == connectionToClient) { return; }

            if(other.TryGetComponent<Health>(out Health health)) 
            {
                // FOR RESOURCES
                if(firerUnitType == UnitType.Worker &&
                other.TryGetComponent<ResourceNode>(out ResourceNode resourceNode) && resourceNode.enabled) 
                { 
                    if(resourceGatherer.heldResourcesType != resourceNode.GetResourceType())
                    {
                        resourceGatherer.heldResources = 0;
                    }

                    resourceGatherer.AddResources(10, resourceNode.GetResourceType());

                    resourceNode.TakeResources(10);

                    return;
                };
            
                // FOR RESOURCES
                if(firerUnitType == UnitType.Worker &&
                other.TryGetComponent<Corpse>(out Corpse corpse)) 
                { 
                    NetworkServer.Destroy(corpse.gameObject);

                    Destroy(corpse.gameObject);

                    return;
                };

                // FOR ENEMIES
                health.DealDamage( (int)Math.Ceiling(damageToDeal));
            }

            DestroySelf();
        }
    }

    [Server]
    private void DestroySelf() 
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void DeactivateSelf() 
    {
        rb.velocity = Vector3.zero;

        gameObject.SetActive(false);
    }

    // [Server]
    // private void OnDisable() 
    // {
    //     CancelInvoke();
    // }
}
