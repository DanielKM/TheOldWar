using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitTask : NetworkBehaviour
{
    // [SerializeField]
    // private Animator anim;
    // [SerializeField]
    // private NetworkAnimator networkAnimator;
    // private UnitAnimation unitAnimation;

    private UnitSelectionHandler unitSelection = null;

    [SyncVar(hook=nameof(HandleDisplayTaskUpdated))]
    [SerializeField]
    private ActionList task = ActionList.Idle;
    
    private void Start()
    {
        // unitAnimation = GetComponent<UnitAnimation>();
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

        if(unit.GetComponent<UnitInformation>().selected == false) { return; }

        unitSelection.UpdateUnitPanel(unit);
    }

    [Command(ignoreAuthority = true)]
    private void CmdSetTask(ActionList newTask)
    {
        SetTask(newTask);
    }

    #endregion

    #region Client

    [ClientRpc]
    public void SetUnitTask(ActionList newTask)
    {   
        // networkAnimator.animator.SetBool("isWalking", true);
        CmdSetTask(newTask);
    }

    public void HandleDisplayTaskUpdated(ActionList oldTask, ActionList newTask)
    {   
        task = newTask;
    }

    #endregion
}
