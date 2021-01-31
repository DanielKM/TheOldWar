using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Technology
{
    public enum AvailabilityState { Locked, Unlocked, Researching, Learned }
    public AvailabilityState availabilityState;
    public string techName;
    public string techDescription;
    public Sprite techImage;
    public List<ResourceAmount> resourceCosts;
    public List<RequiredTech> techRequirements;

    [SerializeField]
    private int researchedTurns;
    public int requiredResearchTurns;

    public void Unlock()
    {
        availabilityState = AvailabilityState.Unlocked;
        Debug.Log(this.techName + " was unlocked!");
    }
    
    public void Learn()
    {
        availabilityState = AvailabilityState.Learned;
        Debug.Log(this.techName + " was learned!");
    }

    public void NewTurn()
    {
        if(availabilityState == AvailabilityState.Researching)
        {
            researchedTurns++;
            if(researchedTurns >= requiredResearchTurns) 
            {
                Learn();
            }
        }
    }

    private void CheckRequirements(Technology tech)
    {
        if(availabilityState == AvailabilityState.Locked)
        {
            RequiredTech requiredTech = techRequirements.FirstOrDefault(rt=>rt.techName == tech.techName);
            if(requiredTech != null && !requiredTech.completed)
            {
                requiredTech.completed = true;
                if(!techRequirements.Any(t=>t.completed == false))
                {
                    Unlock();
                }
            }
        }
    }
}

[System.Serializable]
public class RequiredTech
{
    public string techName;
    public bool completed;
}

