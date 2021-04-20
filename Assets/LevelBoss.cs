using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LevelBoss : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health = null;
    GameOverHandler gameOverHandler = null;

    #region Server

    public override void OnStartServer()
    {
        gameOverHandler = GameObject.Find("GameOverHandler").GetComponent<GameOverHandler>();
        health.ServerOnInjured += ServerHandleUnitInjured;
    }

    public override void OnStopServer()
    {
        gameOverHandler = GameObject.Find("GameOverHandler").GetComponent<GameOverHandler>();
        health.ServerOnInjured -= ServerHandleUnitInjured;
    }

    [Server]
    private void ServerHandleUnitInjured()
    {
        gameOverHandler.ServerGameOver();
    }

    #endregion
}
