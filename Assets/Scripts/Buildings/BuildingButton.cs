using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Material greenMaterial = null;
    [SerializeField] private Material redMaterial = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private MeshRenderer buildingRendererInstance;
    BuildingPlacementHandler buildingPlacementHandler;

    private 

    UIController UI = null;
    
    // SYSTEM VARIABLES
    public bool testing;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();
        testing = GameObject.Find("Testing").GetComponent<Testing>().testing;

        if(!testing) 
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
        
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();

        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update() {
        // if(testing)
        // {
        //     if(player == null)
        //     {
        //         player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //     }
        // }

        if(buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI.pointerExitedButton = false;

        UI.OpenUnitCostPanel(building.GetPrice(), building.GetDescription(), player);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI.pointerExitedButton = true;

        UI.CloseUnitCostPanel();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buildingPlacementHandler = GameObject.Find("UnitHandlers").GetComponent<BuildingPlacementHandler>();
        if(!buildingPlacementHandler.placingBuilding) 
        {
            if(eventData.button != PointerEventData.InputButton.Left) { return; }

            if(player.CannotAfford(player.GetResources(), building.GetPrice())) { return; }

            // Add pooled buildings here
            buildingPreviewInstance = Instantiate(building.GetBuildingPreview());

            buildingPreviewInstance.SetActive(false);
            
            buildingPlacementHandler.placingBuilding = true;
            buildingPlacementHandler.buildingToPlace = building.GetId();
            buildingPlacementHandler.player = player;
            buildingPlacementHandler.floorMask = floorMask;
            buildingPlacementHandler.buildingPreviewInstance = buildingPreviewInstance;
        }
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; } 

        buildingPreviewInstance.transform.position = hit.point;

        if(!buildingPreviewInstance.activeSelf) 
        {
            buildingPreviewInstance.SetActive(true);
        }
        
        Material mat = player.CanPlaceBuilding(buildingCollider, hit.point) ? greenMaterial : redMaterial;
        if(buildingPreviewInstance.transform.childCount > 0) {
            foreach (Transform child in buildingPreviewInstance.transform) 
            {
                if(child.gameObject.GetComponent<MeshRenderer>() != null)
                {
                    MeshRenderer renderers = child.gameObject.GetComponent<MeshRenderer>();

                    Material[] mats = new Material[renderers.materials.Length];
                    for(int i=0; i<renderers.materials.Length; i++)
                    {
                        mats[i] = mat;
                    }

                    renderers.materials = mats;
                }
            }
        } else {
           if(buildingPreviewInstance.GetComponent<MeshRenderer>() != null)
           {
                MeshRenderer renderers = buildingPreviewInstance.GetComponent<MeshRenderer>();

                Material[] mats = new Material[renderers.materials.Length];
                for(int i=0; i<renderers.materials.Length; i++)
                {
                    mats[i] = mat;
                }

                renderers.materials = mats;
           } 
        }
    }
}
