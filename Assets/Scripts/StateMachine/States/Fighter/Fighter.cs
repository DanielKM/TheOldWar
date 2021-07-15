using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Fighter : MonoBehaviour
{
    public event Action<int> OnAttacked; // switch

    [SerializeField] Animator animator;
    [SerializeField] EnemyDetection enemyDetector;
    [SerializeField] Targeter targeter;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moveAudio;
    [SerializeField] AudioClip pursueAudio;
    [SerializeField] AudioClip attackClip;

    [SerializeField] public Unit unit;

    public string currentState = "STARTING";
    // DAN ADDED 
    private GameobjectLists gameObjectLists;

    private StateMachine _stateMachine;



    void Awake()
    {
        var navMeshAgent = GetComponent<NavMeshAgent>();
        gameObjectLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();
        
        _stateMachine = new StateMachine();

        // DEFAULT STATES
        var idle = new Idle(animator, navMeshAgent);
        var forceMove = new RightClickMove(unit, targeter, navMeshAgent, animator, moveAudio);
        var pursue = new Pursue(unit, navMeshAgent, enemyDetector, animator, pursueAudio);
        var attack = new MeleeAttack(this, targeter, animator, audioSource, attackClip);

        // BREAK TRANSITIONS
        _stateMachine.AddAnyTransition(forceMove, () => unit.forceMove);
        At(forceMove, idle, () => !unit.forceMove);

        _stateMachine.AddAnyTransition(pursue, WasEnemyDetected());
        At(pursue, attack, IsEnemyInRange());
        At(pursue, idle, () => !enemyDetector.enemyDetected);
        
        _stateMachine.SetState(idle);

        // METHODS
        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);

        // PURSUE TREE BOOLEANS
        Func<bool> WasEnemyDetected() => () => enemyDetector.enemyDetected;    
        Func<bool> IsEnemyInRange() => () =>  targeter.GetTarget() != null && navMeshAgent.remainingDistance < 2.2f;
    }

    private void Update() => _stateMachine.Tick();

    public void DealDamage() 
    {
        Health enemyHealth = targeter.GetTarget().GetComponent<Health>();
        
        if(!enemyHealth) { return; }

        enemyHealth.DealDamage(10);

        OnAttacked?.Invoke(1);
    }
}
