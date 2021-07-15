using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GathererDuty : MonoBehaviour, IPointerDownHandler
{
    public Resource resource;

    UnitSelectionHandler unitSelectionHandler;

    public void OnPointerDown(PointerEventData eventData)
    {
        int resourceID = GetResourceID(resource);

        unitSelectionHandler = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();
        
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) 
        {
            if(unit.gameObject.TryGetComponent<ResourceGatherer>(out ResourceGatherer gatherer)) 
            {
                unit.gameObject.GetComponent<ResourceGatherer>().DropCurrentlyHeldResources();
                
                unit.gameObject.GetComponent<UnitTask>().SetTask(ActionList.Gathering);    
                
                // unit.GetTargeter().CmdSetClosestResourceTarget(resourceID);

                // Drop current resource

                unit.gameObject.GetComponent<ResourceGatherer>().heldResourcesType = resource;
            }
        }
    }

    private int GetResourceID(Resource selectedResource)
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
