using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gatherer : MonoBehaviour
{
    public event Action<int> OnGatheredChanged; // switch
    public event Action<int> onBuildProgressChanged; // switch
    public event Action<int> onClearDeadProgressChanged; // switch

    [SerializeField] Animator animator;
    [SerializeField] EnemyDetection enemyDetector;
    [SerializeField] Targeter targeter;
    [SerializeField] ResourceGatherer resourceGatherer;
    [SerializeField] UnitInformation unitInformation;
    [SerializeField] UnitTask unitTask;
    [SerializeField] public Unit unit;

    [SerializeField] AudioClip fleeAudio;
    [SerializeField] AudioClip moveAudio;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip harvestClip;
    [SerializeField] AudioClip buildClip;

    public string currentState = "idle";
    // DAN ADDED 
    private GameobjectLists gameObjectLists;

    private StateMachine _stateMachine;
    private int _gathered;
    private int _built;
    private int _cleared;

    public ResourceDropOff Stockpile { get; set; }
    
    // Resource Type
    // Resource Node
    // Stockpile
    // Enemy 
    // MaxResources
    // CurrentResources

    void Awake()
    {
        var navMeshAgent = GetComponent<NavMeshAgent>();
        gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();

        _stateMachine = new StateMachine();

        // DEFAULT STATES
        var idle = new Idle(animator, navMeshAgent);

        // HARVEST STATES
        var searchResources = new SearchForClosestResource(this, targeter, resourceGatherer, gameObjectLists);
        var moveToSelectedResource = new MoveToSelectedTarget(unit, targeter, navMeshAgent, animator);
        var harvest = new HarvestResource(this, targeter, animator, audioSource, harvestClip);
        var returnToStockpile = new ReturnToStockpile(this, targeter, navMeshAgent, animator);
        var placeResourcesInStockpile = new PlaceResourcesInStockpile(this);

        // BUILD STATES
        var searchFoundations = new SearchForClosestFoundation(this, targeter, resourceGatherer, gameObjectLists);
        var moveToSelectedFoundation = new MoveToSelectedTarget(unit, targeter, navMeshAgent, animator);
        var construct = new ConstructFoundation(this, targeter, animator, audioSource, buildClip);

        // CLEAR DEAD STATES
        var searchCorpses = new SearchForClosestCorpse(this, targeter, resourceGatherer, gameObjectLists);
        var moveToSelectedCorpse = new MoveToSelectedTarget(unit, targeter, navMeshAgent, animator);
        var clearDead = new ClearDead(this, targeter, animator, audioSource, buildClip);

        // BREAK STATES
        var flee = new Flee(unit, navMeshAgent, enemyDetector, animator, fleeAudio);
        var forceMove = new RightClickMove(unit, targeter, navMeshAgent, animator, moveAudio);

        // ADD CLEAR ALL COMMANDS

        // HARVEST TRANSITIONS
        At(idle, searchResources, WasGatherCommandGiven());
        At(searchResources, moveToSelectedResource, HasGatherTarget());
        At(moveToSelectedResource, searchResources, StuckForOverASecond());
        At(moveToSelectedResource, harvest, ReachedResourceTarget());
        At(harvest, searchResources, TargetIsDepletedAndICanCarryMore());
        At(harvest, returnToStockpile, InventoryFull());
        At(returnToStockpile, placeResourcesInStockpile, ReachedStockpile());
        At(placeResourcesInStockpile, searchResources, () => resourceGatherer.heldResources == 0);

        // BUILDING TRANSITIONS
        At(idle, searchFoundations, WasBuildCommandGiven());
        At(searchFoundations, moveToSelectedFoundation, HasBuildTarget());
        At(moveToSelectedFoundation, searchFoundations, StuckForOverASecond());
        At(moveToSelectedFoundation, construct, ReachedFoundationTarget());
        At(construct, searchFoundations, ConstructionComplete());

        // CLEAR DEAD TRANSITIONS
        At(idle, searchCorpses, WasClearDeadCommandGiven());
        At(searchCorpses, moveToSelectedCorpse, HasClearDeadTarget());
        At(moveToSelectedCorpse, searchCorpses, StuckForOverASecond());
        At(moveToSelectedCorpse, clearDead, ReachedClearDeadTarget());
        At(clearDead, searchCorpses, ClearDeadComplete());

        // BREAK TRANSITIONS
        _stateMachine.AddAnyTransition(forceMove, () => unit.forceMove);
        At(forceMove, idle, () => !unit.forceMove);

        _stateMachine.AddAnyTransition(flee, () => enemyDetector.enemyDetected);
        At(flee, idle, () => !enemyDetector.enemyDetected);

        // _stateMachine.AddAnyTransition(searchFoundations, () => unitTask.GetTask() == ActionList.Construction);
        // At(searchFoundations, idle, () => unitTask.GetTask() != ActionList.Construction);

        // _stateMachine.AddAnyTransition(searchResources, () => unitTask.GetTask() == ActionList.Gathering);
        // At(searchResources, idle, () => unitTask.GetTask() != ActionList.Gathering);

        _stateMachine.SetState(idle);

        // METHODS
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);

        // HARVEST BOOLEANS
        Func<bool> WasGatherCommandGiven() => () => unitTask.GetTask() == ActionList.Gathering;    
        Func<bool> HasGatherTarget() => () => targeter.GetTarget() != null;
        Func<bool> StuckForOverASecond() => () => moveToSelectedResource.TimeStuck > 1f;
        Func<bool> ReachedResourceTarget() => () => targeter.GetTarget() != null && Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f && targeter.GetTarget().GetComponent<ResourceNode>();
        Func<bool> TargetIsDepletedAndICanCarryMore() => () => (targeter.GetTarget() == null || targeter.GetTarget().GetComponent<ResourceNode>().heldResources <= 0) && !InventoryFull().Invoke();
        Func<bool> InventoryFull() => () => resourceGatherer.heldResources >= resourceGatherer.maxHeldResources;
        Func<bool> ReachedStockpile() => () => targeter.GetTarget() != null &&
                                            Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f;
                                            
        // BUILD BOOLEANS
        Func<bool> WasBuildCommandGiven() => () => unitTask.GetTask() == ActionList.Construction;    
        Func<bool> HasBuildTarget() => () => targeter.GetTarget() != null;
        Func<bool> ReachedFoundationTarget() => () => targeter.GetTarget() != null && Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f && targeter.GetTarget().GetComponent<Foundation>();
        Func<bool> ConstructionComplete() => () => targeter.GetTarget() == null;
                                           
        // CLEAR DEAD BOOLEANS
        Func<bool> WasClearDeadCommandGiven() => () => unitTask.GetTask() == ActionList.ClearingDead;    
        Func<bool> HasClearDeadTarget() => () => targeter.GetTarget() != null;
        Func<bool> ReachedClearDeadTarget() => () => targeter.GetTarget() != null && Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f && targeter.GetTarget().GetComponent<Corpse>();
        Func<bool> ClearDeadComplete() => () => targeter.GetTarget() == null;
    }

    private void Update() => _stateMachine.Tick();

    public void TakeFromTarget() 
    {
        ResourceNode resourceNode = targeter.GetTarget().GetComponent<ResourceNode>();

        resourceGatherer.AddResources(10, resourceNode.GetResourceType());

        resourceNode.TakeResources(10);

        OnGatheredChanged?.Invoke(_gathered);
    }

    public void DropResourcesOff() 
    {
        Dictionary<Resource, int> newResourceDictionary = unitInformation.owner.GetResources();

        newResourceDictionary[resourceGatherer.heldResourcesType] += resourceGatherer.ReturnResources();

        unitInformation.owner.SetResources(newResourceDictionary);

        resourceGatherer.heldResources = 0;

        OnGatheredChanged?.Invoke(_gathered);
    }
    
    public void BuildFoundation() 
    {
        Foundation foundation = targeter.GetTarget().GetComponent<Foundation>();

        foundation.SetProgress(10);

        onBuildProgressChanged?.Invoke(_built);
    }

    public void ClearDead() 
    {
        GameObject corpse = targeter.GetTarget().gameObject;

        Destroy(corpse);

        onClearDeadProgressChanged?.Invoke(_cleared);
    }
}
