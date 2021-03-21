using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Building : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int id = -1;
    
    [SerializeField] public int gold = 0;
    [SerializeField] public int iron = 0;
    [SerializeField] public int steel = 0;
    [SerializeField] public int skymetal = 0;
    [SerializeField] public int wood = 0;
    [SerializeField] public int stone = 0;
    [SerializeField] public int food = 0;
    [SerializeField] public int population = 0;

    BuildingPlacementHandler buildingPlacementHandler;

    [TextArea]
    public string description;

    [SerializeField] public GameObject foundation = null;
    [SerializeField] private UIController UI = null;
    private GameObject UIGameObject = null;
    
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;
    
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    public void Start()
    {
        UIGameObject = GameObject.Find("UI");
        UI = UIGameObject.GetComponent<UIController>();
        buildingPlacementHandler = GameObject.Find("UnitHandlers").GetComponent<BuildingPlacementHandler>();
    }

    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public int GetId()
    {
        return id;
    }

    public Dictionary<Resource, int> GetPrice()
    {    
        Dictionary<Resource, int> buildingPrice = new Dictionary<Resource, int>(){ 
            {Resource.Gold, gold}, 
            {Resource.Iron, iron}, 
            {Resource.Steel, steel}, 
            {Resource.Skymetal, skymetal}, 
            {Resource.Wood, wood}, 
            {Resource.Stone, stone}, 
            {Resource.Food, food}, 
            {Resource.Population, population}, 
        }; 
        return buildingPrice;
    }

    public string GetDescription()
    {
        return description;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);

        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);

        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        if(gameObject.GetComponent<UnitInformation>().owner == null) 
        { 
            Destroy(gameObject);
            
            return;
        }

        NetworkServer.Destroy(gameObject);
        
        Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if(!hasAuthority) { return; }

        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left || buildingPlacementHandler.placingBuilding) { return; }

        if(!hasAuthority) { return; }

        SelectBuilding();
    }

    public void SelectBuilding()
    {
        UIGameObject.GetComponent<Buttons>().building = gameObject.GetComponent<Building>();
        UpdateBuildingPanel(gameObject.GetComponent<Building>());
        UnitType unitType = gameObject.GetComponent<UnitInformation>().unitType;
        UI.BuildingSelect(unitType);
    }

    public void UpdateBuildingPanel(Building building)
    {
        UI.buildingIcon.GetComponent<Image>().sprite = building.GetComponent<Building>().icon;
        UI.buildingHealthBar.maxValue = building.gameObject.GetComponent<Health>().maxHealth;
        UI.buildingHealthBar.value = building.gameObject.GetComponent<Health>().currentHealth;

        // UI.unitEnergyBar.maxValue = unitScript.maxEnergy;
        // UI.unitEnergyBar.value = unitScript.energy;

        UI.buildingHealthDisp.text = "HEALTH: " + building.gameObject.GetComponent<Health>().currentHealth;
        // UI.unitEnergyDisplay.text = "ENERGY: " + unitScript.energy;

        UnitInformation unitInformation = building.gameObject.GetComponent<UnitInformation>();
        
        UI.buildingNameDisp.text = unitInformation.unitName;
        UI.buildingTypeDisp.text = unitInformation.unitType.ToString();
        UI.buildingRankDisp.text = unitInformation.unitRank.ToString();
        UI.buildingKillDisp.text = "Kills: " + unitInformation.unitKills;

        UI.buildingWeaponDisp.text = unitInformation.unitWeapon.ToString();
        UI.buildingArmourDisp.text = unitInformation.unitArmour.ToString();
    }

    #endregion

}