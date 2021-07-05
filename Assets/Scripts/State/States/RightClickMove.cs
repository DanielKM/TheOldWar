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

    private static readonly int Speed = Animator.StringToHash("Speed");

    private Vector3 _lastPosition = Vector3.zero;

    public float TimeStuck;

    public RightClickMove(Gatherer gatherer, NavMeshAgent navMeshAgent, Animator animator, AudioClip audioClip)
    {
        _gatherer = gatherer;
        _navMeshAgent = navMeshAgent;
        _animator = animator;
        _audioClip = audioClip;
    }

    public void Tick()
    {
        if(_navMeshAgent.remainingDistance < 0.5f)
        { 
            _gatherer.unit.forceMove = false;
        }

        if(Vector3.Distance(_gatherer.transform.position, _lastPosition) <= 0f)
            TimeStuck += Time.deltaTime;

        _lastPosition = _gatherer.transform.position;
    }

    public void OnEnter()
    {
        Debug.Log("ENTERED");
        _gatherer.currentState = "MOVING";
        TimeStuck = 0f;
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_gatherer.unit.selectedDestination);
        _animator.SetFloat(Speed, 1f);
    }

    public void OnExit()
    {
        Debug.Log("EXITED");
        // _gatherer.unit.selectedDestination = selectedDestination; // maybe change to same destination?
        _navMeshAgent.enabled = false;
        _animator.SetFloat(Speed, 0f);
    }
}
