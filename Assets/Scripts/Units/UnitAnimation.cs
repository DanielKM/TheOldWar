using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitAnimation : NetworkBehaviour
{
    [Header("References")]
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
        if(task == ActionList.Moving || task == ActionList.Attacking || task == ActionList.Gathering || task == ActionList.Delivering || task == ActionList.Construction) 
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Idle) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Fighting || task == ActionList.Building || task == ActionList.Harvesting) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", true);
        } else if (task == ActionList.Injured || task == ActionList.Dead ) 
        {
            anim.SetBool("isInjured", true);
        }
    }

    #endregion
}
