using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;
    public float panSpeed = 0.2f;
    public float minHeight = 10f;
    public float maxHeight = 40f;

    BuildingPlacementHandler buildingPlacementHandler;

    private float mouseWheelRotation;

    private Vector2 previousInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if(!hasAuthority || !Application.isFocused) { return; }

        //cheap scene check
        if(GameObject.Find("UnitHandlers") == null) { return; }
        
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;
        float moveY = Camera.main.transform.position.y;

        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if(cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }

            if(cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if(cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }
            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else 
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        // Clamping main camera bounds
        moveY -= Input.GetAxis("Mouse ScrollWheel") * (panSpeed * 30);

        buildingPlacementHandler = GameObject.Find("UnitHandlers").GetComponent<BuildingPlacementHandler>();
        
        if(!buildingPlacementHandler.placingBuilding) { pos.y = Mathf.Clamp(moveY, minHeight, maxHeight); }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x - 20, screenZLimits.y + 30); // here
        pos.z = Mathf.Clamp(pos.z, screenXLimits.x - 20, screenZLimits.y + 30);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}
