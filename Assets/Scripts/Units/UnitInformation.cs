using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitInformation : NetworkBehaviour
{
    [Header("References")]
    public RTSPlayer owner = null;
    RTSPlayer player = null;
    Cursors cursors = null;
    
    [Header("Settings")]
    public bool selected = false;
    public int unitEnergy = 100;
    public int MaxUnitEnergy = 100;
    public string unitName = "Harold Barkeye";
    public UnitType unitType = UnitType.Worker;
    public UnitRank unitRank = UnitRank.Recruit;
    public int unitKills = 0;
    public WeaponType unitWeapon = WeaponType.Pickaxe;
    public ArmourType unitArmour = ArmourType.None;
    
    private void Start()
    {
        cursors = GameObject.Find("UnitHandlers").GetComponent<Cursors>();
        
        if(connectionToClient == null ) { return; }
        
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
    }

    private void OnMouseEnter()
    {
        if(gameObject.layer == 14) 
        { 
            Cursor.SetCursor(cursors.mine, new Vector2(0, 0), CursorMode.Auto);
        } else if(owner != player) 
        {
            Cursor.SetCursor(cursors.sword, new Vector2(0, 0), CursorMode.Auto);
        } else 
        {
            Cursor.SetCursor(cursors.pointer, new Vector2(0, 0), CursorMode.Auto);
        }
    }

    private void OnMouseExit()
    {
        // if(!owner) { return; }
        Cursor.SetCursor(cursors.basic, new Vector2(0, 0), CursorMode.Auto);
    }
}
