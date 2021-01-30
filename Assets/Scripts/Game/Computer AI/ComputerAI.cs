using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ComputerAI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    public void Update()
    {
        RunAI();
    }

    public void RunAI()
    {
        int enemyInformation = CheckEnemyInformation(); // 0-9 + 0-9
        // 0-1 = no info
        // 2-5 = enemy far
        // 6-9 = enemy close

        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        int armyInformation = CheckArmyComposition(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        int baseInformation = CheckBaseSize(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        int resourceInformation = CheckResources(); // 0-9
        // 0-1 = weak
        // 2-5 = moderate
        // 6-9 = strong

        int inputData = enemyInformation + armyInformation + baseInformation + resourceInformation;

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

    private int CheckArmyComposition()
    {
        throw new NotImplementedException();
    }

    private int CheckEnemyInformation()
    {
        throw new NotImplementedException();
    }

    public string SelectOverallStrategy(int input)
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
