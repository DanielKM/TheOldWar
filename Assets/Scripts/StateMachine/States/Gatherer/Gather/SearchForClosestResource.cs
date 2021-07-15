using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchForClosestResource : IState
{
    private readonly Gatherer _gatherer;
    private readonly Targeter _targeter;
    private readonly ResourceGatherer _resourceGatherer;
    private readonly GameobjectLists _gameObjectLists;

    public SearchForClosestResource(Gatherer gatherer, Targeter targeter, ResourceGatherer resourceGatherer, GameobjectLists gameObjectLists) 
    {
        _gatherer = gatherer;
        _targeter = targeter;
        _resourceGatherer = resourceGatherer;
        _gameObjectLists = gameObjectLists;
    }

    public void Tick()
    {
        _targeter.CmdSetTarget(GetClosestResource(GetResourceID(_resourceGatherer.heldResourcesType)).gameObject);
    }

    public Targetable GetClosestResource(int resourceID)
    {
        Targetable closestResourceNode = null;
        float closestDistance = Mathf.Infinity;
        Vector3 position = _gatherer.transform.position;

        _targeter.ClearTarget(); 

        foreach(ResourceNode node in _gameObjectLists.GetAllActiveResourceNodes())
        {
            if(node.TryGetComponent<ResourceNode>(out ResourceNode resourceNode)) 
            {
                if(resourceNode.enabled) 
                {
                    int nodeID = GetResourceID(resourceNode.GetResourceType());

                    if(nodeID != resourceID) { continue; }

                    Vector3 direction = node.transform.position - position;

                    float distance = direction.sqrMagnitude;

                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        
                        closestResourceNode = node.GetComponent<Targetable>();
                    }
                }
            }
        }

        return closestResourceNode;
    }

    public void OnEnter()
    {
        _gatherer.currentState = "SEARCH";
    }

    public void OnExit()
    {

    }

    public int GetResourceID(Resource selectedResource)
    {
        switch (selectedResource)
        {
            case Resource.Gold:
                return 0;
            case Resource.Iron:
                return 1;
            case Resource.Steel:
                return 2;
            case Resource.Skymetal:
                return 3;
            case Resource.Wood:
                return 4;
            case Resource.Stone:
                return 5;
            case Resource.Food:
                return 6;
            case Resource.Population:
                return 7;
            default:
                break;
        }
        return 0;
    }
}
