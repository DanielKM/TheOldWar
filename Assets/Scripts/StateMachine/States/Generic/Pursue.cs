using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pursue : IState
{
    private readonly Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private readonly EnemyDetection _enemyDetection;
    private Animator _animator;
    private readonly AudioClip _audioClip;
    private static readonly int FleeHash = Animator.StringToHash("Flee");

    private float _initialSpeed;
    private const float PURSUE_SPEED = 2f;
    private const float PURSUE_DISTANCE = 1f;

    public Pursue(Unit unit, NavMeshAgent navMeshAgent, EnemyDetection enemyDetection, Animator animator, AudioClip audioClip)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _enemyDetection = enemyDetection;
        _animator = animator;
        _audioClip = audioClip;
    }

    public void Tick()
    {
        // if(_navMeshAgent.remainingDistance < 2)
        // {

        // }

        _navMeshAgent.SetDestination(_enemyDetection.closestEnemy.transform.position);
    }
    
    public void OnEnter()
    {
        Debug.Log("Entering Pursue");
        // _navMeshAgent.enabled = true;
        _animator.SetBool(FleeHash, true);
        _initialSpeed = _navMeshAgent.speed;
        _navMeshAgent.speed = PURSUE_SPEED;
        // PLAY AUDIO
    }

    public void OnExit()
    {
        Debug.Log("Exiting Pursue");
        _navMeshAgent.speed = _initialSpeed;
        _navMeshAgent.ResetPath();
        // _navMeshAgent.enabled = false;
        _animator.SetBool(FleeHash, false);

    }
}
