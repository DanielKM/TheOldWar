using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSelectionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private UIController UI = null;
    public RTSPlayer player;
    
    [Header("Settings")]
    private Vector2 startPosition;
    private Vector2 endPosition;
    private Camera mainCamera;

    public List<Unit> SelectedUnits { get; set; } = new List<Unit>();
    public BuildingPlacementHandler BuildingPlacementHandler = null;

    // Control groups
    public List<Unit> controlGroup1;
    public List<Unit> controlGroup2;
    public List<Unit> controlGroup3;
    public List<Unit> controlGroup4;
    public List<Unit> controlGroup5;
    public List<Unit> controlGroup6;
    public List<Unit> controlGroup7;
    public List<Unit> controlGroup8;
    public List<Unit> controlGroup9;
    public List<Unit> controlGroup0;

    public List<Renderer> visibleRenderers = new List<Renderer>();

    // SYSTEM VARIABLES
    public bool testing;
    // Last Click Time
    private float lastclickTime;
    private const float DOUBLE_CLICK_TIME = .2f;

    private void Start()
    {
        testing = GameObject.Find("Testing").GetComponent<Testing>().testing;
        UI = GameObject.Find("UI").GetComponent<UIController>();
        BuildingPlacementHandler = gameObject.GetComponent<BuildingPlacementHandler>();

        if(!testing) 
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
        mainCamera = Camera.main;

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update() {

        if(testing)
        {
            if(player == null)
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }
        }

        PointerEventData mouse1 = EventSystem.current.gameObject.GetComponent<CustomStandaloneInputModule>().GetLastPointerEventDataPublic(-1);


        if(mouse1 != null)
        {
            if(mouse1.pointerPress) 
            {
                if (mouse1.pointerPress.CompareTag( "UI" )) { return; }
            }
        }

        if(Mouse.current.leftButton.wasPressedThisFrame) 
        {        
            StartSelectionArea();
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame) 
        {
            ClearSelectionArea();
        } 
        else if(Mouse.current.leftButton.isPressed) 
        {
            UpdateSelectionArea();
        }
        if(Input.GetKeyDown(KeyCode.Q)) 
        {
            SelectAllVisibleUnits();
            
            // gameObject.GetComponent<GameobjectLists>().AllUnitsAttack();
        }

        ActivateControlGroups();
    }
        
    private void SelectVisibleUnitsOfSameType(Unit unit)
    {
        Renderer[] sceneRenderers = FindObjectsOfType<Renderer>();
        visibleRenderers.Clear();
        // ADD OWNERS/TEAMS
        for(int i = 0; i < sceneRenderers.Length; i++) {
            if(IsVisible(sceneRenderers[i])) {
                Transform parent = sceneRenderers[i].transform.parent;
                if (parent) {
                    GameObject parentGameObject = parent.gameObject;
                    if (parentGameObject) {
                        if(parentGameObject.GetComponent<UnitInformation>()) {
                            if(parentGameObject.GetComponent<UnitInformation>().owner == player) {
                                visibleRenderers.Add(sceneRenderers[i]);
                            }
                        }
                    }
                }
            }
        }

        foreach( Renderer renderer in visibleRenderers) {
            GameObject doubleClickSelection = renderer.transform.parent.gameObject;
            if (doubleClickSelection.GetComponent<Unit>()) {
                if(unit.gameObject.GetComponent<UnitInformation>().unitType == doubleClickSelection.GetComponent<UnitInformation>().unitType) {
                    AddSelectedUnit(doubleClickSelection.GetComponent<Unit>());
                }
            }
        }
    }

    void SelectAllVisibleUnits() {
        Renderer[] sceneRenderers = FindObjectsOfType<Renderer>();
        visibleRenderers.Clear();
        
        // ADD OWNERS/TEAMS
        for(int i = 0; i < sceneRenderers.Length; i++) {
            if(IsVisible(sceneRenderers[i])) {
                if (sceneRenderers[i].transform.parent) {
                    if (sceneRenderers[i].transform.parent.gameObject) {
                        if(sceneRenderers[i].transform.parent.gameObject.GetComponent<UnitInformation>()) {
                            if(sceneRenderers[i].transform.parent.gameObject.GetComponent<UnitInformation>().owner == player) {
                                visibleRenderers.Add(sceneRenderers[i]);
                            }
                        }
                    }
                }
            }
        }

        foreach( Renderer renderer in visibleRenderers) {
            GameObject doubleClickSelection = renderer.transform.parent.gameObject;
            if (doubleClickSelection.GetComponent<Unit>()) {
                AddSelectedUnit(doubleClickSelection.GetComponent<Unit>());
            }
        }
    }

    bool IsVisible(Renderer renderer) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return (GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) ? true : false;
    }

    private void StartSelectionArea() 
    {    
        // DESELECT BUILDINGS
        foreach(GameObject circle in BuildingPlacementHandler.selectionCircles)
        {
            if(circle)
            {
                GameObject parentBuildingGameObject = circle.transform.parent.gameObject;
                if(parentBuildingGameObject) {
                    if(parentBuildingGameObject.TryGetComponent<UnitSpawner>(out UnitSpawner spawner))
                    {
                        spawner.rallyPointGameObject.SetActive(false);
                    }
                    circle.transform.parent.GetComponent<UnitInformation>().selected = false;
                    circle.SetActive(false);
                }
            }
        }

        if(!Keyboard.current.leftShiftKey.isPressed) 
        {
            foreach(Unit selectedUnit in SelectedUnits) 
            {
                selectedUnit.Deselect();
            }

            UI.CloseAllPanels();
            
            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + 
        new Vector2(areaWidth/2, areaHeight/2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if(unitSelectionArea.sizeDelta.magnitude == 0) 
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }    

            if(!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            if(!unit.hasAuthority) { return; }

            AddSelectedUnit(unit);

            Unit firstUnit = SelectedUnits[0].gameObject.GetComponent<Unit>();
            
            firstUnit.unitAudio.clip = firstUnit.unitSelectedClip;

            firstUnit.unitAudio.Play();
            
            float timeSinceLastClick = Time.time - lastclickTime;

            if(timeSinceLastClick <= DOUBLE_CLICK_TIME) 
            {
                Debug.Log("Double!");
                SelectVisibleUnitsOfSameType(unit);
            }
            lastclickTime = Time.time;

            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta/2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta/2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if(SelectedUnits.Contains(unit)) { continue; }
            
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x && 
                screenPosition.x < max.x && 
                screenPosition.y > min.y && 
                screenPosition.y < max.y) 
            {
                AddSelectedUnit(unit);
            }
        }

        if(SelectedUnits.Count < 1) { return; }

        Unit singleUnit =  SelectedUnits[0].gameObject.GetComponent<Unit>();
        
        singleUnit.unitAudio.clip = singleUnit.unitSelectedClip;

        singleUnit.unitAudio.Play();
    }

    private void AuthorityHandleUnitDespawn(Unit unit) 
    {
        RemoveSelectedUnit(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

    private void AddSelectedUnit (Unit unit)
    { 
        UI.UnitSelect(unit.gameObject.GetComponent<UnitInformation>().unitType);

        UpdateUnitPanel(unit);

        SelectedUnits.Add(unit);

        unit.Select();
    }

    private void RemoveSelectedUnit (Unit unit)
    {
        // UI.CloseAllPanels();
        SelectedUnits.Remove(unit);
        
        unit.Deselect();
    }

    void ActivateControlGroups() {
        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            if(Input.GetKeyDown(KeyCode.Alpha1)) {
                SaveControlGroup(1);
            } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
                SaveControlGroup(2);
            } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
                SaveControlGroup(3);
            } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
                SaveControlGroup(4);
            } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
                SaveControlGroup(5);
            } else if(Input.GetKeyDown(KeyCode.Alpha6)) {
                SaveControlGroup(6);
            } else if(Input.GetKeyDown(KeyCode.Alpha7)) {
                SaveControlGroup(7);
            } else if(Input.GetKeyDown(KeyCode.Alpha8)) {
                SaveControlGroup(8);
            } else if(Input.GetKeyDown(KeyCode.Alpha9)) {
                SaveControlGroup(9);
            }
        } else {
            if(Input.GetKeyDown(KeyCode.Alpha1) ) {
                LoadControlGroup(1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                LoadControlGroup(2);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                LoadControlGroup(3);
            } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                LoadControlGroup(4);
            } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                LoadControlGroup(5);
            } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
                LoadControlGroup(6);
            } else if (Input.GetKeyDown(KeyCode.Alpha7)) {
                LoadControlGroup(7);
            } else if (Input.GetKeyDown(KeyCode.Alpha8)) {
                LoadControlGroup(8);
            } else if (Input.GetKeyDown(KeyCode.Alpha9)) {
                LoadControlGroup(9);
            } else if (Input.GetKeyDown(KeyCode.Alpha0)) {
                LoadControlGroup(0);
            }
        }
    }

    void SaveControlGroup(int number) {
        List<Unit> currentList = new List<Unit>();
        if(number == 1) {
            currentList = controlGroup1;
        } else if (number == 2) {
            currentList = controlGroup2;
        } else if (number == 3) {
            currentList = controlGroup3;
        } else if (number == 4) {
            currentList = controlGroup4;
        } else if (number == 5) {
            currentList = controlGroup5;
        } else if (number == 6) {
            currentList = controlGroup6;
        } else if (number == 7) {
            currentList = controlGroup7;
        } else if (number == 8) {
            currentList = controlGroup8;
        } else if (number == 9) {
            currentList = controlGroup9;
        } else if (number == 0) {
            currentList = controlGroup0;
        }
        currentList.Clear();
        foreach (Unit unit in SelectedUnits) {
            currentList.Add(unit);
        }
    }

    void LoadControlGroup(int number) {
        if(!Input.GetKey(KeyCode.LeftShift)) {
            foreach(Unit unit in SelectedUnits) 
            {
                RemoveSelectedUnit(unit);
            }
        }
        List<Unit> currentList = new List<Unit>();
        if(number == 1) {
            currentList = controlGroup1;
        } else if (number == 2) {
            currentList = controlGroup2;
        } else if (number == 3) {
            currentList = controlGroup3;
        } else if (number == 4) {
            currentList = controlGroup4;
        } else if (number == 5) {
            currentList = controlGroup5;
        } else if (number == 6) {
            currentList = controlGroup6;
        } else if (number == 7) {
            currentList = controlGroup7;
        } else if (number == 8) {
            currentList = controlGroup8;
        } else if (number == 9) {
            currentList = controlGroup9;
        } else if (number == 0) {
            currentList = controlGroup0;
        }
        
        foreach (Unit unit in currentList) {
            AddSelectedUnit(unit);
        }
    }

    public void UpdateUnitPanel(Unit unit)
    { 
        UI.unitIcon.GetComponent<Image>().sprite = unit.GetComponent<Unit>().unitIcon;
        // if (RC.artisanArmourSmithing) {
        //     UI.armour1.GetComponent<Image>().color = new Color32(255,165,0,255);
        //     UI.armour2.GetComponent<Image>().color = new Color32(255,165,0,255);
        //     UI.armour3.GetComponent<Image>().color = new Color32(255,165,0,255);
        //     UI.armour4.GetComponent<Image>().color = new Color32(255,165,0,255);
        //     UI.armour5.GetComponent<Image>().color = new Color32(255,165,0,255);
        // } else if (RC.basicArmourSmithing) {
        //     UI.armour1.GetComponent<Image>().color = new Color32(114,160,193,255);
        //     UI.armour2.GetComponent<Image>().color = new Color32(114,160,193,255);
        //     UI.armour3.GetComponent<Image>().color = new Color32(114,160,193,255);
        //     UI.armour4.GetComponent<Image>().color = new Color32(114,160,193,255);
        //     UI.armour5.GetComponent<Image>().color = new Color32(114,160,193,255);
        // } else {
        //     UI.armour1.GetComponent<Image>().color = new Color32(205,127,50,255);
        //     UI.armour2.GetComponent<Image>().color = new Color32(205,127,50,255);
        //     UI.armour3.GetComponent<Image>().color = new Color32(205,127,50,255);
        //     UI.armour4.GetComponent<Image>().color = new Color32(205,127,50,255);
        //     UI.armour5.GetComponent<Image>().color = new Color32(205,127,50,255);
        // }

        // if(unitScript.armour <= 0.0f) {
        //     UI.armour1.alpha = 0;
        //     UI.armour2.alpha = 0;
        //     UI.armour3.alpha = 0;
        //     UI.armour4.alpha = 0;
        //     UI.armour5.alpha = 0;
        // } else if(unitScript.armour <= 1.0f) {
        //     UI.armour1.alpha = 1;
        //     UI.armour2.alpha = 0;
        //     UI.armour3.alpha = 0;
        //     UI.armour4.alpha = 0;
        //     UI.armour5.alpha = 0;
        // } else if(unitScript.armour <= 2.0f) {
        //     UI.armour1.alpha = 1;
        //     UI.armour2.alpha = 1;
        //     UI.armour3.alpha = 0;
        //     UI.armour4.alpha = 0;
        //     UI.armour5.alpha = 0;
        // } else if(unitScript.armour <= 3.0f) {
        //     UI.armour1.alpha = 1;
        //     UI.armour2.alpha = 1;
        //     UI.armour3.alpha = 1;
        //     UI.armour4.alpha = 0;
        //     UI.armour5.alpha = 0;
        // } else if(unitScript.armour <= 4.0f) {
        //     UI.armour1.alpha = 1;
        //     UI.armour2.alpha = 1;
        //     UI.armour3.alpha = 1;
        //     UI.armour4.alpha = 1;
        //     UI.armour5.alpha = 0;
        // } else if(unit.armour <= 5.0f) {
        //     UI.armour1.alpha = 1;
        //     UI.armour2.alpha = 1;
        //     UI.armour3.alpha = 1;
        //     UI.armour4.alpha = 1;
        //     UI.armour5.alpha = 1;
        // }

        UI.unitHealthBar.maxValue = unit.gameObject.GetComponent<Health>().maxHealth;
        UI.unitHealthBar.value = unit.gameObject.GetComponent<Health>().currentHealth;

        // UI.unitEnergyBar.maxValue = unitScript.maxEnergy;
        // UI.unitEnergyBar.value = unitScript.energy;

        UI.unitHealthDisp.text = "HEALTH: " + unit.gameObject.GetComponent<Health>().currentHealth;
        // UI.unitEnergyDisplay.text = "ENERGY: " + unitScript.energy;

        UnitInformation unitInformation = unit.gameObject.GetComponent<UnitInformation>();
        if(unit.gameObject.TryGetComponent<ResourceGatherer>(out ResourceGatherer resourceGatherer)) 
        {
            Resource resourceType = resourceGatherer.heldResourcesType;
            UI.unitItemDisp.text = resourceType + ": " + resourceGatherer.heldResources;
        }
        
        UI.unitNameDisp.text = unitInformation.unitName;
        UI.unitTypeDisp.text = unitInformation.unitType.ToString();
        UI.unitRankDisp.text = unitInformation.unitRank.ToString();
        UI.unitKillDisp.text = "Kills: " + unitInformation.unitKills;

        UI.unitWeaponDisp.text = unitInformation.unitWeapon.ToString();
        UI.unitArmourDisp.text = unitInformation.unitArmour.ToString();


        UI.unitTaskDisp.text = "" + unit.gameObject.GetComponent<UnitTask>().GetTask();
    }
}
