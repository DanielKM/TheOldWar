using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchForClosestFoundation : IState
{
    private readonly Gatherer _gatherer;
    private readonly Targeter _targeter;
    private readonly ResourceGatherer _resourceGatherer;
    private readonly GameobjectLists _gameObjectLists;

    public SearchForClosestFoundation(Gatherer gatherer, Targeter targeter, ResourceGatherer resourceGatherer, GameobjectLists gameObjectLists) 
    {
        _gatherer = gatherer;
        _targeter = targeter;
        _resourceGatherer = resourceGatherer;
        _gameObjectLists = gameObjectLists;
    }

    public void Tick()
    {
        Targetable currentTarget = GetClosestFoundation();

        if(!currentTarget) { return; }

        _targeter.CmdSetTarget(currentTarget.gameObject);
    }

    public Targetable GetClosestFoundation()
    {
        Targetable closestFoundation = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = _gatherer.transform.position;
        RTSPlayer player = _gatherer.GetComponent<UnitInformation>().owner;

        _targeter.ClearTarget(); 

        foreach(Building building in player.GetMyBuildings())
        {
            if(building.TryGetComponent<Foundation>(out Foundation foundation)) 
            {
                Vector3 direction = building.transform.position - position;

                float distance = direction.sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    closestFoundation = building.GetComponent<Targetable>();
                }
            }
        }

        return closestFoundation;
    }

    public void OnEnter()
    {
        _gatherer.currentState = "SEARCH";
    }

    public void OnExit()
    {

    }
}
