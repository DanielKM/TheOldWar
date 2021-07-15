using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : IState
{
    private readonly Fighter _fighter;
    private readonly Animator _animator;
    private readonly Targeter _targeter;
    private readonly AudioSource _audioSource;
    private readonly AudioClip _attackClip;
    private int _attackInterval = 2;

    private float _nextAttackTime;
    private static readonly int Harvest = Animator.StringToHash("Harvest");

    public MeleeAttack(Fighter fighter, Targeter targeter, Animator animator, AudioSource audioSource, AudioClip attackClip)
    {
        _fighter = fighter;
        _animator = animator;
        _targeter = targeter;
        _audioSource = audioSource;
        _attackClip = attackClip;
    }

    public void Tick()
    {
        if(_targeter.GetTarget() != null) 
        {
            if(Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + _attackInterval;
                _fighter.DealDamage();
                _animator.SetTrigger(Harvest);
                _audioSource.clip = _attackClip;
                _audioSource.Play();
        Debug.Log("ATTACK");
            }
        }
    }

    public void OnEnter()
    {
        Debug.Log("ATTACKINGGGG");
    }

    public void OnExit()
    {

        Debug.Log("exiting attack");
    }
}
