﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f;

    Unit[] trainableUnits = null;
    List<Unit> unitsToTrain = new List<Unit>();

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private RTSPlayer player = null;
    UnitInformation unitInformation = null;

    public Vector3 rallyPoint;
    public GameObject rallyPointGameObject = null;
    public bool rallyPointSet = false;

    public void Start()
    {
        rallyPoint = gameObject.transform.position;
        
        unitInformation = gameObject.GetComponent<UnitInformation>();
    }

    private void Update()
    {
        if(player == null) 
        {
            // player = connectionToClient.identity.GetComponent<RTSPlayer>();
        }

        if (Input.GetMouseButtonDown(1) && unitInformation.selected == true)
        {
            BuildingRightClick();
        }

        if(isServer)
        {
            ProduceUnits();
        }
        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    public void BuildingRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 350))
        {   
            rallyPoint = hit.point;
            rallyPointGameObject.transform.position = rallyPoint;
            rallyPointSet = true;
        }
    }

    public void ShowRallyPoint()
    {
        // DestroyRallyPoint();
        if(!rallyPointSet) { return; }

        rallyPointGameObject.transform.position = rallyPoint;
        rallyPointGameObject.SetActive(true);
    }

    string[] unitFirstNames = {"Aron", "Aston", "Arslan", "Aiden", "Damon", "Varyn", "Nordas", "Harald", "Gavan", "Davyd", "Kevan", "Steven", "Rian"};
    string[] unitSecondNamesStart = {"Stone", "Fire", "Grass", "North", "South", "East", "West", "White", "Black", "Red", "Green", "Gold", "Swift", "Keen"};
    string[] unitSecondNamesEnd = {"heart", "axe", "hammer", "stone", "spear", "herder", "watcher", "arm", "legs", "tree", "path", "guide", "shield", "sword", "bow", "banner"};

    #region Server

    private void ProduceUnits()
    {
        if(queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if(unitTimer < unitSpawnDuration) { return; }

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = 0;        
        
        GameObject unitInstance = null;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        // List<GameObject> pooledUnits;

        int firstNameNumber = Random.Range(0, unitFirstNames.Length);
        int secondNamesStartNumber = Random.Range(0, unitSecondNamesStart.Length);
        int secondNamesEndNumber = Random.Range(0, unitSecondNamesEnd.Length);
       
        string unitName = unitFirstNames[firstNameNumber] + " " + unitSecondNamesStart[secondNamesStartNumber] + unitSecondNamesEnd[secondNamesEndNumber];

        switch(unitsToTrain[0].gameObject.GetComponent<UnitInformation>().unitType) 
        {
            case UnitType.Worker:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().worker, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().workers;
                break;
            case UnitType.Swordsman:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().swordsman, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().swordsmen;
                break;
            case UnitType.Footman:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().footman, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().footmen;
                break;
            case UnitType.Archer:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().archer, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().archers;
                break;
            case UnitType.Outrider:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().outrider, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().outriders;
                break;
            case UnitType.Knight:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().knight, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().knights;
                break;
            case UnitType.Wizard:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().wizard, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().wizards;
                break;
            case UnitType.Healer:
                unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().healer, unitSpawnPoint.position, unitSpawnPoint.rotation);
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().wizards;
                break;
            default: 
                // pooledUnits = player.gameObject.GetComponent<PooledGameobjects>().workers;
                break;
        }
        NavMeshAgent agent = unitInstance.GetComponent<NavMeshAgent>();
        Unit unit = unitInstance.GetComponent<Unit>();
        agent.Warp(unitInstance.transform.position);


        // unitInstance = Instantiate(player.gameObject.GetComponent<PooledGameobjects>().worker, unitSpawnPoint.position,unitSpawnPoint.rotation);
        
        unitInstance.GetComponent<UnitInformation>().owner = player;
        unitInstance.GetComponent<UnitInformation>().team = player.team;
        unitInstance.GetComponent<UnitInformation>().unitName = unitName;

        NetworkServer.Spawn(unitInstance, connectionToClient);

        if(gameObject.transform.position != rallyPoint)
        {
            unit.Move(rallyPoint);
        } else {
            unit.Move(unitSpawnPoint.position + spawnOffset);
        }

        // Pooled Projectiles
        // for(int i = 0; i<pooledUnits.Count; i++)
        // {
        //     if(!pooledUnits[i].activeInHierarchy)
        //     {
        //         GameObject unitToSpawn = pooledUnits[i];
        //         ServerSpawnUnit(unitToSpawn, spawnOffset);
        //         break;
        //     }
        // }
        unitsToTrain.RemoveAt(0);
        queuedUnits--;
        unitTimer = 0;
    }

    [Server]
    public void ServerSpawnUnit(GameObject unitToSpawn, Vector3 spawnOffset)
    {
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        unitToSpawn.transform.position = unitSpawnPoint.position;
        unitToSpawn.transform.rotation = unitSpawnPoint.rotation;
        unitToSpawn.GetComponent<UnitInformation>().owner = player;
        unitToSpawn.GetComponent<UnitInformation>().team = player.team;
        unitToSpawn.SetActive(true);
        unitToSpawn.GetComponent<UnitMovement>().ServerMove(unitSpawnPoint.position + spawnOffset);
    }

    [Command] 
    public void CmdSpawnUnit(int unitId) 
    {
        if(queuedUnits == maxUnitQueue) { return; }

        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        trainableUnits = player.GetMyTrainableUnits();

        foreach (Unit unit in trainableUnits)
        {
            if(unit.GetId() == unitId)
            {
                if(player.CannotAfford(player.GetResources(), unit.GetPrice())) { return; }
                
                player.SetResources(player.SubtractPrice(unit.GetPrice()));

                queuedUnits++;
                
                unitsToTrain.Add(unit);
                break;
            }
        }
    }

    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if(newProgress < unitProgressImage.fillAmount) 
        {
            unitProgressImage.fillAmount = newProgress;
        } 
        else 
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.01f
            );
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        if(!hasAuthority) { return; }
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    #endregion
}
