﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitTask : NetworkBehaviour
{
    // [SerializeField]
    // private Animator anim;
    // private NetworkAnimator networkAnimator;
    
    [Header("References")]
    [SerializeField] private UnitAnimation unitAnimation;

    private UnitSelectionHandler unitSelection = null;
    
    [Header("Settings")]
    [SyncVar(hook=nameof(HandleDisplayTaskUpdated))]
    [SerializeField]
    private ActionList task = ActionList.Idle;

    private void Start()
    {
        unitSelection = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();
    }

    public ActionList GetTask()
    {
        return task;
    }

    #region Server

    [Server]
    public void SetTask(ActionList newTask)
    {   
        Unit unit = gameObject.GetComponent<Unit>();
        
        task = newTask;        

        unitAnimation.SetAnimation(newTask);

        if(unit.GetComponent<UnitInformation>().selected == false) { return; }

        // if(unitSelection.SelectedUnits.Count <= 0) { return; }

        // if(unit != unitSelection.SelectedUnits[0]) { return; }

        unitSelection.UpdateUnitPanel(unit);
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    private void CmdSetTask(ActionList newTask)
    {
        SetTask(newTask);
    }

    #endregion

    #region Client

    [ClientRpc]
    public void SetUnitTask(ActionList newTask)
    {   
        SetTask(newTask);
    }

    public void HandleDisplayTaskUpdated(ActionList oldTask, ActionList newTask)
    {   
        task = newTask;
    }

    #endregion
}
