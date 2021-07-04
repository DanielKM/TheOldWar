using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ReturnToStockpile : IState
{
    private readonly Gatherer _gatherer;
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Animator _animator;
    private readonly Targeter _targeter;

    private static readonly int Speed = Animator.StringToHash("Speed");

    public ReturnToStockpile(Gatherer gatherer, Targeter targeter, NavMeshAgent navMeshAgent, Animator animator)
    {
        _gatherer = gatherer;
        _targeter = targeter;
        _navMeshAgent = navMeshAgent;
        _animator = animator;
    }

    public void Tick()
    {

    }

    public void OnEnter()
    {
        _gatherer.currentState = "RETURN";
        _targeter.TargetClosestDropOff();
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_targeter.GetTarget().transform.position);
        _gatherer.Stockpile = _targeter.GetTarget().GetComponent<ResourceDropOff>();
        _animator.SetFloat(Speed, 1f);
    }

    public void OnExit()
    {
        _navMeshAgent.enabled = false;
        _animator.SetFloat(Speed, 0f);
    }
}
