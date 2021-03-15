using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Foundation : NetworkBehaviour
{
    public GameObject buildingPrefab;
    private GameObject buildingToBuild;
    [SerializeField] private GameObject progressBarParent = null;
    [SerializeField] private Image progressBarImage = null;

    [SyncVar(hook = nameof(HandleProgressUpdated))]
    public int progress = 0;
    public int maxProgress = 100;

    public event Action<int, int> ClientOnProgressUpdated;
    
    public int GetProgress()
    {
        return progress;
    }

    public void SetProgress(int amount)
    {
        progress += amount;
        if(progress >= maxProgress) 
        {
            CreateStructure();
        }
    }

    private void OnMouseEnter()
    {
        progressBarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        progressBarParent.SetActive(false);
    }

    public void CreateStructure()
    {
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);

        buildingToBuild = Instantiate(buildingPrefab, gameObject.transform.position, Quaternion.identity);
        buildingToBuild.transform.rotation = gameObject.transform.rotation;
        buildingToBuild.GetComponent<UnitInformation>().owner = gameObject.GetComponent<UnitInformation>().owner;
        buildingToBuild.GetComponent<UnitInformation>().team = gameObject.GetComponent<UnitInformation>().team;

        NetworkServer.Spawn(buildingToBuild, connectionToClient);
    }
    
    #region Client

    private void HandleProgressUpdated(int oldProgress, int newProgress)
    {
        ClientOnProgressUpdated?.Invoke(newProgress, maxProgress);

        ClientHandleProgressUpdated(newProgress, maxProgress);

        if(gameObject.GetComponent<UnitInformation>().selected == false) { return; }
    }

    private void ClientHandleProgressUpdated(int currentProgress, int maxProgress)
    {
        progressBarImage.fillAmount = (float)currentProgress/maxProgress;
    }

    #endregion
}