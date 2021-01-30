using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]  UnitTask unitTask = null;

    [Header("Settings")]
    [SerializeField] public int maxHealth = 100;
    private UnitSelectionHandler unitSelection = null;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    public int currentHealth = 100;

    public event Action ServerOnDie;
    public event Action ServerOnInjured;
    public event Action<int, int> ClientOnHealthUpdated;
    private Unit unit = null;
    private UnitInformation unitInformation = null;

    public void Start()
    {
        unit = gameObject.GetComponent<Unit>();

        unitSelection = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();

        unitInformation = gameObject.GetComponent<UnitInformation>();
    }

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
        ServerOnInjured += ServerHandleUnitInjured;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
        ServerOnInjured -= ServerHandleUnitInjured;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId) 
    {
        if(connectionToClient.connectionId == connectionId) { return; }
        
        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount) 
    {
        float armourModifier = CheckArmourModifiers();

        // if(currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - (int)Math.Ceiling(damageAmount/armourModifier), 0);

        if(currentHealth > 0) { return; }

        ServerOnInjured?.Invoke();
        ServerOnDie?.Invoke();
    }

    [Server]
    public void ServerHandleUnitInjured()
    {
        unitTask.SetTask(ActionList.Injured);
    }

    
    [Server]
    private float CheckArmourModifiers()
    {
        float armourModifier = 1f;

        switch (unitInformation.unitArmour)
        {
            case ArmourType.None:
                armourModifier = 1f;
                return armourModifier;
            case ArmourType.Leather:
                armourModifier = 2f;
                return armourModifier;
            case ArmourType.Mail:
                armourModifier = 3f;
                return armourModifier;
            case ArmourType.Plate:
                armourModifier = 4f;
                return armourModifier;
            case ArmourType.Wood:
                armourModifier = 5f;
                return armourModifier;
            case ArmourType.Stone:
                armourModifier = 6f;
                return armourModifier;
            default:
                break;
        }

        return armourModifier;
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);

        if(gameObject.GetComponent<UnitInformation>().selected == false) { return; }

        unitSelection.UpdateUnitPanel(unit);
    }

    #endregion
}
