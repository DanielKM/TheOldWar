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
    private ResourceGatherer gatherer;

    private bool isGatherer = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if(gameObject.TryGetComponent<ResourceGatherer>(out ResourceGatherer unitGatherer)) 
        {   
            isGatherer = true;
            gatherer = GetComponent<ResourceGatherer>();
        }
        else 
        {
            isGatherer = false;
        }
    }

    #region Client

    [ServerCallback]
    public void SetAnimation(ActionList task)
    {   
        if(task == ActionList.ClearingDead || task == ActionList.Moving || task == ActionList.Attacking || task == ActionList.Gathering || task == ActionList.Delivering || task == ActionList.Construction) 
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.Idle) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", false);
            if(gameObject.TryGetComponent<Necromancer>(out Necromancer necro))
            {
                anim.SetBool("isCastingAOE", false);
            }
        } else if (task == ActionList.Fighting || task == ActionList.Building || task == ActionList.Harvesting || task == ActionList.Destroying) 
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", true);
        } else if (task == ActionList.Injured || task == ActionList.Dead ) 
        {
            anim.SetBool("isInjured", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isFiring", false);
        } else if (task == ActionList.CastingAOE)
        {
            anim.SetBool("isCastingAOE", true);
        }

        if(isGatherer && gatherer.heldResources > 0)
        {
            if(gatherer.heldResourcesType == Resource.Wood)
            {
                anim.SetBool("hasWood", true);
            } else {
                anim.SetBool("hasBag", true);
            }
        } else if(isGatherer && gatherer.heldResources <= 0) {
            anim.SetBool("hasBag", false);
            anim.SetBool("hasWood", false);
        }
    }

    #endregion
}
