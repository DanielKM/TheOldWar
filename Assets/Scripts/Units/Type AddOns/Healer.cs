using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject healProjectile;
    [SerializeField]
    private GameObject healCaster;
    [SerializeField]
    UnitTask unitTask;

    [Header("Settings")]
    [SerializeField]
    Unit unit = null;
    public bool healing = false;

    public float period = 2f;
    private float checkTime = 0f;
    public float detectionRadius = 10;
    public LayerMask unitLayer;

    RTSPlayer owner = null;
    Team team = null;

    // Start is called before the first frame update
    void Start()
    {
        unit = gameObject.GetComponent<Unit>();

        owner = gameObject.GetComponent<UnitInformation>().owner;
        team = gameObject.GetComponent<UnitInformation>().team;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > checkTime) {

            if(unitTask.GetTask() == ActionList.Dead || unitTask.GetTask() == ActionList.Injured) { return; }

            AttemptToDetectInjured();
        }
    }
    
    void AttemptToDetectInjured()
    {
        checkTime = Time.time + period;

        Vector3 center = unit.gameObject.transform.position;

        Collider[] colliders = Physics.OverlapSphere(center, detectionRadius,  1 << 10);

        Collider nearestCollider = null;
        Collider nearestWoundedCollider = null;
        
        float minSqrDistance = Mathf.Infinity;
        float minSqrWoundedDistance = Mathf.Infinity;

        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject.GetComponent<UnitInformation>().team != team) { continue; }

            float sqrDistanceToCenter = (center - colliders[i].transform.position).sqrMagnitude;
            
            if(colliders[i].gameObject.GetComponent<UnitTask>().GetTask() == ActionList.Injured) 
            { 
                if (sqrDistanceToCenter < minSqrDistance)
                {
                    minSqrDistance = sqrDistanceToCenter;

                    nearestCollider = colliders[i];
                }
            } else if(colliders[i].gameObject.GetComponent<Health>().currentHealth < colliders[i].gameObject.GetComponent<Health>().maxHealth) 
            {
                if (sqrDistanceToCenter < minSqrDistance)
                {
                    minSqrWoundedDistance = sqrDistanceToCenter;

                    nearestWoundedCollider = colliders[i];
                }
            }
        }

        if(nearestCollider) 
        {
            TryHealInjured(1, nearestCollider.transform.position, nearestCollider.gameObject);
        } else if(nearestWoundedCollider) 
        {
            TryHealWounded(1, nearestWoundedCollider.transform.position, nearestWoundedCollider.gameObject);
        }
    }

    public void TryHealInjured(int count, Vector3 spawnPos, GameObject injured)
    {
        if(!healing) {
            StartCoroutine(HealInjured(count, spawnPos, injured));
        }      
    }

    public void TryHealWounded(int count, Vector3 spawnPos, GameObject injured)
    {
        if(!healing) {
            StartCoroutine(HealWounded(count, spawnPos, injured));
        }      
    }
    
    IEnumerator HealInjured(int number, Vector3 spawnPos, GameObject injured) {
        healing = true;

        unitTask.SetTask(ActionList.CastingAOE);

        if(injured != gameObject) 
        { 
            GameObject spellToCast = Instantiate(healProjectile, injured.transform.position, injured.transform.rotation);

            NetworkServer.Spawn(spellToCast);
        }

        GameObject casterEffects = Instantiate(healCaster, gameObject.transform.position, gameObject.transform.rotation);

        casterEffects.transform.parent = gameObject.transform;

        NetworkServer.Spawn(casterEffects);

        yield return new WaitForSeconds(10);

        if(injured) 
        {
            injured.GetComponent<Health>().CmdHealDamage(10);
        }
        
        healing = false;
    }
    
    IEnumerator HealWounded(int number, Vector3 spawnPos, GameObject injured) {
        healing = true;

        unitTask.SetTask(ActionList.CastingAOE);

        if(injured != gameObject) { 
            GameObject spellToCast = Instantiate(healProjectile, injured.transform.position, injured.transform.rotation);

            NetworkServer.Spawn(spellToCast);
        }

        GameObject casterEffects = Instantiate(healCaster, gameObject.transform.position, gameObject.transform.rotation);

        casterEffects.transform.parent = gameObject.transform;

        NetworkServer.Spawn(casterEffects);

        yield return new WaitForSeconds(1);

        if(injured) 
        {
            injured.GetComponent<Health>().CmdHealDamage(10);
        }
        
        healing = false;
    }
}
