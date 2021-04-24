using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChatDetection : MonoBehaviour
{      
    [SerializeField]
    Unit unit = null;
    [SerializeField]
    GameObject chatBubble = null;
    [SerializeField]
    int chatDuration = 5;
    UnitInformation unitInformation = null;
    UnitCommandGiver unitCommandGiver = null;

    public float period = 2f;
    private float checkTime = 0f;
    public float detectionRadius = 10;
    public LayerMask unitLayer;

    private bool messageSent = false;

    public void Start()
    {
        unitInformation = unit.GetComponent<UnitInformation>();

        unitCommandGiver = GameObject.Find("UnitHandlers").GetComponent<UnitCommandGiver>();
    }

    public void Update()
    {
        if(messageSent) { return; }

        AttemptToDetectUnits();
    }
    
    void AttemptToDetectUnits()
    {
        if(Time.time > checkTime) 
        {
            checkTime = Time.time + period;
            
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

                        if(colliders[i].TryGetComponent<Rescuable>(out Rescuable rescuable) && rescuable.rescued == false) { continue; }

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
                StartCoroutine(Chat(chatDuration));
                // targeter.CmdSetTarget(nearestCollider.gameObject);
            }
        }
    }
  
    private IEnumerator Chat(int messageLength)
    {
        chatBubble.SetActive(true);
        messageSent = true;
        yield return new WaitForSeconds(messageLength);
        gameObject.transform.parent.GetComponent<NavMeshAgent>().ResetPath();
        chatBubble.SetActive(false);
    }
    #region Server

    #endregion

    #region Client

    #endregion
}
