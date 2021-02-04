using Mirror;
using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour 
{
    [Header("References")]
    [SerializeField] private UnitInformation unitInformation = null;
    private UnitSelectionHandler unitSelection = null;
    private Unit unit = null;
    [SerializeField] public AudioSource unitAudio;
    [SerializeField] public AudioClip unitMovingClip;
    [SerializeField] public AudioClip unitMovingSoundClip;
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnitTask unitTask;

    [Header("Settings")]
    [SerializeField] private float chaseRange = 10;
    [SerializeField] private float dropoffRange = 5;


    // private Animator anim;
    RTSPlayer player = null;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }    
    
    [ClientCallback]
    private void Start() 
    {
        // anim = GetComponent<Animator>();
        unit = GetComponent<Unit>();
        unitTask = GetComponent<UnitTask>();
        unitInformation = gameObject.GetComponent<UnitInformation>();
        unitSelection = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();

        player = connectionToClient != null ? connectionToClient.identity.GetComponent<RTSPlayer>() : null;
    }

    // Change from servercallback to clientcallback
    [ServerCallback]
    private void Update() 
    {
        // POSSIBLE ISSUE FOR LAG
        Targetable target = targeter.GetTarget();

        if(target != null) 
        {   
            // Chase logic
            if((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);

                if(target.TryGetComponent<ResourceDropOff>(out ResourceDropOff resourceDropOff))
                {
                    if(unitTask.GetTask() != ActionList.Delivering && gameObject.GetComponent<ResourceGatherer>() != null) 
                    {
                        unitTask.SetUnitTask(ActionList.Delivering);
                    }

                    if((target.transform.position - transform.position).sqrMagnitude <= dropoffRange * dropoffRange) 
                    {
                        // RESOURCE DROP OFF

                        if(player == null) { return; }

                        Dictionary<Resource, int> newResourceDictionary = player.GetResources();

                        ResourceGatherer gatherer = gameObject.GetComponent<ResourceGatherer>();

                        newResourceDictionary[gatherer.heldResourcesType] += gatherer.ReturnResources();
                        
                        player.SetResources(newResourceDictionary);

                        gatherer.heldResources = 0;

                        if(targeter.GetResourceTarget() == null) { return; }

                        targeter.ServerSetTarget(targeter.GetResourceTarget().gameObject);
                    }
                }
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        } 

        if(unitTask.GetTask() == ActionList.Building || unitTask.GetTask() == ActionList.Construction) 
        {
            unit.gameObject.GetComponent<UnitTask>().SetTask(ActionList.Construction);    

            unit.GetTargeter().CmdSetFoundationTarget();
        }

        if(!agent.hasPath) 
        { 
            if(unitTask.GetTask() != ActionList.Idle)
            {
                unitTask.SetUnitTask(ActionList.Idle);
            }

            return; 
        }
        
        if(unitTask.GetTask() != ActionList.Moving) 
        {
            unitTask.SetUnitTask(ActionList.Moving);
        }
        
        float velocity = agent.velocity.magnitude/agent.speed;

        if(agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath();

        if(unitTask.GetTask() != ActionList.Idle) 
        {
            unitTask.SetUnitTask(ActionList.Idle);
        }
    }

    [Command]
    public void CmdMove(Vector3 position) 
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position) 
    {
        targeter.ClearTarget();

        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion

}


