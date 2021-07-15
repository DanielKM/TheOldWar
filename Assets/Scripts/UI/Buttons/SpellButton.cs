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
    // private Image iconImage = null;
    private SpellHandler spellHandler = null;
    private UnitSelectionHandler unitSelection = null;
    private Camera mainCamera;
    private RTSPlayer player;
    private UIController UI = null;

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
