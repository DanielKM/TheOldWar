using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Movement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;

    [Command]
    public void CmdMove(Vector3 position) 
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position) 
    {
        targeter.ClearTarget();

        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        if(!agent.enabled) { return; }
        
        agent.SetDestination(hit.position);
    }
}
