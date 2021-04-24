using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingPlacementHandler : MonoBehaviour
{
    // Placement variables
    private float mouseWheelRotation;
    public bool placingBuilding = false;
    public bool readyToPlaceBuilding = false;

    public int buildingToPlace;
    public RTSPlayer player;
    public LayerMask floorMask;
    public GameObject buildingPreviewInstance;
    public List<GameObject> selectionCircles = new List<GameObject>();
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(!placingBuilding) { return; }
        
        if(Mouse.current.leftButton.wasPressedThisFrame) 
        {
            if(!readyToPlaceBuilding) 
            { 
                readyToPlaceBuilding = true;
                return; 
            }

            if(buildingPreviewInstance == null) { return; }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                player.CmdTryPlaceBuilding(buildingToPlace, hit.point, buildingPreviewInstance.transform.rotation);
            } 
        }

        if(Input.GetKeyDown(KeyCode.Escape) || Mouse.current.rightButton.wasPressedThisFrame)
        {
            Destroy(buildingPreviewInstance);

            placingBuilding = false;
            readyToPlaceBuilding = false;
        }
        
        RotateFromMouseWheel();
    }
    
    private void RotateFromMouseWheel() {
        mouseWheelRotation = Input.mouseScrollDelta.y;
        buildingPreviewInstance.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }
}
