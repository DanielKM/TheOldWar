using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchForClosestCorpse : IState
{
    private readonly Gatherer _gatherer;
    private readonly Targeter _targeter;
    private readonly ResourceGatherer _resourceGatherer;
    private readonly GameobjectLists _gameObjectLists;

    public SearchForClosestCorpse(Gatherer gatherer, Targeter targeter, ResourceGatherer resourceGatherer, GameobjectLists gameObjectLists) 
    {
        _gatherer = gatherer;
        _targeter = targeter;
        _resourceGatherer = resourceGatherer;
        _gameObjectLists = gameObjectLists;
    }

    public void Tick()
    {
        Targetable currentTarget = GetClosestCorpse();

        if(!currentTarget) { return; }

        _targeter.CmdSetTarget(currentTarget.gameObject);
    }

    public Targetable GetClosestCorpse()
    {
        Targetable closestCorpse = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = _gatherer.transform.position;

        _targeter.ClearTarget(); 

        foreach(Unit unit in _gameObjectLists.GetAllActiveUnitGameobjects())
        {
            if(unit.TryGetComponent<Corpse>(out Corpse corpse)) 
            {
                Vector3 direction = unit.transform.position - position;

                float distance = direction.sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    closestCorpse = unit.GetComponent<Targetable>();
                }
            }
        }

        return closestCorpse;
    }

    public void OnEnter()
    {
        _gatherer.currentState = "SEARCH";
    }

    public void OnExit()
    {

    }
}
