using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RightClickMove : IState
{
    private readonly Unit _unit;
    private Targeter _targeter;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private readonly AudioClip _audioClip;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private Vector3 _lastPosition = Vector3.zero;

    public float TimeStuck;

    public RightClickMove(Unit unit, Targeter targeter, NavMeshAgent navMeshAgent, Animator animator, AudioClip audioClip)
    {
        _unit = unit;
        _targeter = targeter;
        _navMeshAgent = navMeshAgent;
        _animator = animator;
        _audioClip = audioClip;
    }

    public void Tick()
    {
        if(_unit.selectedDestination != _navMeshAgent.destination) 
        {
            _navMeshAgent.SetDestination(_unit.selectedDestination);
        }

        if(Vector3.Distance(_unit.transform.position, _unit.selectedDestination) < 0.2f && _unit.forceMove == true) 
        {
            _unit.forceMove = false;
        }

        if(Vector3.Distance(_unit.transform.position, _lastPosition) <= 0f)
            TimeStuck += Time.deltaTime;

        _lastPosition = _unit.transform.position;
    }

    public void OnEnter()
    {
        TimeStuck = 0f;
        // _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_unit.selectedDestination);
        _animator.SetFloat(Speed, 1f);
    }

    public void OnExit()
    {
        // _gatherer.unit.selectedDestination = selectedDestination; // maybe change to same destination?
        // _navMeshAgent.enabled = false;
        _unit.forceMove = false;
        _navMeshAgent.ResetPath();
        _animator.SetFloat(Speed, 0f);
    }
}
