using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Technology
{
    public enum AvailabilityState { Locked, Unlocked, Researching, Learned }
    public AvailabilityState availabilityState;
    public string techName;
    public string techDescription;
    public Sprite techImage;
    public List<ResourceAmount> resourceCosts;
    public List<RequiredTech> techRequirements;

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
}

public class ResourceAmount
{
    public string resourceName;
    public int resourceAmount;
}

public class RequiredTech
{
    public string techName;
    public bool completed;
}

