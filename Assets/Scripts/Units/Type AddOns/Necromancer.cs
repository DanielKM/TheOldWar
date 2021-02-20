using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Necromancer : MonoBehaviour
{
    GameObject EventHandler;
    DayNightCycle DayNight;
    UnitTask unitTask;
    GameobjectLists gameobjectLists;

    public GameObject skeleton;

    public bool raisingDead = false;
    private Vector3 spawnPosition;

    // Start is called before the first frame update
    void Start()
    {
        EventHandler = GameObject.Find("EventHandler");

        DayNight = EventHandler.GetComponent<DayNightCycle>();

        unitTask = gameObject.GetComponent<UnitTask>();

        gameobjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();  
    }

    // Update is called once per frame
    void Update()
    {
        if(DayNight.time >= 14400 && DayNight.time <= 16000 ) {
            if(unitTask.GetTask() != ActionList.Dead) {
                if(!raisingDead) {
                    StartCoroutine(RaiseDead(DayNight.days));
                }          
            }
        }
    }

    IEnumerator RaiseDead(int number) {
        raisingDead = true;
        yield return new WaitForSeconds(10);
        spawnPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 5f);
        
        RTSPlayer owner = gameObject.GetComponent<UnitInformation>().owner;

        GameObject closestPlayerSpawnPoint = GetClosestEnemyPlayer(owner, gameObject, gameobjectLists.GetAllActivePlayerGameobjects());

        // loadedTransforms[0] = TownCenter;
        for(int i=0; i<number + 1; i++) {
            // skeleton.GetComponent<NPCController>().waypoints = loadedTransforms;
            GameObject raisedSkeleton = Instantiate(skeleton, spawnPosition, Quaternion.identity);  

            raisedSkeleton.GetComponent<UnitInformation>().owner = owner;

            NetworkServer.Spawn(raisedSkeleton);

            raisedSkeleton.GetComponent<NavMeshAgent>().SetDestination(closestPlayerSpawnPoint.transform.position);

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
}
