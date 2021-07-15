using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearDead : IState
{
    private readonly Gatherer _gatherer;
    private readonly Animator _animator;
    private readonly Targeter _targeter;
    private readonly AudioSource _audioSource;
    private readonly AudioClip _clearDeadClip;
    private int _buildInterval = 2;

    private float _nextBuildTime;
    private static readonly int Harvest = Animator.StringToHash("Harvest");

    public ClearDead(Gatherer gatherer, Targeter targeter, Animator animator, AudioSource audioSource, AudioClip clearDeadClip)
    {
        _gatherer = gatherer;
        _animator = animator;
        _targeter = targeter;
        _audioSource = audioSource;
        _clearDeadClip = clearDeadClip;
    }

    public void Tick()
    {
        if(_targeter.GetTarget() != null) 
        {
            if(Time.time >= _nextBuildTime)
            {
                _nextBuildTime = Time.time + _buildInterval;
                _gatherer.ClearDead();
                _animator.SetTrigger(Harvest);
                _audioSource.clip = _clearDeadClip;
                _audioSource.Play();
            }
        }
    }

    public void OnEnter()
    {
        _gatherer.currentState = "BUILD";
    }

    public void OnExit()
    {

    }
}
