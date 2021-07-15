using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flee : IState
{
    private readonly Unit _unit;
    private NavMeshAgent _navMeshAgent;
    private readonly EnemyDetection _enemyDetection;
    private Animator _animator;
    private readonly AudioClip _audioClip;
    private static readonly int FleeHash = Animator.StringToHash("Flee");

    private float _initialSpeed;
    private const float FLEE_SPEED = 4f;
    private const float FLEE_DISTANCE = 5f;

    public Flee(Unit unit, NavMeshAgent navMeshAgent, EnemyDetection enemyDetection, Animator animator, AudioClip audioClip)
    {
        _unit = unit;
        _navMeshAgent = navMeshAgent;
        _enemyDetection = enemyDetection;
        _animator = animator;
        _audioClip = audioClip;
    }

    public void Tick()
    {
        if(_navMeshAgent.remainingDistance < 1f)
        { 
            var away = GetRandomPoint();
            _navMeshAgent.SetDestination(away);
        }
    }
    
    public void OnEnter()
    {
        // _navMeshAgent.enabled = true;
        _animator.SetBool(FleeHash, true);
        _initialSpeed = _navMeshAgent.speed;
        _navMeshAgent.speed = FLEE_SPEED;
        // PLAY AUDIO
    }

    private Vector3 GetRandomPoint()
    {
        Vector3 directionFromEnemy = _unit.transform.position - _enemyDetection.closestEnemy.transform.position;
        directionFromEnemy.Normalize();

        Vector3 endPoint = _unit.transform.position + (directionFromEnemy * FLEE_DISTANCE);
        if(NavMesh.SamplePosition(endPoint, out var hit, 10f, NavMesh.AllAreas))
        { 
            return hit.position;
        }

        return _unit.transform.position;
    }

    public void OnExit()
    {
        _navMeshAgent.speed = _initialSpeed;
        // _navMeshAgent.enabled = false;
        _animator.SetBool(FleeHash, false);
    }
}
