using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitCommandGiver : NetworkBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;

    private void Start() 
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if(!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if(target.hasAuthority && target.TryGetComponent<ResourceDropOff>(out ResourceDropOff dropOff))
            {
                TryResourceDropOffTarget(target);
                return;
            }
            if(target.hasAuthority && target.TryGetComponent<Foundation>(out Foundation foundation))
            {
                TryTarget(target);
                return;
            }
            if(target.hasAuthority) 
            {
                TryMove(hit);
                return;
            } 
            if(target.TryGetComponent<ResourceNode>(out ResourceNode node))
            {
                TryResourceTarget(target);
                return;
            }
            TryTarget(target);
            return;
        }

        TryMove(hit);
    }


    private void TryMove(RaycastHit hit)
    {
        if(unitSelectionHandler.SelectedUnits.Count <= 0) { return; }

        // UnitMovement firstUnitMovement = unitSelectionHandler.SelectedUnits[0].gameObject.GetComponent<UnitMovement>();
        
        // firstUnitMovement.unitAudio.clip = firstUnitMovement.unitMovingClip;

        // firstUnitMovement.unitAudio.Play();

        CreateBoxFormation(hit, unitSelectionHandler.SelectedUnits);
    }

    private void TryTarget(Targetable target)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) 
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }
    
    private void TryResourceDropOffTarget(Targetable target)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) 
        {
            unit.GetTargeter().CmdSetResourceDropOffTarget(target.gameObject);
        }
    }

    private void TryResourceTarget(Targetable target)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) 
        {
            unit.GetTargeter().CmdSetResourceTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

    public void CreateBoxFormation(RaycastHit hit, List<Unit> formationList) {
        float row = 0.0f;
        float rowOffset = 1.2f;
        int counter = 0;
        if(formationList.Count == 1) { 
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                formationList[iteration].GetComponent<NavMeshAgent>().enabled = true;
                formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x, hit.point.y, hit.point.z));           
            }
        } else if(formationList.Count <= 4) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                formationList[iteration].GetComponent<NavMeshAgent>().enabled = true;
                if(iteration <= 1) {
                    if(iteration % 2 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 2 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x + rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                } else {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x - rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                }
            }
        } else if(formationList.Count > 4 && formationList.Count <= 16) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                formationList[iteration].GetComponent<NavMeshAgent>().enabled = true;
                if(iteration <= 1) {
                    if(iteration % 4 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 4 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x + rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                } else {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x - rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                }
            }
        } else if(formationList.Count >= 16) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                formationList[iteration].GetComponent<NavMeshAgent>().enabled = true;
                if(iteration <= 1) {
                    if(iteration % 8 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 8 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x + rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                } else {
                    formationList[iteration].GetComponent<NavMeshAgent>().SetDestination(new Vector3(hit.point.x - rowOffset/2 * counter, hit.point.y, hit.point.z + row)); 
                }
            }
        } 		
    }

    [Command]
    public void CmdCreateBoxFormation(Vector3 rayCastPoint) {
        float row = 0.0f;
        float rowOffset = 1.2f;
        int counter = 0;

        UnitSelectionHandler unitSelectionHandler = GameObject.Find("UnitHandlers").GetComponent<UnitSelectionHandler>();

        List<Unit> formationList = unitSelectionHandler.SelectedUnits;

        if(formationList.Count == 1) { 
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x, rayCastPoint.y, rayCastPoint.z));           
            }
        } else if(formationList.Count <= 4) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                if(iteration <= 1) {
                    if(iteration % 2 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 2 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x + rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                } else {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x - rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                }
            }
        } else if(formationList.Count > 4 && formationList.Count <= 16) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                if(iteration <= 1) {
                    if(iteration % 4 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 4 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x + rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                } else {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x - rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                }
            }
        } else if(formationList.Count >= 16) {
            for(int iteration = 0; iteration < formationList.Count; iteration++) {
                if(iteration <= 1) {
                    if(iteration % 8 == 0) {
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                } else {
                    if(iteration % 8 == 0) {
                        row += rowOffset;
                        counter = 0;
                    } else {
                        counter += 1;
                    }
                }

                if(iteration % 2 == 0) {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x + rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                } else {
                    formationList[iteration].GetUnitMovement().ServerMove(new Vector3(rayCastPoint.x - rowOffset/2 * counter, rayCastPoint.y, rayCastPoint.z + row)); 
                }
            }
        } 		
    }

}
