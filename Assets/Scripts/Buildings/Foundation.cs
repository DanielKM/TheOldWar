using Mirror;
using UnityEngine;

public class Foundation : NetworkBehaviour
{
    public GameObject buildingPrefab;
    private GameObject buildingToBuild;

    public int progress = 0;
    
    public int GetProgress()
    {
        return progress;
    }

    public void SetProgress(int amount)
    {
        progress += amount;
        if(progress >= 100) 
        {
            CreateStructure();
        }
    }

    public void CreateStructure()
    {
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);

        buildingToBuild = Instantiate(buildingPrefab, gameObject.transform.position, Quaternion.identity);
        buildingToBuild.transform.rotation = gameObject.transform.rotation;
        buildingToBuild.GetComponent<UnitInformation>().owner = gameObject.GetComponent<UnitInformation>().owner;

        NetworkServer.Spawn(buildingToBuild, connectionToClient);
    }
}