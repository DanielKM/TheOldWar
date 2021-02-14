using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameobjectLists : MonoBehaviour
{
    public List<GameObject> players = new List<GameObject>();
    public List<Unit> units = new List<Unit>();
    public List<Building> buildings = new List<Building>();
    public List<ResourceNode> resourceNodes = new List<ResourceNode>();
  
    public List<GameObject> GetAllActivePlayerGameobjects() 
    {
        return players;
    }

    public List<Unit> GetAllActiveUnitGameobjects() 
    {
        return units;
    }

    public List<Building> GetAllActiveBuildingGameobjects() 
    {
        return buildings;
    }

    public List<ResourceNode> GetAllActiveResourceNodes() 
    {
        return resourceNodes;
    }

    public void AllUnitsAttack()
    {
        RTSPlayer[] myPlayers = FindObjectsOfType(typeof(RTSPlayer)) as RTSPlayer[];

        for(int i = 0; i < myPlayers.Length; i++)
        {
            // PLAYER
            List<Unit> myActiveUnits = myPlayers[i].GetMyActiveUnits();
            
            List<Unit> enemyActiveUnits = units.Except(myActiveUnits).ToList();
                        
            Debug.Log(myPlayers[i] + " has " + myActiveUnits.Count + " units and have " + enemyActiveUnits.Count + " targets");

            Targeter targeter;
            foreach (Unit unit in myActiveUnits)
            {
                targeter = unit.GetTargeter();

                if(targeter.target != null) { return; }

                TargetNew(targeter, enemyActiveUnits);
            }
        }
    }

    public void TargetNew(Targeter targeter, List<Unit> enemyActiveUnits)
    {
        Unit closestEnemyUnit = GetClosestEnemy(targeter, enemyActiveUnits);

        if(closestEnemyUnit == null) { return; }

        targeter.target = closestEnemyUnit.GetComponent<Targetable>();
    }

    Unit GetClosestEnemy(Targeter targeter, List<Unit> enemies)
    {
        Unit tMin = null;
        float minDist = 300;
        Vector3 currentPos = targeter.transform.position;
        foreach (Unit enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = enemy;
                minDist = dist;
            }
        }
        return tMin;
    }
}

// foreach(Unit targetUnit in enemyActiveUnits)
//         {
//             float dist = Vector3.Distance(targetUnit.transform.position, targeter.transform.position);
//         }

//         if(other.gameObject.GetComponent<UnitInformation>() == null) { return; }

//         if(other.gameObject.GetComponent<UnitInformation>().owner == unitInformation.owner) { return; }

//         if(other.TryGetComponent<Targetable>(out Targetable target))
//         {
//             if(other.gameObject.TryGetComponent<Health>(out Health health)) 
//             {
//                 if(other.gameObject.GetComponent<Health>().currentHealth <= 0) { return; }

//                 targeter.CmdSetTarget(other.gameObject);

//                 return;
//             } 
//         }