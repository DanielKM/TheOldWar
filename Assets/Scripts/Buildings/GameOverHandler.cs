using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    [Server]
    private void Start()
    {
        // if(gameObject.GetComponent<UnitInformation>().unitType == UnitType.CaravanFoundation)
        // {
        //     ServerGameOver();
        // }
    }

    [Server]
    public void ServerGameOver()
    {
        StartCoroutine(ReadyCaravan());
    }

    [Server]
    IEnumerator ReadyCaravan()
    {
        // HERE DAN

        yield return new WaitForSeconds(10);

        RpcGameOver("You escaped unscathed!");

        ServerOnGameOver?.Invoke();
    }

    [Server]
    public void GameOver()
    {
        RpcGameOver("You escaped unscathed!");

        ServerOnGameOver?.Invoke();
    }

    // [Server]
    // private void ServerHandleBaseSpawned(UnitBase unitBase) 
    // {
    //     bases.Add(unitBase);
    // }

    // [Server]
    // private void ServerHandleBaseDespawned(UnitBase unitBase) 
    // {
    //     bases.Remove(unitBase);

    //     if(bases.Count !=1) { return; }

    //     int playerId = bases[0].connectionToClient.connectionId;

    //     RpcGameOver($"Player {playerId}");

    //     ServerOnGameOver?.Invoke();
    // }

    #endregion

    #region client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    // [Command(ignoreAuthority = true)]
    [Command]
    public void CmdGameOver()
    {
        ServerGameOver();
    }

    #endregion


}
