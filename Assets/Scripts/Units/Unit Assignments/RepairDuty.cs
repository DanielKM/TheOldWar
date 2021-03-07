using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RepairDuty : MonoBehaviour, IPointerDownHandler
{
    UnitSelectionHandler unitSelectionHandler;

    public void OnPointerDown(PointerEventData eventData)
    {
        unitSelectionHandler = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();
        
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) 
        {
            if(unit.gameObject.TryGetComponent<ResourceGatherer>(out ResourceGatherer gatherer)) 
            {
                unit.gameObject.GetComponent<UnitTask>().SetTask(ActionList.RepairDuty);    
                
                unit.GetTargeter().CmdSetRepairTarget();
            }
        }
    }
}
