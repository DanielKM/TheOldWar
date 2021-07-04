using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PooledGameobjects : NetworkBehaviour
{
    [SerializeField] private LayerMask floorMask = new LayerMask();
    private Camera mainCamera;

    [SerializeField] public bool isComputerAI = false;
    public int pooledAmount = 200;

    [Header("Projectiles")]
    [Header("GameObjects")]
    public GameObject arrow;
    public GameObject pickaxe;
    public GameObject sword;
    public GameObject fireball;

    public GameObject click;

    [Header("Units")]
    // Individual Units
    public GameObject worker;
    public GameObject swordsman;
    public GameObject footman;
    public GameObject archer;
    public GameObject outrider;
    public GameObject knight;
    public GameObject wizard;
    public GameObject healer;

    [Header("Buildings")]
    // Individual Buildings
    public GameObject house;
    public GameObject townHall;
    public GameObject farm;
    public GameObject barracks;
    public GameObject stables;
    public GameObject blacksmith;
    public GameObject lumberYard;
    public GameObject market;
    public GameObject watchTower;
    public GameObject woodWall;
    public GameObject wizardTower;
    public GameObject hospital;
    public GameObject caravan;

    [Header("Projectiles")]
    [Header("Lists")]
    public List<GameObject> arrows;
    public List<GameObject> pickaxes;
    public List<GameObject> swords;
    public List<GameObject> fireballs;
    public List<GameObject> clicks;

    [Header("Units")]
    // Unit lists
    public List<GameObject> workers;
    public List<GameObject> swordsmen;
    public List<GameObject> footmen;
    public List<GameObject> archers;
    public List<GameObject> outriders;
    public List<GameObject> knights;
    public List<GameObject> wizards;
    public List<GameObject> healers;

    [Header("Buildings")]
    // Building lists
    public List<GameObject> houses;
    public List<GameObject> townHalls;
    public List<GameObject> farms;
    public List<GameObject> barracksList;
    public List<GameObject> stablesList;
    public List<GameObject> blacksmiths;
    public List<GameObject> lumberYards;
    public List<GameObject> markets;
    public List<GameObject> watchTowers;
    public List<GameObject> woodWalls;
    public List<GameObject> wizardTowers;
    public List<GameObject> hospitals;
    public List<GameObject> caravans;

    void Start() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if(isComputerAI == false) { return; }

        // CmdLoadAllPooledObjects();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CmdLoadAllPooledObjects();
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdLoadAllPooledObjects() 
    {
        // ServerLoadAllPooledObjects();
    }

    [Server]
    void ServerAddItemInHierarchy(string childName, GameObject parent)
    {
        GameObject childGameObject = new GameObject(childName);
        childGameObject.AddComponent<NetworkIdentity>();
        childGameObject.transform.parent = parent.transform;
    }

    [Server]
    void ServerLoadAllPooledObjects()
    {
        GameObject allPooledObjects = new GameObject("All Pooled Objects");
        allPooledObjects.AddComponent<NetworkIdentity>();
        
        #region Projectiles
        
        GameObject pooledProjectiles = new GameObject("Projectiles");
        pooledProjectiles.AddComponent<NetworkIdentity>();
        pooledProjectiles.transform.parent = allPooledObjects.transform;

        ServerAddItemInHierarchy("Arrows", pooledProjectiles);

        ServerAddItemInHierarchy("Pickaxes", pooledProjectiles);

        ServerAddItemInHierarchy("Swords", pooledProjectiles);

        ServerAddItemInHierarchy("Fireballs", pooledProjectiles);

        ServerAddItemInHierarchy("Clicks", pooledProjectiles);

        #endregion

        #region Buildings

        GameObject pooledBuildings = new GameObject("Buildings");
        pooledBuildings.AddComponent<NetworkIdentity>();
        pooledBuildings.transform.parent = allPooledObjects.transform;

        ServerAddItemInHierarchy("Houses", pooledBuildings);

        ServerAddItemInHierarchy("Town Halls", pooledBuildings);

        ServerAddItemInHierarchy("Farms", pooledBuildings);

        ServerAddItemInHierarchy("Barracks", pooledBuildings);

        ServerAddItemInHierarchy("Stables", pooledBuildings);

        ServerAddItemInHierarchy("Blacksmiths", pooledBuildings);

        ServerAddItemInHierarchy("Lumber Yards", pooledBuildings);

        ServerAddItemInHierarchy("Markets", pooledBuildings);

        ServerAddItemInHierarchy("Watch Towers", pooledBuildings);

        ServerAddItemInHierarchy("Wood Walls", pooledBuildings);

        ServerAddItemInHierarchy("Wizard Towers", pooledBuildings);

        ServerAddItemInHierarchy("Hospitals", pooledBuildings);

        ServerAddItemInHierarchy("Caravans", pooledBuildings);

        #endregion

        #region Units

        GameObject pooledUnits = new GameObject("Units");
        pooledUnits.AddComponent<NetworkIdentity>();
        pooledUnits.transform.parent = allPooledObjects.transform;

        ServerAddItemInHierarchy("Workers", pooledUnits);

        ServerAddItemInHierarchy("Swordsmen", pooledUnits);

        ServerAddItemInHierarchy("Footmen", pooledUnits);

        ServerAddItemInHierarchy("Archers", pooledUnits);

        ServerAddItemInHierarchy("Outriders", pooledUnits);

        ServerAddItemInHierarchy("Knights", pooledUnits);

        ServerAddItemInHierarchy("Wizards", pooledUnits);

        ServerAddItemInHierarchy("Healers", pooledUnits);

        #endregion
        for(int i = 0; i < pooledAmount; i++)
        {
            #region Projectile Spawning

            SpawnGameObject(arrow, arrows, "Arrows");

            SpawnGameObject(pickaxe, pickaxes, "Pickaxes");

            SpawnGameObject(sword, swords, "Swords");

            SpawnGameObject(fireball, fireballs, "Fireballs");

            SpawnGameObject(click, clicks, "Clicks");

            #endregion

            #region Unit Spawning
            
            SpawnGameObject(worker, workers, "Workers");

            SpawnGameObject(swordsman, swordsmen, "Swordsmen");

            SpawnGameObject(footman, footmen, "Footmen");

            SpawnGameObject(archer, archers, "Archers");

            SpawnGameObject(outrider, outriders, "Outriders");
                        
            SpawnGameObject(knight, knights, "Knights");

            SpawnGameObject(wizard, wizards, "Wizards");

            SpawnGameObject(healer, healers, "Healers");

            #endregion

            #region Building Spawning

            if(isComputerAI == false) {

                SpawnGameObject(house, houses, "Houses");

                SpawnGameObject(townHall, townHalls, "Town Halls");

                SpawnGameObject(farm, farms, "Farms");

                SpawnGameObject(barracks, barracksList, "Barracks");

                SpawnGameObject(stables, stablesList, "Stables");
                            
                SpawnGameObject(blacksmith, blacksmiths, "Blacksmiths");

                SpawnGameObject(lumberYard, lumberYards, "Lumber Yards");

                SpawnGameObject(market, markets, "Markets");

                SpawnGameObject(watchTower, watchTowers, "Watch Towers");

                SpawnGameObject(woodWall, woodWalls, "Wood Walls");

                SpawnGameObject(wizardTower, wizardTowers, "Wizard Towers");

                SpawnGameObject(hospital, hospitals, "Hospitals");

                SpawnGameObject(caravan, caravans, "Caravans");
            }
            
            #endregion
        }
    }

    public void SpawnGameObject(GameObject prefab, List<GameObject> prefabList, string parent) 
    {
        GameObject spawnableInstance = (GameObject)Instantiate(prefab);
        spawnableInstance.SetActive(false);
        NetworkServer.Spawn(spawnableInstance, connectionToClient);
        spawnableInstance.transform.parent = GameObject.Find(parent).transform;
        prefabList.Add(spawnableInstance);
    }

    public override void OnStopClient()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Pooled clicks
        
    void Update()
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) 
        {              
            if (Input.GetMouseButtonUp(1)) 
            {
                mainCamera = Camera.main;
                AddClick();
            }
        }
    }

    void AddClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; } 

        // CreatePooledClick(clicks, hit);

        CreateInstantiatedClick(hit);
    }

    void CreatePooledClick(List<GameObject> clicks, RaycastHit hit) 
    {
        for(int i = 0; i < clicks.Count; i++)
        {
            if(!clicks[i].activeInHierarchy)
            {
                clicks[i].transform.position = new Vector3(hit.point.x + 0f, hit.point.y + 1f, hit.point.z - 1f);
                clicks[i].SetActive(true);

                break;
            }
        }
    }

    void CreateInstantiatedClick(RaycastHit hit) 
    {
        GameObject instantiatedClick = Instantiate(click, new Vector3(hit.point.x + 0f, hit.point.y + 0.1f, hit.point.z), click.transform.rotation);

        // not sure if necessary to be networked
        // NetworkServer.Spawn(instantiatedClick, connectionToClient);
    }
}