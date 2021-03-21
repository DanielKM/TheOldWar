using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScaleX = 250f;
    [SerializeField] private float mapScaleY = 300f;
    [SerializeField] private float offsetY = -40f;
    [SerializeField] private float offsetX = -40f;

    private Transform playerCameraTransform;
    
    // SYSTEM VARIABLES
    public bool testing;

    private void Update()
    {
        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }
                
    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, 
            mousePos,
            null,
            out Vector2 localPoint
        )) { return; }

        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScaleX, mapScaleX, lerp.x),
            playerCameraTransform.position.y, // 0
            Mathf.Lerp(-mapScaleY, mapScaleY, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(offsetX, 0, offsetY);
    }
}
