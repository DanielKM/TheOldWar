using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public Button buttonOne;

    // Worker buttons
    public Button workerBasicBuildingButton;
    public Button workerAdvancedBuildingButton;
    public Button workerGatherButton;
    public Button workerClearDeadButton;

    public Button workerBasicBackButton;
    public Button workerAdvancedBackButton;  
    public Button workerGatherBackButton;

    // Barracks buttons
    public Button barracksButtonOne;
    public Button barracksButtonTwo;
    public Button barracksButtonThree;
    public Button barracksButtonFour;
    public Button barracksButtonFive;
    public Button barracksTrainWizard;

    public Button wizardTowerButton1;

    public Building building;
    public Unit unit;
    UIController UI = null;

    // Start is called before the first frame update
    void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();

        UpdateSelectedBuilding();

        workerBasicBuildingButton.onClick.AddListener(delegate{UI.WorkerBasicBuildings();});
        workerAdvancedBuildingButton.onClick.AddListener(delegate{UI.WorkerAdvancedBuildings();});
        workerGatherButton.onClick.AddListener(delegate{UI.WorkerGatherPanelOpen();});

        workerBasicBackButton.onClick.AddListener(delegate{UI.WorkerSelect();});
        workerAdvancedBackButton.onClick.AddListener(delegate{UI.WorkerSelect();});
        workerGatherBackButton.onClick.AddListener(delegate{UI.WorkerSelect();});
    }

    public void UpdateSelectedBuilding()
    {
        buttonOne.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(1);});
        barracksButtonOne.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(2);});
        barracksButtonTwo.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(3);});
        barracksButtonThree.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(4);});
        barracksButtonFour.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(5);});
        barracksButtonFive.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(6);});
        barracksTrainWizard.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(7);});

        wizardTowerButton1.onClick.AddListener(delegate{building.GetComponent<UnitSpawner>().CmdSpawnUnit(7);});
    }
}
