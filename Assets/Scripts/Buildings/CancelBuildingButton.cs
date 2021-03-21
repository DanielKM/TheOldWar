using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CancelBuildingButton : MonoBehaviour, IPointerDownHandler
{
    // Start is called before the first frame update
    GameObject UIGameObject = null;
    UIController UI = null;

    void Start()
    {
        UIGameObject = GameObject.Find("UI");
        UI = UIGameObject.GetComponent<UIController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Building selectedBuilding = UIGameObject.GetComponent<Buttons>().building;
        Foundation selectedFoundation = selectedBuilding.GetComponent<Foundation>();
        GameObject prefab = selectedFoundation.buildingPrefab;
        Building buildingPrefab = prefab.GetComponent<Building>();

        RTSPlayer player = selectedFoundation.gameObject.GetComponent<UnitInformation>().owner;
                
        player.gold += buildingPrefab.gold;
        player.iron += buildingPrefab.iron;
        player.steel += buildingPrefab.steel;
        player.skymetal += buildingPrefab.skymetal;
        player.wood += buildingPrefab.wood;
        player.stone += buildingPrefab.stone;
        player.food += buildingPrefab.food;

        selectedBuilding.gameObject.GetComponent<Health>().CmdServerDie();
        
        UI.CloseAllPanels();
    }
}
