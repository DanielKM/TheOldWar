using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] public float damageToDeal = 2.0f;
    [SerializeField] public float savedDamage = 2.0f;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

    public UnitFiring projectileFirer = null;

    // void OnEnable()
    // {        
    //     rb.velocity = transform.forward * launchForce;

    //     Invoke(nameof(DestroySelf), destroyAfterSeconds);
    // }

    void Start()
    {
        rb.velocity = transform.forward * launchForce;

        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            // FOR CONSTRUCTION
            if(projectileFirer.gameObject.GetComponent<UnitInformation>().unitType == UnitType.Worker &&
            other.TryGetComponent<Foundation>(out Foundation foundation)) 
            { 
                foundation.SetProgress(5);

                return;
            };

            if(networkIdentity.connectionToClient == connectionToClient) { return; }

            if(other.TryGetComponent<Health>(out Health health)) 
            {
                // FOR RESOURCES
                if(projectileFirer.gameObject.GetComponent<UnitInformation>().unitType == UnitType.Worker &&
                other.TryGetComponent<ResourceNode>(out ResourceNode resourceNode)) 
                { 
                    projectileFirer.gameObject.GetComponent<ResourceGatherer>().AddResources(10, resourceNode.GetResourceType());

                    resourceNode.TakeResources(10);

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
