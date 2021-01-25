﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class UIController : MonoBehaviour
{
    // PLAYER
    public GameObject player;
    public GameObject team;
    public GameObject saveMenu;
    public GameObject loadMenu;
    // ResourceManager RM;
    // InputManager IM;
    // BuildingButtonController BBC;

    // GAME MENU
    public CanvasGroup GameMenuPanel;

    public int panelOpen = 0;
    // 0 - no panel
    // 1 - unit panel
    // 2 - building panel

    // UNIT PANELS
    public CanvasGroup UnitPanel;
    public CanvasGroup WorkerPanel;
    public CanvasGroup BasicBuildingsPanel;
    public CanvasGroup AdvancedBuildingsPanel;
    public CanvasGroup ResourcesBuildingPanel;
    public CanvasGroup FootmanPanel;
    public CanvasGroup WizardPanel;

    public CanvasGroup armour1;
    public CanvasGroup armour2;
    public CanvasGroup armour3;
    public CanvasGroup armour4;
    public CanvasGroup armour5;

    // INDIVIDUAL UNIT FIELDS
    public GameObject unitIcon;

    public Slider unitHealthBar;
    public Text unitHealthDisp;

    public Slider unitEnergyBar;
    public Text unitEnergyDisplay;

    public Text unitName;
    public Text unitNameDisp;
    public Text unitTypeDisp;
    public Text unitRankDisp;
    public Text unitKillDisp;

    public Text unitWeaponDisp;
    public Text unitArmourDisp;
    public Text unitItemDisp;
    public Text unitTaskDisp;

    public int unitResourceHeld;

    // INDIVIDUAL BUILDING FIELDS
    public GameObject buildingIcon;

    public Slider buildingHealthBar;
    public Text buildingHealthDisp;

    public Slider buildingEnergyBar;
    public Text buildingEnergyDisplay;

    public Text buildingName;
    public Text buildingNameDisp;
    public Text buildingTypeDisp;
    public Text buildingRankDisp;
    public Text buildingKillDisp;

    public Text buildingWeaponDisp;
    public Text buildingArmourDisp;
    public Text buildingItemDisp;
    public Text buildingTaskDisp;

    public int buildingResourceHeld;

    // BUILDING PANELS
    public CanvasGroup BuildingPanel;
    public CanvasGroup BuildingActionPanel;
    public CanvasGroup BuildingProgressPanel;
    public CanvasGroup BlacksmithActionPanel;
    public CanvasGroup LumberYardActionPanel;
    public CanvasGroup BarracksActionPanel;
    public CanvasGroup WizardTowerActionPanel;
    public CanvasGroup TrainingProgressPanel;

    // TOOLTIPS
    public CanvasGroup unitCostPanel;
    public TMP_Text unitCostText;
    public TMP_Text unitDescriptionText;

    // NOTIFICATION PANELS
    public CanvasGroup noResourcesText;
    public CanvasGroup rotationText;
    public CanvasGroup placementText;
    
    public CanvasGroup attackMovingText;
    public CanvasGroup movingText;
    public CanvasGroup patrolText;

    public bool pointerExitedButton = true;

    // Start is called before the first frame update
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if(currentScene.name != "Main Menu") {
            FindAllPanels();
            CloseGameMenuPanel();
            CloseAllPanels();
        }
    }
    
    public void FindAllPanels() {
        GameMenuPanel = GameObject.Find("GameMenu").GetComponent<CanvasGroup>();

        saveMenu = GameObject.Find("SaveMenu");
        loadMenu = GameObject.Find("loadMenu");
        UnitPanel = GameObject.Find("UnitPanel").GetComponent<CanvasGroup>();
        WorkerPanel = GameObject.Find("WorkerPanel").GetComponent<CanvasGroup>();
        BasicBuildingsPanel = GameObject.Find("BasicBuildingsPanel").GetComponent<CanvasGroup>();
        AdvancedBuildingsPanel = GameObject.Find("AdvancedBuildingsPanel").GetComponent<CanvasGroup>();
        ResourcesBuildingPanel = GameObject.Find("ResourcesBuildingsPanel").GetComponent<CanvasGroup>();
        FootmanPanel = GameObject.Find("FootmanPanel").GetComponent<CanvasGroup>();
        WizardPanel = GameObject.Find("WizardPanel").GetComponent<CanvasGroup>();
                
        BuildingPanel = GameObject.Find("BuildingPanel").GetComponent<CanvasGroup>();
        BlacksmithActionPanel = GameObject.Find("BlacksmithActionPanel").GetComponent<CanvasGroup>();
        LumberYardActionPanel = GameObject.Find("LumberYardActionPanel").GetComponent<CanvasGroup>();
        BarracksActionPanel = GameObject.Find("BarracksActionPanel").GetComponent<CanvasGroup>();
        WizardTowerActionPanel = GameObject.Find("WizardTowerActionPanel").GetComponent<CanvasGroup>();

        TrainingProgressPanel = GameObject.Find("TrainingProgressPanel").GetComponent<CanvasGroup>();
        BuildingProgressPanel = GameObject.Find("BuildingProgressPanel").GetComponent<CanvasGroup>();
        BuildingActionPanel = GameObject.Find("BuildingActions").GetComponent<CanvasGroup>();

        armour1 = GameObject.Find("Armour1").GetComponent<CanvasGroup>();
        armour2 = GameObject.Find("Armour2").GetComponent<CanvasGroup>();
        armour3 = GameObject.Find("Armour3").GetComponent<CanvasGroup>();
        armour4 = GameObject.Find("Armour4").GetComponent<CanvasGroup>();
        armour5 = GameObject.Find("Armour5").GetComponent<CanvasGroup>();

        noResourcesText = GameObject.Find("No Resources Panel").GetComponent<CanvasGroup>();
        rotationText = GameObject.Find("Rotation Text").GetComponent<CanvasGroup>();
        placementText = GameObject.Find("Placement Text").GetComponent<CanvasGroup>();

        attackMovingText = GameObject.Find("Attack Move Text").GetComponent<CanvasGroup>();
        movingText = GameObject.Find("Normal Move Text").GetComponent<CanvasGroup>();
        patrolText = GameObject.Find("Patrol Move Text").GetComponent<CanvasGroup>();

        // patrolText = GameObject.Find("Patrol Move Text").GetComponent<CanvasGroup>();
        // team = GameObject.Find("Faction");
        // player = GameObject.Find("Game").GetComponent<SaveLoad>().loadedPlayer;
        // RM = team.GetComponent<ResourceManager>();
        // IM = player.GetComponent<InputManager>();
    }

    public void BuildingSelect(UnitType buildingType)
    {
        switch (buildingType)
        {
            case UnitType.TownHall:
                TownHallSelect();
                break;
            case UnitType.Barracks:
                BarracksSelect();
                break;
            case UnitType.GoldMine:
                HouseSelect();
                break;
            case UnitType.Market:
                HouseSelect();
                break;
            case UnitType.House:
                HouseSelect();
                break;
            case UnitType.WizardTower:
                WizardTowerSelect();
                break;
            default:
                HouseSelect();
                break;
        }
    }

    public void ShowNotEnoughResources(Resource resource)
    {
        string resourcetext = "";
        if(resource == Resource.Population)
        {
            resourcetext = "Max Population Reached";
        } else {
            resourcetext = "Not Enough " + resource;
        }

        StartCoroutine(ShowNoResourcesPanel(resourcetext));
    }

    public void UnitSelect(UnitType unitType)
    {
        switch (unitType)
        {
            case UnitType.Worker:
                WorkerSelect();
                break;
            case UnitType.Swordsman:
                FighterSelect();
                break;
            case UnitType.Footman:
                FighterSelect();
                break;
            case UnitType.Archer:
                FighterSelect();
                break;
            case UnitType.Outrider:
                FighterSelect();
                break;
            case UnitType.Knight:
                FighterSelect();
                break;
            case UnitType.Wizard:
                WizardSelect();
                break;
            default:
                FighterSelect();
                break;
        }
    }

    public void DisplaySelectedObjects(GameObject selectedUnit)
    {
        // if(!IM.selectedObjects.Contains(selectedUnit)) {
        //     UnitController unitScript = selectedUnit.GetComponent<UnitController>();
        //     GameObject[] allSelectedUnitIcons = GameObject.FindGameObjectsWithTag("SelectedUnitIcon");

        //     bool unitTypePresentInArray = false;
        //     GameObject selectedIcon = null;
        //     for(int i = 0; i<allSelectedUnitIcons.Length; i++) {
        //         if(allSelectedUnitIcons[i].name == unitScript.unitType) {
        //             unitTypePresentInArray = true;
        //             selectedIcon = allSelectedUnitIcons[i];
        //         }
        //     }

        //     int unitCount = 1;
        //     for(int i = 0; i<IM.selectedObjects.Count; i++) {
        //         if(IM.selectedObjects[i].GetComponent<UnitController>().unitType == unitScript.unitType) {
        //             unitCount += 1;
        //         }
        //     }

        //     if(unitTypePresentInArray) {
        //         selectedIcon.GetComponentInChildren<Text>().text = unitCount + "";
        //     } else {
        //         GameObject newUnit = new GameObject();
        //         GameObject text = new GameObject();
        //         newUnit.name = unitScript.unitType;
        //         newUnit.tag = "SelectedUnitIcon";
        //         newUnit.AddComponent<Image>();
        //         newUnit.GetComponent<Image>().sprite = unitScript.unitIcon;
        //         newUnit.AddComponent<Outline>();         
        //         newUnit.GetComponent<Outline>().effectColor = new Color(0, 0, 255);
        //         newUnit.GetComponent<Outline>().effectDistance = new Vector2(5, 5);

        //         newUnit.transform.SetParent(UnitPanel.transform); 
        //         newUnit.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        //         text.AddComponent<Text>();
        //         text.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        //         text.GetComponent<Text>().text = "1";
        //         text.transform.SetParent(newUnit.transform);
        //         text.transform.Translate(50, 0, Time.deltaTime);
        //         if(unitScript.unitType == "Worker") {
        //             newUnit.transform.position = new Vector3(620, 270, 50);
        //         } else if (unitScript.unitType == "Swordsman") {
        //             newUnit.transform.position = new Vector3(670, 270, 50);
        //         } else if (unitScript.unitType == "Archer") {
        //             newUnit.transform.position = new Vector3(720, 270, 50);
        //         } else if (unitScript.unitType == "Footman") {
        //             newUnit.transform.position = new Vector3(770, 270, 50);
        //         } else if (unitScript.unitType == "Outrider") {
        //             newUnit.transform.position = new Vector3(820, 270, 50);
        //         } else if (unitScript.unitType == "Knight") {
        //             newUnit.transform.position = new Vector3(870, 270, 50);
        //         } else if (unitScript.unitType == "Wizard") {
        //             newUnit.transform.position = new Vector3(920, 270, 50);
        //         }
        //     }
        // }
    }

    public void RemoveSelectedObjects(GameObject selectedUnit) {
        // GameObject selectedIcon = null;
        // GameObject[] allSelectedUnitIcons = GameObject.FindGameObjectsWithTag("SelectedUnitIcon");
        // string unitType = selectedUnit.GetComponent<UnitController>().unitType;
        // int unitCount = 0;
        // for(int i = 0; i<IM.selectedObjects.Count; i++) {
        //     if(IM.selectedObjects[i].GetComponent<UnitController>().unitType == selectedUnit.GetComponent<UnitController>().unitType) {
        //         unitCount += 1;
        //     }
        // }

        // for(int i=allSelectedUnitIcons.Length - 1; i>0; i--) {
        //     if(unitType == allSelectedUnitIcons[i].name) {
        //         selectedIcon = allSelectedUnitIcons[i];
        //         selectedIcon.GetComponentInChildren<Text>().text = (unitCount-1) + "";
        //         if(unitCount-1 == 0) {
        //             Destroy(selectedIcon);
        //         }
        //         break;
        //     }
        // }
    }

    public void RemoveAllSelectedObjects(GameObject selectedUnit) {
        // GameObject[] allSelectedUnitIcons = GameObject.FindGameObjectsWithTag("SelectedUnitIcon");
        // string unitType = selectedUnit.GetComponent<UnitController>().unitType;
        // for(int i=allSelectedUnitIcons.Length - 1; i>0; i--) {
        //     if(unitType == allSelectedUnitIcons[i].name) {
        //         Destroy(allSelectedUnitIcons[i]);
        //         break;
        //     }
        // }
    }

    public void ResetSelectionIcons() {
        GameObject[] allSelectedUnitIcons = GameObject.FindGameObjectsWithTag("SelectedUnitIcon");
        foreach(GameObject go in allSelectedUnitIcons)
        {
            Destroy(go);
        }
    }

    public void OpenGameMenuPanel()
    {
        GameMenuPanel.alpha = 1;
        GameMenuPanel.blocksRaycasts = true;
        GameMenuPanel.interactable = true;
    }

    public void CloseGameMenuPanel()
    {
        GameMenuPanel.alpha = 0;
        GameMenuPanel.blocksRaycasts = false;
        GameMenuPanel.interactable = false;
    }

    public void CloseAllPanels() {
        // UNITS
        UnitPanel.alpha = 0;
        UnitPanel.blocksRaycasts = false;
        UnitPanel.interactable = false;
        WorkerPanel.alpha = 0;
        WorkerPanel.blocksRaycasts = false;
        WorkerPanel.interactable = false;

        // Worker PANELS
        BasicBuildingsPanel.alpha = 0;
        BasicBuildingsPanel.blocksRaycasts = false;
        BasicBuildingsPanel.interactable = false;
        AdvancedBuildingsPanel.alpha = 0;
        AdvancedBuildingsPanel.blocksRaycasts = false;
        AdvancedBuildingsPanel.interactable = false;
        ResourcesBuildingPanel.alpha = 0;
        ResourcesBuildingPanel.blocksRaycasts = false;
        ResourcesBuildingPanel.interactable = false;

        // FOOTMAN PANELS
        FootmanPanel.alpha = 0;
        FootmanPanel.blocksRaycasts = false;
        FootmanPanel.interactable = false;

        // WIZARD PANELS
        WizardPanel.alpha = 0;
        WizardPanel.blocksRaycasts = false;
        WizardPanel.interactable = false;

        // BUILDINGS
        BuildingPanel.alpha = 0;
        BuildingPanel.blocksRaycasts = false;
        BuildingPanel.interactable = false;
        BuildingActionPanel.alpha = 0;
        BuildingActionPanel.blocksRaycasts = false;
        BuildingActionPanel.interactable = false;
        BuildingProgressPanel.alpha = 0;
        BuildingProgressPanel.blocksRaycasts = false;
        BuildingProgressPanel.interactable = false;

        // BLACKSMITH
        BlacksmithActionPanel.alpha = 0;
        BlacksmithActionPanel.blocksRaycasts = false;
        BlacksmithActionPanel.interactable = false;

        // LUMBER YARD
        LumberYardActionPanel.alpha = 0;
        LumberYardActionPanel.blocksRaycasts = false;
        LumberYardActionPanel.interactable = false;

        // BARRACKS
        BarracksActionPanel.alpha = 0;
        BarracksActionPanel.blocksRaycasts = false;
        BarracksActionPanel.interactable = false;

        // Wizard tower
        WizardTowerActionPanel.alpha = 0;
        WizardTowerActionPanel.blocksRaycasts = false;
        WizardTowerActionPanel.interactable = false;

        // TRAINING
        TrainingProgressPanel.alpha = 0;
        TrainingProgressPanel.blocksRaycasts = false;
        TrainingProgressPanel.interactable = false;

        // NOTIFICATIONS/TOOLTIPS
        CloseUnitCostPanel();
        CloseNoResourcesText();
        NoModeText();

        panelOpen = 0;        
    }


    // TOOLTIPS TEXT

    public void CloseUnitCostPanel()
    {
        unitCostPanel.alpha = 0;
        unitCostPanel.blocksRaycasts = false;
        unitCostPanel.interactable = false;
    }

    public void OpenUnitCostPanel(Dictionary<Resource, int> price, string description, RTSPlayer player)
    { 
        CloseUnitCostPanel();
        
        Dictionary<Resource, int> playerResources = player.GetResources();
        Dictionary<Resource, int> playerMaxResources = player.GetMaxResources();
        
        string descriptionText = "";
        string red = "<color=red>";
        string green = "<color=green>";
        string color = green;
        
        for(int i = 0; i<price.Count; i++)
        {
            if(price.ElementAt(i).Key != Resource.Population) 
            { 
                if(price.ElementAt(i).Value > 0) 
                { 
                    if(playerResources[price.ElementAt(i).Key] < price.ElementAt(i).Value) { 
                        color = red;
                    } else {
                        color = green;
                    }
                    descriptionText += price.ElementAt(i).Key + " - " + color + price.ElementAt(i).Value * -1 + "</color>" + "\n";
                }
            } else {
                if(price.ElementAt(i).Value < 0) 
                { 
                    if(playerResources[price.ElementAt(i).Key] - playerMaxResources[price.ElementAt(i).Key] == 0) { 
                        color = red;
                    } else {
                        color = green;
                    }
                    descriptionText += price.ElementAt(i).Key + " - " + color + price.ElementAt(i).Value + "</color>" + "\n";
                }
            }
        }
        
        unitCostText.text = descriptionText;
        unitDescriptionText.text = description;
        unitCostPanel.alpha = 1;
        unitCostPanel.blocksRaycasts = true;
        unitCostPanel.interactable = true;

        StartCoroutine(UpdateUnitCostPanel(price, description, player));
    }
    // DIFFERENT STATES

    IEnumerator UpdateUnitCostPanel(Dictionary<Resource, int> price, string description, RTSPlayer player)
    {
        if(pointerExitedButton) { yield break; };
        
        yield return new WaitForSecondsRealtime(0.001f);

        if(pointerExitedButton) { yield break; };
        
        OpenUnitCostPanel(price, description, player);
        yield break;
    }

    // NOTIFICATIONS TEXT
    public void CloseNoResourcesText() {
        noResourcesText.alpha = 0;
        noResourcesText.blocksRaycasts = false;
        noResourcesText.interactable = false;
    }

    public void OpenNoResourcesText(string text) {
        noResourcesText.alpha = 1;
        noResourcesText.blocksRaycasts = true;
        noResourcesText.interactable = true;
        noResourcesText.GetComponentInChildren<Text>().text = text;
    }

    public void RotationModeText() {
        rotationText.alpha = 1;
        rotationText.blocksRaycasts = true;
        rotationText.interactable = true;
    }

    public void PlacementModeText() {
        placementText.alpha = 1;
        placementText.blocksRaycasts = true;
        placementText.interactable = true;
    }

    public void AttackMovementText() {
        attackMovingText.alpha = 1;
        attackMovingText.blocksRaycasts = true;
        attackMovingText.interactable = true;
    }

    public void StandardMovementText() {
        movingText.alpha = 1;
        movingText.blocksRaycasts = true;
        movingText.interactable = true;
    }

    public void PatrolMovementText() {
        patrolText.alpha = 1;
        patrolText.blocksRaycasts = true;
        patrolText.interactable = true;
    }

    public void NoModeText() {
        rotationText.alpha = 0;
        rotationText.blocksRaycasts = false;
        rotationText.interactable = false;

        placementText.alpha = 0;
        placementText.blocksRaycasts = false;
        placementText.interactable = false;

        attackMovingText.alpha = 0;
        attackMovingText.blocksRaycasts = false;
        attackMovingText.interactable = false;

        movingText.alpha = 0;
        movingText.blocksRaycasts = false;
        movingText.interactable = false;

        patrolText.alpha = 0;
        patrolText.blocksRaycasts = false;
        patrolText.interactable = false;
    }


    // On worker selection
    public void WorkerSelect() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        WorkerPanel.alpha = 1;
        WorkerPanel.blocksRaycasts = true;
        WorkerPanel.interactable = true;
    }

    // On Worker clicking basic buildings
    public void WorkerBasicBuildings() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        BasicBuildingsPanel.alpha = 1;
        BasicBuildingsPanel.blocksRaycasts = true;
        BasicBuildingsPanel.interactable = true;
    }

    // On Worker clicking advanced buildings
    public void WorkerAdvancedBuildings() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        AdvancedBuildingsPanel.alpha = 1;
        AdvancedBuildingsPanel.blocksRaycasts = true;
        AdvancedBuildingsPanel.interactable = true;
    }

    // On Worker clicking advanced buildings
    public void WorkerResourcesBuildings() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        ResourcesBuildingPanel.alpha = 1;
        ResourcesBuildingPanel.blocksRaycasts = true;
        ResourcesBuildingPanel.interactable = true;
    }

    public void FighterSelect() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        FootmanPanel.alpha = 1;
        FootmanPanel.blocksRaycasts = true;
        FootmanPanel.interactable = true;
    }

    public void WizardSelect() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
        WizardPanel.alpha = 1;
        WizardPanel.blocksRaycasts = true;
        WizardPanel.interactable = true;
    }

    // Enemy selection
    public void EnemySelect() {
        CloseAllPanels();

        UnitPanel.alpha = 1;
        UnitPanel.blocksRaycasts = true;
        UnitPanel.interactable = true;

        panelOpen = 1;        
    }

    // On house selection
    public void HouseSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        panelOpen = 2;
    }

    // On town hall selection
    public void TownHallSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        BuildingActionPanel.alpha = 1;
        BuildingActionPanel.blocksRaycasts = true;
        BuildingActionPanel.interactable = true;
        CloseTrainingProgressPanel();
        panelOpen = 2;
    }

    // On town hall selection if it is training
    public void TownHallTraining() {
        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        BuildingActionPanel.alpha = 1;
        BuildingActionPanel.blocksRaycasts = true;
        BuildingActionPanel.interactable = true;

        OpenTrainingProgressPanel();
        panelOpen = 2;
    }

    // On town hall selection
    public void BlacksmithSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        BlacksmithActionPanel.alpha = 1;
        BlacksmithActionPanel.blocksRaycasts = true;
        BlacksmithActionPanel.interactable = true;

        panelOpen = 2;
    }

    public void BlacksmithTraining() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        OpenBuildingProgressPanel();
        panelOpen = 2;
    }

    // On town hall selection
    public void LumberYardSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        LumberYardActionPanel.alpha = 1;
        LumberYardActionPanel.blocksRaycasts = true;
        LumberYardActionPanel.interactable = true;
        panelOpen = 2;
    }

    // On town hall selection
    public void BarracksSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        BarracksActionPanel.alpha = 1;
        BarracksActionPanel.blocksRaycasts = true;
        BarracksActionPanel.interactable = true;
        CloseTrainingProgressPanel();
        panelOpen = 2;
    }

    public void BarracksTraining() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        BarracksActionPanel.alpha = 1;
        BarracksActionPanel.blocksRaycasts = true;
        BarracksActionPanel.interactable = true;
        OpenTrainingProgressPanel();
        panelOpen = 2;
    }

    // On town hall selection
    public void WizardTowerSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        WizardTowerActionPanel.alpha = 1;
        WizardTowerActionPanel.blocksRaycasts = true;
        WizardTowerActionPanel.interactable = true;
        CloseTrainingProgressPanel();
        panelOpen = 2;
    }

    // On town hall selection
    public void StablesSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        panelOpen = 2;
    }

    // On resource node selection (including farms)
    public void ResourceSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        panelOpen = 2;
    }

   // On resource node selection (including farms)
    public void FoundationSelect() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;

        panelOpen = 2;
    }
    
   // On resource node selection (including farms)
    public void FoundationBuilding() {
        CloseAllPanels();

        BuildingPanel.alpha = 1;
        BuildingPanel.blocksRaycasts = true;
        BuildingPanel.interactable = true;
        
        OpenBuildingProgressPanel();
        panelOpen = 2;
    }

    public void OpenBuildingProgressPanel(){
        BuildingProgressPanel.alpha = 1;
        BuildingProgressPanel.blocksRaycasts = true;
        BuildingProgressPanel.interactable = true;
    }

    public void OpenTrainingProgressPanel(){
        TrainingProgressPanel.alpha = 1;
        TrainingProgressPanel.blocksRaycasts = true;
        TrainingProgressPanel.interactable = true;
    }

    public void CloseTrainingProgressPanel(){
        TrainingProgressPanel.alpha = 0;
        TrainingProgressPanel.blocksRaycasts = false;
        TrainingProgressPanel.interactable = false;
    }

    IEnumerator ShowNoResourcesPanel(string text)
    {
        OpenNoResourcesText(text);
        yield return new WaitForSecondsRealtime(3);
        CloseNoResourcesText();
    }
}

