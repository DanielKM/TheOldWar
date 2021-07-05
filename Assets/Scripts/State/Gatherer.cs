using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gatherer : MonoBehaviour
{
    public event Action<int> OnGatheredChanged; // switch
    [SerializeField] private int _maxCarried = 20; // switch
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

    public string currentState = "idle";
    // DAN ADDED 
    private GameobjectLists gameObjectLists;

    private StateMachine _stateMachine;
    private int _resourceType = 0;
    private int _gathered;

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

        // STATES
        var idle = new Idle(this, animator, navMeshAgent);
        var search = new SearchForClosestResource(this, targeter, resourceGatherer, gameObjectLists);
        var moveToSelectedResource = new MoveToSelectedResource(this, targeter, navMeshAgent, animator);
        var harvest = new HarvestResource(this, targeter, animator, audioSource, harvestClip);
        // eventually add "SearchForClosestStockpile -> code in returntostockpile
        var returnToStockpile = new ReturnToStockpile(this, targeter, navMeshAgent, animator);
        var placeResourcesInStockpile = new PlaceResourcesInStockpile(this);

        // BREAK STATES
        var flee = new Flee(this, navMeshAgent, enemyDetector, animator, fleeAudio);
        var forceMove = new RightClickMove(this, navMeshAgent, animator, moveAudio);

        // TRANSITIONS
        At(idle, search, WasGatherCommandGiven());
        At(search, moveToSelectedResource, HasTarget());
        At(moveToSelectedResource, search, StuckForOverASecond());
        At(moveToSelectedResource, harvest, ReachedResource());
        At(harvest, search, TargetIsDepletedAndICanCarryMore());
        At(harvest, returnToStockpile, InventoryFull());
        At(returnToStockpile, placeResourcesInStockpile, ReachedStockpile());
        At(placeResourcesInStockpile, search, () => resourceGatherer.heldResources == 0);

        // AddAnyTransition breaks up the normal flow
        _stateMachine.AddAnyTransition(forceMove, () => unit.forceMove);
        At(forceMove, idle, ArrivedAtMoveDestination());

        _stateMachine.AddAnyTransition(flee, () => enemyDetector.enemyDetected);
        At(flee, idle, () => !enemyDetector.enemyDetected);

        _stateMachine.SetState(idle);

        // METHODS
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);

        Func<bool> ArrivedAtMoveDestination() => () => Vector3.Distance(transform.position, unit.selectedDestination) < 1;    
        Func<bool> WasGatherCommandGiven() => () => unitTask.GetTask() == ActionList.Gathering;    
        Func<bool> HasTarget() => () => targeter.GetTarget() != null;
        Func<bool> StuckForOverASecond() => () => moveToSelectedResource.TimeStuck > 1f;
        Func<bool> ReachedResource() => () => targeter.GetTarget() != null &&
                                            Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f;
        Func<bool> TargetIsDepletedAndICanCarryMore() => () => (targeter.GetTarget() == null || targeter.GetTarget().GetComponent<ResourceNode>().heldResources <= 0) && !InventoryFull().Invoke();
        Func<bool> InventoryFull() => () => resourceGatherer.heldResources >= resourceGatherer.maxHeldResources;
        Func<bool> ReachedStockpile() => () => targeter.GetTarget() != null &&
                                            Vector3.Distance(transform.position, targeter.GetTarget().transform.position) < 2.2f;
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
}
