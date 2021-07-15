using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceResourcesInStockpile : IState
{
    public readonly Gatherer _gatherer;

    public PlaceResourcesInStockpile(Gatherer gatherer)
    {
        _gatherer = gatherer;
    }

    public void OnEnter()
    {
        _gatherer.currentState = "DELIVER";
    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
        _gatherer.DropResourcesOff();
    }
}
