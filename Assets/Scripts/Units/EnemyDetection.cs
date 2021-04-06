using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDetection : MonoBehaviour
{
    public LayerMask layerMask;
    
    [SerializeField]
    Unit unit = null;
    Targeter targeter = null;
    UnitInformation unitInformation = null;
    UnitCommandGiver unitCommandGiver = null;
    Necromancer necromancer = null;

    public float period = 2f;
    private float checkTime = 0f;
    public float detectionRadius = 10;
    public LayerMask unitLayer;

    public void Start()
    {
        targeter = unit.GetTargeter();

        unitInformation = unit.GetComponent<UnitInformation>();

        necromancer = unit.GetComponent<Necromancer>();

        unitCommandGiver = GameObject.Find("UnitHandlers").GetComponent<UnitCommandGiver>();
    }

    public void Update()
    {
        AttemptToDetectEnemies();
    }
    
    void AttemptToDetectEnemies()
    {
        if(Time.time > checkTime) 
        {
            checkTime = Time.time + period;
            // if(targeter.target != null) { return; }

            Vector3 center = unit.gameObject.transform.position;

            Collider[] colliders = Physics.OverlapSphere(center, detectionRadius, unitLayer);
            
            Collider nearestCollider = null;
            float minSqrDistance = Mathf.Infinity;
            for (int i = 0; i < colliders.Length; i++)
            {
                if(colliders[i].gameObject.GetComponent<UnitInformation>() == null) { continue; }

                if(colliders[i].gameObject.GetComponent<UnitInformation>().team == unitInformation.team) { continue; }

                if(colliders[i].TryGetComponent<Targetable>(out Targetable target))
                {
                    if(colliders[i].gameObject.TryGetComponent<Health>(out Health health)) 
                    {
                        if(colliders[i].gameObject.GetComponent<Health>().currentHealth <= 0) { continue; }

                        if(colliders[i].TryGetComponent<Rescuable>(out Rescuable rescuable)) { continue; }

                        float sqrDistanceToCenter = (center - colliders[i].transform.position).sqrMagnitude;
                        
                        if (sqrDistanceToCenter < minSqrDistance)
                        {
                            minSqrDistance = sqrDistanceToCenter;

                            nearestCollider = colliders[i];

                        }
                    } 
                }
            }
            if(nearestCollider) 
            {
                targeter.CmdSetTarget(nearestCollider.gameObject);
            }
        }
    }
  
    #region Server

    #endregion

    #region Client

    #endregion
}
