using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RightClickMove : IState
{
    private readonly Gatherer _gatherer;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private readonly AudioClip _audioClip;
    private static readonly int FleeHash = Animator.StringToHash("Move");

    public RightClickMove(Gatherer gatherer, NavMeshAgent navMeshAgent, Animator animator, AudioClip audioClip)
    {
        _gatherer = gatherer;
        _navMeshAgent = navMeshAgent;
        _animator = animator;
        _audioClip = audioClip;
    }

    public void Tick()
    {
        // if(_navMeshAgent.remainingDistance < 1f)
        // { 
        //     // _navMeshAgent.SetDestination();
        // }
    }

    public void OnEnter()
    {
        _gatherer.currentState = "MOVING";
    }

    public void OnExit()
    {

    }
}
