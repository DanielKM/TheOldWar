using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestResource : IState
{
    private readonly Gatherer _gatherer;
    private readonly Animator _animator;
    private readonly Targeter _targeter;
    private readonly AudioSource _audioSource;
    private readonly AudioClip _harvestClip;
    private int _harvestInterval = 2;

    private float _nextTakeResourceTime;
    private static readonly int Harvest = Animator.StringToHash("Harvest");

    public HarvestResource(Gatherer gatherer, Targeter targeter, Animator animator, AudioSource audioSource, AudioClip harvestClip)
    {
        _gatherer = gatherer;
        _animator = animator;
        _targeter = targeter;
        _audioSource = audioSource;
        _harvestClip = harvestClip;
    }

    public void Tick()
    {
        if(_targeter.GetTarget() != null) 
        {
            if(Time.time >= _nextTakeResourceTime)
            {
                _nextTakeResourceTime = Time.time + _harvestInterval;
                _gatherer.TakeFromTarget();
                _animator.SetTrigger(Harvest);
                _audioSource.clip = _harvestClip;
                _audioSource.Play();
            }
        }
    }

    public void OnEnter()
    {
        _gatherer.currentState = "HARVEST";
    }

    public void OnExit()
    {

    }
}
