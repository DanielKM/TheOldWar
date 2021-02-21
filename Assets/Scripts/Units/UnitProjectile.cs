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

        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            // FOR CONSTRUCTION
            if(firerUnitType == UnitType.Worker &&
            other.TryGetComponent<Foundation>(out Foundation foundation)) 
            { 
                foundation.SetProgress(5);

                return;
            };

            if(owner == other.gameObject.GetComponent<UnitInformation>().owner) { return; }
            // if(networkIdentity.connectionToClient == connectionToClient) { return; }

            // if(gam)

            if(other.TryGetComponent<Health>(out Health health)) 
            {
                // FOR RESOURCES
                if(firerUnitType == UnitType.Worker &&
                other.TryGetComponent<ResourceNode>(out ResourceNode resourceNode) && resourceNode.enabled) 
                { 
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
