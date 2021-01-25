﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitAnimation : NetworkBehaviour
{
    [SerializeField]
    private Animator anim;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    #region Client

    [ServerCallback]
    public void SetAnimation(ActionList task)
    {   
        if(task == ActionList.Moving) 
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Idle) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Attacking) 
        {
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Fighting) 
        {
            anim.SetBool("isFiring", true);
        } else if (task == ActionList.Delivering || task == ActionList.Gathering ) 
        {
            anim.SetBool("isFiring", false);
        }
    }

    #endregion
}
