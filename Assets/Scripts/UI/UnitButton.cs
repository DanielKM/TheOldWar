using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Unit unit = null;

    private RTSPlayer player;
    UIController UI = null;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI.pointerExitedButton = false;

        UI.OpenUnitCostPanel(unit.GetPrice(), unit.GetDescription(), player);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI.pointerExitedButton = true;

        UI.CloseUnitCostPanel();
    }
}
