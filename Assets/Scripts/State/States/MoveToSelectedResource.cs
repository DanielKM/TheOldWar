using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToSelectedResource : IState
{
    private readonly Gatherer _gatherer;
    private readonly Targeter _targeter;
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Animator _animator;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private Vector3 _lastPosition = Vector3.zero;

    public float TimeStuck;

    public MoveToSelectedResource(Gatherer gatherer, Targeter targeter, NavMeshAgent navMeshAgent, Animator animator) 
    {
        _gatherer = gatherer;
        _targeter = targeter;
        _navMeshAgent = navMeshAgent;
        _animator = animator;
    }

    public void Tick()
    {
        if(Vector3.Distance(_gatherer.transform.position, _lastPosition) <= 0f)
            TimeStuck += Time.deltaTime;

        _lastPosition = _gatherer.transform.position;
    }

    public void OnEnter()
    {
        _gatherer.currentState = "FLEE";
        TimeStuck = 0f;
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_targeter.target.transform.position);
        _animator.SetFloat(Speed, 1f);
    }

    public void OnExit()
    {
        _navMeshAgent.enabled = false;
        _animator.SetFloat(Speed, 0f);
    }
}
