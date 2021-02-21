using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ComputerAI : MonoBehaviour
{
    [Header("References")]
    private RTSPlayer player;
    private GameobjectLists gameObjectLists;
    public Unit closestEnemyUnit = null;
    private List<Unit> myActiveUnits;
    
    [Header("Settings")]
    public float checkRate = 1.0f;
    public float closeDistance = 50.0f;
    public float farDistance = 100.0f;


    void Awake()
    {
        player = GetComponent<RTSPlayer>();

        gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();
    }

    void Start()
    {
        InvokeRepeating("RunAI", 0.0f, checkRate);
    }

    public void RunAI()
    {
        string enemyInformation = CheckEnemyInformation(); // 0-9 + 0-9
        // 0-1 = no info
        // 2-5 = enemy far
        // 6-9 = enemy close

        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        string armyInformation = CheckArmyComposition(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        return;
        // Get all buildings - next steps
        int baseInformation = CheckBaseSize(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        int resourceInformation = CheckResources(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        string inputData = enemyInformation + armyInformation + baseInformation + resourceInformation;

        string strategy = SelectOverallStrategy(inputData);
       
        AssignTasks(strategy);
    }

    private int CheckResources()
    {
        throw new NotImplementedException();
    }

    private int CheckBaseSize()
    {
        throw new NotImplementedException();
    }

    private string CheckArmyComposition()
    {
        string armyPowerLevel = "0";

        if(myActiveUnits.Count > 90) { armyPowerLevel = "9"; }
        else if (myActiveUnits.Count > 80) { armyPowerLevel = "8"; }
        else if (myActiveUnits.Count > 70) { armyPowerLevel = "7"; }
        else if (myActiveUnits.Count > 60) { armyPowerLevel = "6"; }
        else if (myActiveUnits.Count > 50) { armyPowerLevel = "5"; }
        else if (myActiveUnits.Count > 40) { armyPowerLevel = "4"; }
        else if (myActiveUnits.Count > 30) { armyPowerLevel = "3"; }
        else if (myActiveUnits.Count > 20) { armyPowerLevel = "2"; }
        else if (myActiveUnits.Count > 10) { armyPowerLevel = "1"; }
        else { armyPowerLevel = "0"; }

        return armyPowerLevel;
    }

    private string CheckEnemyInformation()
    {
        string enemyInformationCode = "00";

        float minDistance = 10000f;

        string proximity = "0";
        string count = "0";
        
        myActiveUnits = player.GetMyActiveUnits();
        
        List<Unit> enemyActiveUnits = gameObjectLists.GetAllActiveUnitGameobjects().Except(myActiveUnits).ToList();

        if(enemyActiveUnits.Count == 0 ) { enemyInformationCode = "00"; } else {

            foreach (Unit friendly in myActiveUnits) 
            {
                if(friendly == null) { continue; }

                Unit thisUnitClosestEnemy = GetClosestEnemy(friendly.GetComponent<Targeter>(), enemyActiveUnits);

                float dist = Vector3.Distance(thisUnitClosestEnemy.transform.position, friendly.transform.position);

                if(dist < minDistance) 
                { 
                    minDistance = dist; 
                    closestEnemyUnit = thisUnitClosestEnemy;
                }
            }     
        }

        if(minDistance < 30) { proximity = "9"; }
        else if (minDistance < 40) { proximity = "8"; }
        else if (minDistance < 50) { proximity = "7"; }
        else if (minDistance < 60) { proximity = "6"; }
        else if (minDistance < 70) { proximity = "5"; }
        else if (minDistance < 80) { proximity = "4"; }
        else if (minDistance < 90) { proximity = "3"; }
        else if (minDistance < 100) { proximity = "2"; }
        else if (minDistance < 110) { proximity = "1"; }
        else { proximity = "0"; }

        if(enemyActiveUnits.Count > 90) { proximity = "9"; }
        else if (enemyActiveUnits.Count > 80) { proximity = "8"; }
        else if (enemyActiveUnits.Count > 70) { proximity = "7"; }
        else if (enemyActiveUnits.Count > 60) { proximity = "6"; }
        else if (enemyActiveUnits.Count > 50) { proximity = "5"; }
        else if (enemyActiveUnits.Count > 40) { proximity = "4"; }
        else if (enemyActiveUnits.Count > 30) { proximity = "3"; }
        else if (enemyActiveUnits.Count > 20) { proximity = "2"; }
        else if (enemyActiveUnits.Count > 10) { proximity = "1"; }
        else { proximity = "0"; }

        enemyInformationCode = proximity + count + "";

        return enemyInformationCode;
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

    public string SelectOverallStrategy(string input)
    {
        switch(input)
        {
            // case input > 80000:
            // return "Defend";
            // case input > 1:
            // return "Resources";
            // case input > 1000:
            // return "Muster";
            // case input > 10000:
            // return "Attack";
            default:
            return "Resources";
        }
    }

    public void AssignTasks(string strategy)
    {
        switch(strategy)
        {
            case "Resources":
                SendOutScouts();
                AssignCurrentWorkers();
                TrainNewWorkers();
                BuildMoreHouses();
                ExpandTechTree();
            return;
            case "Muster":
                TrainNewTroops();
                BuildMoreBarracks();
                BuildMoreWizardTowers();
                BuildMoreFortifications();
            return;
            case "Attack":
                Transform enemyBuilding = FindClosestEnemyBuilding();
                FindMusterPoint(enemyBuilding);
                FindMusterPoint(enemyBuilding);
                Attack(enemyBuilding);
            return;
            case "Defend":
                int attackDirection = DetectEnemyAttack(); // 0-12
                SendAppropriateTroops();
            return;
            default:
            return;
        }
    }

    private void SendAppropriateTroops()
    {
        throw new NotImplementedException();
    }

    private int DetectEnemyAttack()
    {
        throw new NotImplementedException();
    }

    private void Attack(Transform enemyBuilding)
    {
        throw new NotImplementedException();
    }

    private void FindMusterPoint(Transform t)
    {
        throw new NotImplementedException();
    }

    private Transform FindClosestEnemyBuilding()
    {
        throw new NotImplementedException();
    }

    private void BuildMoreFortifications()
    {
        throw new NotImplementedException();
    }

    private void BuildMoreWizardTowers()
    {
        throw new NotImplementedException();
    }

    private void BuildMoreBarracks()
    {
        throw new NotImplementedException();
    }

    private void TrainNewTroops()
    {
        throw new NotImplementedException();
    }

    private void ExpandTechTree()
    {
        throw new NotImplementedException();
    }

    private void BuildMoreHouses()
    {
        throw new NotImplementedException();
    }

    private void TrainNewWorkers()
    {
        throw new NotImplementedException();
    }

    private void AssignCurrentWorkers()
    {
        throw new NotImplementedException();
    }

    private void SendOutScouts()
    {
        throw new NotImplementedException();
    }
}
