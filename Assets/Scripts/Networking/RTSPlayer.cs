using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class RTSPlayer : NetworkBehaviour
{
    #region Variables

    [Header("References")]
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    public UIController UI = null;

    [SerializeField] private AudioSource playerAudio;
    [SerializeField] public GameObject spawnPoint;
    [SerializeField] public TechTree techTree;
    public Team team = null;
    [SerializeField] public Material teamMaterial;
    EventCycle EventCycle;
    
    [Header("Settings")]
    [SerializeField] private bool isComputerAI = false;
    [SerializeField] private float buildingRangeLimit = 25;

    [SerializeField] private int maxAllowablePopulation = 400;
    public int numberOfPlayers = 4;

    [SyncVar(hook = nameof(ClientHandleGoldUpdated))]
    [SerializeField] public int gold = 1000;

    [SyncVar(hook = nameof(ClientHandleMaxGoldUpdated))]
    [SerializeField] private int maxGold = 1000;
    
    [SyncVar(hook = nameof(ClientHandleIronUpdated))]
    [SerializeField] public int iron = 0;
    [SyncVar(hook = nameof(ClientHandleMaxIronUpdated))]
    [SerializeField] private int maxIron = 1000;

    [SyncVar(hook = nameof(ClientHandleSteelUpdated))]
    [SerializeField] public int steel = 0;
    [SyncVar(hook = nameof(ClientHandleMaxSteelUpdated))]
    [SerializeField] private int maxSteel = 1000;

    [SyncVar(hook = nameof(ClientHandleSkymetalUpdated))]
    [SerializeField] public int skymetal = 0;
    [SyncVar(hook = nameof(ClientHandleMaxSkymetalUpdated))]
    [SerializeField] private int maxSkymetal = 1000;

    [SyncVar(hook = nameof(ClientHandleWoodUpdated))]
    [SerializeField] public int wood = 0;
    [SyncVar(hook = nameof(ClientHandleMaxWoodUpdated))]
    [SerializeField] private int maxWood = 1000;

    [SyncVar(hook = nameof(ClientHandleStoneUpdated))]
    [SerializeField] public int stone = 0;
    [SyncVar(hook = nameof(ClientHandleMaxStoneUpdated))]
    [SerializeField] private int maxStone = 1000;

    [SyncVar(hook = nameof(ClientHandleFoodUpdated))]
    [SerializeField] public int food = 0;
    [SyncVar(hook = nameof(ClientHandleMaxFoodUpdated))]
    [SerializeField] private int maxFood = 1000;

    [SyncVar(hook = nameof(ClientHandlePopulationUpdated))]
    [SerializeField] public int population = 0;
    [SyncVar(hook = nameof(ClientHandleMaxPopulationUpdated))]
    [SerializeField] private int maxPopulation = 0;

    [SyncVar(hook = nameof(ClientHandleArmySizeUpdated))]
    [SerializeField] public int armySize = 0;
    [SyncVar(hook = nameof(ClientHandleMaxArmySizeUpdated))]
    [SerializeField] private int maxArmySize = 0;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    public string displayName;

    public event Action<int> ClientOnGoldUpdated;
    public event Action<int> ClientOnIronUpdated;
    public event Action<int> ClientOnSteelUpdated;
    public event Action<int> ClientOnSkymetalUpdated;
    public event Action<int> ClientOnWoodUpdated;
    public event Action<int> ClientOnStoneUpdated;
    public event Action<int> ClientOnFoodUpdated;
    public event Action<int> ClientOnPopulationUpdated;
    public event Action<int> ClientOnArmySizeUpdated;
    
    public event Action<int> ClientOnMaxGoldUpdated;
    public event Action<int> ClientOnMaxIronUpdated;
    public event Action<int> ClientOnMaxSteelUpdated;
    public event Action<int> ClientOnMaxSkymetalUpdated;
    public event Action<int> ClientOnMaxWoodUpdated;
    public event Action<int> ClientOnMaxStoneUpdated;
    public event Action<int> ClientOnMaxFoodUpdated;
    public event Action<int> ClientOnMaxPopulationUpdated;
    public event Action<int> ClientOnMaxArmySizeUpdated;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    private GameobjectLists gameObjectLists;
    private Color teamColor = new Color();

    [Header("Lists")]
    public List<Building> myBuildings = new List<Building>();
    public List<Building> myActiveBuildings = new List<Building>();

    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private Unit[] units = new Unit[0];
    [SerializeField] private Spell[] spells = new Spell[0];
    public List<Unit> myUnits = new List<Unit>();
    public List<Unit> myActiveUnits = new List<Unit>();

    #endregion

    #region GETTERS

    public string GetDisplayName() 
    {
        return displayName;
    }

    public bool GetIsPartyOwner() 
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform() 
    {
        return cameraTransform;
    }

    public Color GetTeamColor() 
    {
        return teamColor;
    }

    public Dictionary<Resource, int> GetResources() 
    {    
        Dictionary<Resource, int> resources = new Dictionary<Resource, int>(){ 
            {Resource.Gold, gold}, 
            {Resource.Iron, iron}, 
            {Resource.Steel, steel}, 
            {Resource.Skymetal, skymetal}, 
            {Resource.Wood, wood}, 
            {Resource.Stone, stone}, 
            {Resource.Food, food}, 
            {Resource.Population, population}, 
            {Resource.ArmySize, armySize}
        }; 
        return resources;
    }
    
    public Dictionary<Resource, int> GetMaxResources() 
    {    
        Dictionary<Resource, int> resources = new Dictionary<Resource, int>(){ 
            {Resource.Gold, maxGold}, 
            {Resource.Iron, maxIron}, 
            {Resource.Steel, maxSteel}, 
            {Resource.Skymetal, maxSkymetal}, 
            {Resource.Wood, maxWood}, 
            {Resource.Stone, maxStone}, 
            {Resource.Food, maxFood}, 
            {Resource.Population, maxPopulation}, 
            {Resource.ArmySize, armySize}
        }; 
        return resources;
    }

    public List<Unit> GetMyUnits() 
    {
        return myUnits;
    }

    public List<Unit> GetMyActiveUnits() 
    {
        return myActiveUnits;
    }

    public List<Building> GetMyBuildings() 
    {
        return myBuildings;
    }

    public List<Building> GetMyActiveBuildings() 
    {
        return myActiveBuildings;
    }

    public Unit[] GetMyTrainableUnits() 
    {
        return units;
    }
    
    public Spell[] GetMySpells() 
    {
        return spells;
    }

    #endregion

    #region Local Functions

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {        
        if(Physics.CheckBox(
            point + buildingCollider.center, 
            buildingCollider.size / 2, 
            Quaternion.identity,
            buildingBlockLayer)) 
        { 
            return false; 
        }

        foreach(Building building in myBuildings) 
        {
            if((point - building.transform.position).sqrMagnitude 
                <= buildingRangeLimit * buildingRangeLimit) 
            {
                return true;
            }
        }
        return false;
    }

    public void Start()
    {
        if(isComputerAI) 
        {   

            GameObject teamParent = GameObject.Find("Teams");

            int teamParentChildCount = teamParent.transform.childCount;

            foreach ( Transform child in teamParent.transform)
            {
                if(child.TryGetComponent<Team>(out Team childTeam))
                {
                    if(childTeam.teamName == "Enemy")
                    {
                        team = childTeam;
                    }
                }
            }
            GetAllMyActiveUnits();
        }
    }

    public void GetAllMyActiveUnits() 
    {
        gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();

        List<Unit> startingUnits = gameObjectLists.units;

        foreach(Unit startingUnit in startingUnits)
        {
            startingUnit.GetComponent<Health>().currentHealth = startingUnit.GetComponent<Health>().currentHealth;
            startingUnit.GetComponent<Health>().maxHealth = startingUnit.GetComponent<Health>().maxHealth;
            startingUnit.GetComponent<NavMeshAgent>().speed = startingUnit.GetComponent<NavMeshAgent>().speed;
            startingUnit.GetComponent<UnitFiring>().fireRate = startingUnit.GetComponent<UnitFiring>().fireRate;
            startingUnit.GetComponent<UnitInformation>().team = team;
            myActiveUnits.Add(startingUnit);
        }

        gameObjectLists.players.Add(this.gameObject);
    }

    #endregion

    #region Server

    public override void OnStartServer()
    {
        if(isServer) 
        {
            Debug.Log("SERVER!");
        } else 
        {
            Debug.Log("CLIENT");
        }

        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetTeam(Team newTeam)
    {
        team = newTeam;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Server]
    public void SetResources(Dictionary<Resource, int> newResources)
    {
        gold = newResources[Resource.Gold] > maxGold ? maxGold : newResources[Resource.Gold];
        iron = newResources[Resource.Iron] > maxIron ? maxIron : newResources[Resource.Iron];
        steel = newResources[Resource.Steel] > maxSteel ? maxSteel : newResources[Resource.Steel];
        skymetal = newResources[Resource.Skymetal] > maxSkymetal ? maxSkymetal : newResources[Resource.Skymetal];
        wood = newResources[Resource.Wood] > maxWood ? maxWood : newResources[Resource.Wood];
        stone = newResources[Resource.Stone] > maxStone ? maxStone : newResources[Resource.Stone];
        food = newResources[Resource.Food] > maxFood ? maxFood : newResources[Resource.Food];
        population = newResources[Resource.Population] > maxPopulation ? maxPopulation : newResources[Resource.Population];
        armySize = newResources[Resource.ArmySize] > maxArmySize ? maxArmySize : newResources[Resource.ArmySize];
    }

    [Server]
    public void SetMaxResources(Dictionary<Resource, int> newResources)
    {
        maxGold = newResources[Resource.Gold];
        maxIron = newResources[Resource.Iron];
        maxSteel = newResources[Resource.Steel];
        maxSkymetal = newResources[Resource.Skymetal];
        maxWood = newResources[Resource.Wood];
        maxStone = newResources[Resource.Stone];
        maxFood = newResources[Resource.Food];
        maxPopulation = newResources[Resource.Population] > (maxAllowablePopulation/numberOfPlayers) ? (maxAllowablePopulation/numberOfPlayers) : newResources[Resource.Population];
        maxArmySize = newResources[Resource.ArmySize];
    }   

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame("Loading");
    }
    
    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point, Quaternion rotation)
    {
        Building buildingToPlace = null;

        foreach (Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        // HERE DAN

        if(buildingToPlace == null) { return; }

        if(CannotAfford(GetResources(), buildingToPlace.GetPrice())) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if(!CanPlaceBuilding(buildingCollider, point)) { return; }
        
        // List<GameObject> pooledBuildings;

        GameObject selectedBuilding = null;
        switch(buildingToPlace.gameObject.GetComponent<UnitInformation>().unitType) 
        {
            case UnitType.House:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().houses;
                break;
            case UnitType.TownHall:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().townHalls;
                break;
            case UnitType.Farm:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().farms;
                break;
            case UnitType.Barracks:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().barracksList;
                break;
            case UnitType.Stables:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().stablesList;
                break;
            case UnitType.Blacksmith:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().blacksmiths;
                break;
            case UnitType.LumberYard:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().lumberYards;
                break;
            case UnitType.Market:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().markets;
                break;
            case UnitType.WatchTower:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().watchTowers;
                break;
            case UnitType.WoodWall:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().woodWalls;
                break;
            case UnitType.WizardTower:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().wizardTowers;
                break;
            case UnitType.Hospital:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().wizardTowers;
                break;
            case UnitType.Caravan:
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().wizardTowers;
                break;
            default: 
                // pooledBuildings = gameObject.GetComponent<PooledGameobjects>().woodWalls;
                break;
        }  
        // Quaternion projectileRotation =      
        // Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);

        buildingToPlace.foundation.transform.position = point;
        buildingToPlace.foundation.transform.rotation = rotation;
        buildingToPlace.foundation.GetComponent<UnitInformation>().owner = this;
        buildingToPlace.foundation.GetComponent<UnitInformation>().team = this.team;

        selectedBuilding = Instantiate(buildingToPlace.foundation);
        NetworkServer.Spawn(selectedBuilding, connectionToClient);
        SetResources(SubtractPrice(buildingToPlace.GetPrice()));
  
        // // Pooled Buildings
        // for(int i = 0; i<pooledBuildings.Count; i++)
        // {
        //     if(!pooledBuildings[i].activeInHierarchy)
        //     {
        //         GameObject buildingToSpawn = pooledBuildings[i];
        //         buildingToSpawn.transform.position = point;
        //         buildingToSpawn.transform.rotation = rotation;
                
        //         buildingToSpawn.GetComponent<UnitInformation>().owner = this;
                
        //         buildingToSpawn.SetActive(true);

        //         SetResources(SubtractPrice(buildingToPlace.GetPrice()));
        //         break;
        //     }
        // }
    }

    [Command]
    public void CmdCastSpell(Vector3 point)
    {
        EventCycle = GameObject.Find("EventHandler").GetComponent<EventCycle>();
        // Pooled spells
        if(EventCycle.instability <= 100) 
        { 
            EventCycle.instability += 1;
        }

        GameObject objSpell = (GameObject)Instantiate(spells[0].gameObject);

        objSpell.transform.position = point;

        objSpell.GetComponent<UnitInformation>().owner = connectionToClient.identity.GetComponent<RTSPlayer>();
        objSpell.GetComponent<UnitInformation>().team = this.team;
                

        NetworkServer.Spawn(objSpell, connectionToClient);
    }

    public Dictionary<Resource, int> SubtractPrice(Dictionary <Resource, int> resourcePrice)
    {
        Dictionary<Resource, int> newResourceDictionary = GetResources().ToDictionary(p => p.Key, p => p.Value - resourcePrice[p.Key]);

        return newResourceDictionary;
    }

    public bool CannotAfford (Dictionary<Resource, int> currentResources, Dictionary <Resource, int> resourcePrice)
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();
        for(int i = 0; i<currentResources.Count; i++)
        {
            if(currentResources.ElementAt(i).Value < resourcePrice.ElementAt(i).Value) 
            { 
                // UI.ShowNotEnoughResources(currentResources.ElementAt(i).Key);
                return true; 
            }

            if(currentResources.ElementAt(i).Key == Resource.Population) 
            { 
                if(resourcePrice.ElementAt(i).Value < 0 && currentResources[Resource.Population] == GetMaxResources()[Resource.Population]) 
                { 
                    // UI.ShowNotEnoughResources(currentResources.ElementAt(i).Key);
                    return true; 
                }
            }
        }
        return false;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if(unit.connectionToClient == null ) { return; }

        if(unit.connectionToClient != connectionToClient) { return; }

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if(unit.connectionToClient == null ) { return; }

        if(unit.connectionToClient != connectionToClient) { return; }
        
        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if(building.connectionToClient == null ) { return; }

        if(building.connectionToClient != connectionToClient) { return; }
        
        myBuildings.Add(building);

        techTree.UpdateUnlocks(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if(building.connectionToClient == null ) { return; }

        if(building.connectionToClient != connectionToClient) { return; }
        
        myBuildings.Remove(building);
        
        techTree.UpdateUnlocks(building);
    }

    #endregion

    #region Client
    
    public override void OnStartAuthority()
    {
        if(NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        if(NetworkServer.active) { return; }

        DontDestroyOnLoad(gameObject);

        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        // if(!isClientOnly || !hasAuthority) { return; } change trial
        if(!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);
        
        if(!hasAuthority) { return; } // added change trial

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    # region Client Resource Handling
    private void ClientHandleGoldUpdated(int oldResources, int newResources)
    {
        ClientOnGoldUpdated?.Invoke(newResources);
    }

    private void ClientHandleMaxGoldUpdated(int oldResources, int newResources)
    {
        ClientOnMaxGoldUpdated?.Invoke(newResources);
    }

    private void ClientHandleIronUpdated(int oldResources, int newResources)
    {
        ClientOnIronUpdated?.Invoke(newResources);
    }
    
    private void ClientHandleMaxIronUpdated(int oldResources, int newResources)
    {
        ClientOnMaxIronUpdated?.Invoke(newResources);
    }

    private void ClientHandleSteelUpdated(int oldResources, int newResources)
    {
        ClientOnSteelUpdated?.Invoke(newResources);
    }

    private void ClientHandleMaxSteelUpdated(int oldResources, int newResources)
    {
        ClientOnMaxSteelUpdated?.Invoke(newResources);
    }

    private void ClientHandleSkymetalUpdated(int oldResources, int newResources)
    {
        ClientOnSkymetalUpdated?.Invoke(newResources);
    }

    private void ClientHandleMaxSkymetalUpdated(int oldResources, int newResources)
    {
        ClientOnMaxSkymetalUpdated?.Invoke(newResources);
    }
    
    private void ClientHandleWoodUpdated(int oldResources, int newResources)
    {
        ClientOnWoodUpdated?.Invoke(newResources);
    }
    
    private void ClientHandleMaxWoodUpdated(int oldResources, int newResources)
    {
        ClientOnMaxWoodUpdated?.Invoke(newResources);
    }

    private void ClientHandleStoneUpdated(int oldResources, int newResources)
    {
        ClientOnStoneUpdated?.Invoke(newResources);
    }
    
    private void ClientHandleMaxStoneUpdated(int oldResources, int newResources)
    {
        ClientOnMaxStoneUpdated?.Invoke(newResources);
    }

    private void ClientHandleFoodUpdated(int oldResources, int newResources)
    {
        ClientOnFoodUpdated?.Invoke(newResources);
    }
    
    private void ClientHandleMaxFoodUpdated(int oldResources, int newResources)
    {
        ClientOnMaxFoodUpdated?.Invoke(newResources);
    }

    private void ClientHandlePopulationUpdated(int oldResources, int newResources)
    {
        ClientOnPopulationUpdated?.Invoke(newResources);
    }

    private void ClientHandleMaxPopulationUpdated(int oldResources, int newResources)
    {
        ClientOnMaxPopulationUpdated?.Invoke(newResources);
    }

    private void ClientHandleArmySizeUpdated(int oldResources, int newResources)
    {
        ClientOnArmySizeUpdated?.Invoke(newResources);
    }

    private void ClientHandleMaxArmySizeUpdated(int oldResources, int newResources)
    {
        ClientOnMaxArmySizeUpdated?.Invoke(newResources);
    }

    #endregion

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion
}
