using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SpellList spellList;
    [SerializeField] private Spell spell;
    private Image iconImage = null;
    private SpellHandler spellHandler = null;
    private UnitSelectionHandler unitSelection = null;
    GameObject EventHandler;
    EventCycle EventCycle;
    private Camera mainCamera;
    private RTSPlayer player;
    private UIController UI = null;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        
        mainCamera = Camera.main;

        EventHandler = GameObject.Find("EventHandler");

        EventCycle = EventHandler.GetComponent<EventCycle>();
        // iconImage.sprite = spell.GetIcon();

        // buildingCollider = building.GetComponent<BoxCollider>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI.pointerExitedButton = false;

        // UI.OpenUnitCostPanel(building.GetPrice(), building.GetDescription(), player);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI.pointerExitedButton = true;

        UI.CloseUnitCostPanel();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }
        spellHandler = GameObject.Find("UnitHandlers").GetComponent<SpellHandler>();

        unitSelection = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();
        
        spellHandler.spellToCast = SpellList.DomeShield;

        spellHandler.player = player;

        foreach (Unit unit in unitSelection.SelectedUnits) 
        {
            if(unit.gameObject.TryGetComponent<SpellCaster>(out SpellCaster caster)) 
            { 
                player.CmdCastSpell(unit.gameObject.transform.position);
            }
        }
    }
}
