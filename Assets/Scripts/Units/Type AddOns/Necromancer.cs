using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Necromancer : MonoBehaviour
{
    [Header("References")]
    GameObject EventHandler;
    EventCycle EventCycle;
    UnitTask unitTask;
    GameobjectLists gameobjectLists;
    public GameObject skeleton;
    [SerializeField]
    private GameObject raiseDeadSpell;
    [SerializeField]
    private GameObject raiseDeadCaster;

    [Header("Settings")]
    public bool raisingDead = false;
    private Vector3 spawnPosition;
    
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

    // Start is called before the first frame update
    void Start()
    {
        EventHandler = GameObject.Find("EventHandler");

        EventCycle = EventHandler.GetComponent<EventCycle>();

        unitTask = gameObject.GetComponent<UnitTask>();

        gameobjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();  

        unit = gameObject.GetComponent<Unit>();

        targeter = unit.GetTargeter();

        unitInformation = gameObject.GetComponent<UnitInformation>();

        necromancer = gameObject.GetComponent<Necromancer>();

        unitCommandGiver = GameObject.Find("UnitHandlers").GetComponent<UnitCommandGiver>();
    }

    // Update is called once per frame
    void Update()
    {
        if(unitTask.GetTask() == ActionList.Dead || unitTask.GetTask() == ActionList.Injured) { return; }

        if(EventCycle.time >= 14400 && EventCycle.time <= 16000 ) {
            if(unitTask.GetTask() != ActionList.Dead) {
                TryRaiseDead(EventCycle.days + 1, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 5f), gameObject);    
            }
        }
        
        AttemptToDetectCorpses();
    }

    public void TryRaiseDead(int count, Vector3 spawnPos, GameObject corpse)
    {
        if(!raisingDead) {
            StartCoroutine(RaiseDead(count, spawnPos, corpse));
        }      
    }

    IEnumerator RaiseDead(int number, Vector3 spawnPos, GameObject corpse) {
        raisingDead = true;

        unitTask.SetTask(ActionList.CastingAOE);

        if(corpse != gameObject) { 
            GameObject spellToCast = Instantiate(raiseDeadSpell, corpse.transform.position, corpse.transform.rotation);

            NetworkServer.Spawn(spellToCast);
        }
        GameObject casterEffects = Instantiate(raiseDeadCaster, gameObject.transform.position, gameObject.transform.rotation);

        casterEffects.transform.parent = gameObject.transform;

        NetworkServer.Spawn(casterEffects);

        yield return new WaitForSeconds(10);
        
        RTSPlayer owner = gameObject.GetComponent<UnitInformation>().owner;
        Team team = gameObject.GetComponent<UnitInformation>().team;

        GameObject closestPlayerSpawnPoint = GetClosestEnemyPlayer(owner, gameObject, gameobjectLists.GetAllActivePlayerGameobjects());

        // loadedTransforms[0] = TownCenter;
        for(int i=0; i<number; i++) {
            // skeleton.GetComponent<NPCController>().waypoints = loadedTransforms;
            GameObject raisedSkeleton = Instantiate(skeleton, spawnPos, Quaternion.identity);  

            raisedSkeleton.GetComponent<UnitInformation>().owner = owner;
            raisedSkeleton.GetComponent<UnitInformation>().team = team;

            NetworkServer.Spawn(raisedSkeleton);

            raisedSkeleton.GetComponent<NavMeshAgent>().SetDestination(closestPlayerSpawnPoint.transform.position);

            if(corpse != gameObject) { 
                NetworkServer.Destroy(corpse);

                Destroy(corpse);
            }

            raisingDead = false;
        }
    }

    GameObject GetClosestEnemyPlayer(RTSPlayer player, GameObject necro, List<GameObject> enemies)
    {
        GameObject tMin = null;
        float minDist = 300;
        Vector3 currentPos = necro.transform.position;
        foreach (GameObject enemy in enemies)
        {
            RTSPlayer enemyPlayer = enemy.GetComponent<RTSPlayer>();

            if(enemyPlayer == player) { continue; }

            if(enemyPlayer.spawnPoint) {
                float dist = Vector3.Distance(enemyPlayer.spawnPoint.transform.position, currentPos);
                if (dist < minDist)
                {
                    tMin = enemyPlayer.spawnPoint;
                    minDist = dist;
                }
            }
        }

        return tMin;
    }

    void AttemptToDetectCorpses()
    {
        if(Time.time > checkTime) 
        {
            checkTime = Time.time + period;

            Vector3 center = unit.gameObject.transform.position;

            Collider[] colliders = Physics.OverlapSphere(center, detectionRadius,  1 << 15);

            Collider nearestCollider = null;
            float minSqrDistance = Mathf.Infinity;
            for (int i = 0; i < colliders.Length; i++)
            {
                float sqrDistanceToCenter = (center - colliders[i].transform.position).sqrMagnitude;
                
                if (sqrDistanceToCenter < minSqrDistance)
                {
                    minSqrDistance = sqrDistanceToCenter;

                    nearestCollider = colliders[i];
                }
            }

            if(nearestCollider) 
            {
                TryRaiseDead(1, nearestCollider.transform.position, nearestCollider.gameObject);
            }
        }
    }
}
