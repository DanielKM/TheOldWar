using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Idle : IState
{
    private readonly Gatherer _gatherer;
    private readonly Animator _animator;
    private readonly NavMeshAgent _navMeshAgent;
    
    private static readonly int Speed = Animator.StringToHash("Speed");

    public Idle(Gatherer gatherer, Animator animator, NavMeshAgent navMeshAgent)
    {
        _gatherer = gatherer;
        _animator = animator;
        _navMeshAgent = navMeshAgent;
    }

    public void Tick()
    {

    }

    public void OnEnter()
    {
        _gatherer.currentState = "IDLE";
        _navMeshAgent.enabled = false;
        _animator.SetFloat(Speed, 0f);
    }

    public void OnExit()
    {

    }
}
