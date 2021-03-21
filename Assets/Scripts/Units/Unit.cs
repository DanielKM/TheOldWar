using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private UnitInformation unitInformation = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] public AudioSource unitAudio;
    [SerializeField] private UnitTask unitTask;
    [SerializeField] public AudioClip unitReportingClip;
    [SerializeField] public AudioClip unitSelectedClip;
    [SerializeField] public GameObject corpseSkeleton;
    [SerializeField] public GameObject model;
    [SerializeField] public Vector3 boxColliderSize;
    public Sprite unitIcon;
    
    [Header("Settings")]
    [SerializeField] private int id = -1;
    [SerializeField] private int gold = 0;
    [SerializeField] private int iron = 0;
    [SerializeField] private int steel = 0;
    [SerializeField] private int skymetal = 0;
    [SerializeField] private int wood = 0;
    [SerializeField] private int stone = 0;
    [SerializeField] private int food = 0;
    [SerializeField] private int population = 0;

    [Header("Events")]
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    
    [TextArea]
    public string description;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public Dictionary<Resource, int> GetPrice()
    {
        Dictionary<Resource, int> unitPrice = new Dictionary<Resource, int>(){ 
            {Resource.Gold, gold}, 
            {Resource.Iron, iron}, 
            {Resource.Steel, steel}, 
            {Resource.Skymetal, skymetal}, 
            {Resource.Wood, wood}, 
            {Resource.Stone, stone}, 
            {Resource.Food, food}, 
            {Resource.Population, population}, 
        }; 
        return unitPrice;
    }

    public string GetDescription()
    {
        return description;
    }

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }
    
    public int GetId()
    {
        return id;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);

        health.ServerOnDie += ServerHandleDie;

        health.ServerOnInjured += ServerHandleUnitInjured;

        health.ServerOnHealed += ServerHandleUnitHealed;

        boxColliderSize = new Vector3(gameObject.GetComponent<BoxCollider>().size.x, gameObject.GetComponent<BoxCollider>().size.y, gameObject.GetComponent<BoxCollider>().size.z);

        if(unitInformation.owner != null)
        {
            GameObject x = GameObject.Find("UnitHandlers");
          
            x.GetComponent<GameobjectLists>().units.Add(this);

            unitInformation.owner.myActiveUnits.Add(this);
        
            Material[] materialArray = { unitInformation.owner.teamMaterial };

            foreach (Transform child in model.transform)
            {
                if(child.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer))
                {
                    renderer.materials = materialArray;
                }
            }
        }
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        
        health.ServerOnDie -= ServerHandleDie;
        
        health.ServerOnInjured -= ServerHandleUnitInjured;

        health.ServerOnHealed -= ServerHandleUnitHealed;
    }

    [Server]
    private void ServerHandleDie()
    {
        Dictionary<Resource, int> refundPrice = new Dictionary<Resource, int>(){ 
            {Resource.Gold, 0}, 
            {Resource.Iron, 0}, 
            {Resource.Steel, 0}, 
            {Resource.Skymetal, 0}, 
            {Resource.Wood, 0}, 
            {Resource.Stone, 0}, 
            {Resource.Food, 0}, 
            {Resource.Population, -population}, 
        }; 
        
        GameObject unitHandlers = GameObject.Find("UnitHandlers");

        if(unitHandlers != null) 
        { 
            unitHandlers.GetComponent<GameobjectLists>().units.Remove(this);
        }
        
        if(unitInformation.owner == null ) { return; }

        unitInformation.owner.myActiveUnits.Remove(this);

        unitInformation.owner.SetResources(unitInformation.owner.SubtractPrice(refundPrice));
        
        // if(unitInformation.owner == null) 
        // { 
        //     gameObject.SetActive(false);
            
        //     return;
        // }

        // gameObject.SetActive(false);

        if(corpseSkeleton)
        {
            GameObject skeleton = Instantiate(corpseSkeleton, this.transform.position, this.transform.rotation);

            skeleton.GetComponent<UnitTask>().SetTask(ActionList.Injured);

            NetworkServer.Spawn(skeleton, connectionToClient);
        }

        NetworkServer.Destroy(gameObject);

        Destroy(gameObject);
    }

    [Server]
    public void ServerHandleUnitInjured()
    {
        unitTask.SetTask(ActionList.Injured);

        gameObject.GetComponent<UnitFiring>().enabled = false;
        gameObject.GetComponent<Targeter>().enabled = false;
        gameObject.GetComponent<NavMeshAgent>().enabled = false;
        gameObject.GetComponent<UnitMovement>().enabled = false;
        gameObject.GetComponent<Targeter>().ClearTarget();

        if(gameObject.TryGetComponent<Huntable>(out Huntable huntable))  
        {
            ResourceNode spawnedNode = gameObject.AddComponent<ResourceNode>();
            spawnedNode.resource = Resource.Food;
            spawnedNode.heldResources = 1000;
            spawnedNode.health = gameObject.GetComponent<Health>();
            spawnedNode.enabled = true;

            return;
        }

        gameObject.GetComponent<BoxCollider>().size = new Vector3(0, 0, 0);

        System.Random r = new System.Random();
        int timeToDie = r.Next(0, 120);

        StartCoroutine(Decay(timeToDie));
        // enemy detection off
    }

    [Server]
    public void ServerHandleUnitHealed()
    {
        Debug.Log("Healed!");

        unitTask.SetTask(ActionList.Idle);

        gameObject.GetComponent<UnitFiring>().enabled = true;
        gameObject.GetComponent<Targeter>().enabled = true;
        gameObject.GetComponent<NavMeshAgent>().enabled = true;
        gameObject.GetComponent<UnitMovement>().enabled = true;
        gameObject.GetComponent<BoxCollider>().size = new Vector3(boxColliderSize.x, boxColliderSize.y, boxColliderSize.z);

        gameObject.GetComponent<UnitFiring>().enabled = true;
    }

    [Server]
    public IEnumerator Decay(int timeToDeath)
    {
        yield return new WaitForSeconds(timeToDeath);

        if(unitTask.GetTask() == ActionList.Injured) 
        {
            health.ServerDie();
        }
    }
    
    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        GameObject unitHandlers = GameObject.Find("UnitHandlers");

        AuthorityOnUnitSpawned?.Invoke(this);

        unitAudio.clip = unitReportingClip;

        unitAudio.Play();

        // if(unitHandlers != null) 
        // { 
        //     unitHandlers.GetComponent<GameobjectLists>().units.Add(this);
        // }

        // unitInformation.owner.myActiveUnits.Add(this);
    }


    // public void OnEnable()
    // {   
    //     GameObject unitHandlers = GameObject.Find("UnitHandlers");

    //     AuthorityOnUnitSpawned?.Invoke(this);

    //     // unitAudio.clip = unitReportingClip;

    //     // unitAudio.Play();

    //     StartCoroutine(CycleSphereColliders());

    //     if(unitHandlers != null) 
    //     { 
    //         unitHandlers.GetComponent<GameobjectLists>().units.Add(this);
    //     }
        
    //     if(unitInformation.owner == null ) { return; }

    //     unitInformation.owner.myActiveUnits.Add(this);
    // }

    // public void OnDisable()
    // {        
    //     GameObject unitHandlers = GameObject.Find("UnitHandlers");

    //     if(unitHandlers != null) 
    //     { 
    //         unitHandlers.GetComponent<GameobjectLists>().units.Remove(this);
    //     }
        
    //     if(unitInformation.owner == null ) { return; }

    //     unitInformation.owner.myActiveUnits.Remove(this);

    // }

    public override void OnStopClient()
    {
        if(!hasAuthority) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select() 
    {
        if(!hasAuthority) { return; }

        onSelected?.Invoke();
        
        unitInformation.selected = true;
    }

    [Client]
    public void Deselect()
    {
        if(!hasAuthority) { return; }

        onDeselected?.Invoke();
        
        unitInformation.selected = false;
    }

    #endregion
}
