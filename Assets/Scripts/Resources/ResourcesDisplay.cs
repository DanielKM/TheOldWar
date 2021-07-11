using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject goldText = null;
    [SerializeField] private GameObject ironText = null;
    [SerializeField] private GameObject steelText = null;
    [SerializeField] private GameObject skymetalText = null;
    [SerializeField] private GameObject woodText = null;
    [SerializeField] private GameObject stoneText = null;
    [SerializeField] private GameObject foodText = null;
    [SerializeField] private GameObject populationText = null;
    [SerializeField] private GameObject armySizeText = null;

    private RTSPlayer player;
    
    // SYSTEM VARIABLES
    public bool testing;

    private void Start()
    {
        testing = GameObject.Find("Testing").GetComponent<Testing>().testing;

        if(!testing) 
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            ClientHandleGoldUpdated(player.GetResources()[Resource.Gold]);
            ClientHandleIronUpdated(player.GetResources()[Resource.Iron]);
            ClientHandleSteelUpdated(player.GetResources()[Resource.Steel]);
            ClientHandleSkymetalUpdated(player.GetResources()[Resource.Skymetal]);
            ClientHandleWoodUpdated(player.GetResources()[Resource.Wood]);
            ClientHandleStoneUpdated(player.GetResources()[Resource.Stone]);
            ClientHandleFoodUpdated(player.GetResources()[Resource.Food]);
            ClientHandlePopulationUpdated(player.GetResources()[Resource.Population]);
            ClientHandleArmySizeUpdated(player.GetResources()[Resource.ArmySize]);

            ClientHandleMaxGoldUpdated(player.GetMaxResources()[Resource.Gold]);
            ClientHandleMaxIronUpdated(player.GetMaxResources()[Resource.Iron]);
            ClientHandleMaxSteelUpdated(player.GetMaxResources()[Resource.Steel]);
            ClientHandleMaxSkymetalUpdated(player.GetMaxResources()[Resource.Skymetal]);
            ClientHandleMaxWoodUpdated(player.GetMaxResources()[Resource.Wood]);
            ClientHandleMaxStoneUpdated(player.GetMaxResources()[Resource.Stone]);
            ClientHandleMaxFoodUpdated(player.GetMaxResources()[Resource.Food]);
            ClientHandleMaxPopulationUpdated(player.GetMaxResources()[Resource.Population]);
            ClientHandleMaxArmySizeUpdated(player.GetMaxResources()[Resource.ArmySize]);


            player.ClientOnGoldUpdated += ClientHandleGoldUpdated;
            player.ClientOnIronUpdated += ClientHandleIronUpdated;
            player.ClientOnSteelUpdated += ClientHandleSteelUpdated;
            player.ClientOnSkymetalUpdated += ClientHandleSkymetalUpdated;
            player.ClientOnWoodUpdated += ClientHandleWoodUpdated;
            player.ClientOnStoneUpdated += ClientHandleStoneUpdated;
            player.ClientOnFoodUpdated += ClientHandleFoodUpdated;
            player.ClientOnPopulationUpdated += ClientHandlePopulationUpdated;
            player.ClientOnArmySizeUpdated += ClientHandleArmySizeUpdated;
        
            player.ClientOnMaxGoldUpdated += ClientHandleMaxGoldUpdated;
            player.ClientOnMaxIronUpdated += ClientHandleMaxIronUpdated;
            player.ClientOnMaxSteelUpdated += ClientHandleMaxSteelUpdated;
            player.ClientOnMaxSkymetalUpdated += ClientHandleMaxSkymetalUpdated;
            player.ClientOnMaxWoodUpdated += ClientHandleMaxWoodUpdated;
            player.ClientOnMaxStoneUpdated += ClientHandleMaxStoneUpdated;
            player.ClientOnMaxFoodUpdated += ClientHandleMaxFoodUpdated;
            player.ClientOnMaxPopulationUpdated += ClientHandleMaxPopulationUpdated;
            player.ClientOnMaxArmySizeUpdated += ClientHandleMaxArmySizeUpdated;
        }
    }

    private void Update()
    {
        if(!testing)
        {
            if(player == null)
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
                
                ClientHandleGoldUpdated(player.GetResources()[Resource.Gold]);
                ClientHandleIronUpdated(player.GetResources()[Resource.Iron]);
                ClientHandleSteelUpdated(player.GetResources()[Resource.Steel]);
                ClientHandleSkymetalUpdated(player.GetResources()[Resource.Skymetal]);
                ClientHandleWoodUpdated(player.GetResources()[Resource.Wood]);
                ClientHandleStoneUpdated(player.GetResources()[Resource.Stone]);
                ClientHandleFoodUpdated(player.GetResources()[Resource.Food]);
                ClientHandlePopulationUpdated(player.GetResources()[Resource.Population]);
                ClientHandleArmySizeUpdated(player.GetResources()[Resource.ArmySize]);

                ClientHandleMaxGoldUpdated(player.GetMaxResources()[Resource.Gold]);
                ClientHandleMaxIronUpdated(player.GetMaxResources()[Resource.Iron]);
                ClientHandleMaxSteelUpdated(player.GetMaxResources()[Resource.Steel]);
                ClientHandleMaxSkymetalUpdated(player.GetMaxResources()[Resource.Skymetal]);
                ClientHandleMaxWoodUpdated(player.GetMaxResources()[Resource.Wood]);
                ClientHandleMaxStoneUpdated(player.GetMaxResources()[Resource.Stone]);
                ClientHandleMaxFoodUpdated(player.GetMaxResources()[Resource.Food]);
                ClientHandleMaxPopulationUpdated(player.GetMaxResources()[Resource.Population]);
                ClientHandleMaxArmySizeUpdated(player.GetMaxResources()[Resource.ArmySize]);

                player.ClientOnGoldUpdated += ClientHandleGoldUpdated;
                player.ClientOnIronUpdated += ClientHandleIronUpdated;
                player.ClientOnSteelUpdated += ClientHandleSteelUpdated;
                player.ClientOnSkymetalUpdated += ClientHandleSkymetalUpdated;
                player.ClientOnWoodUpdated += ClientHandleWoodUpdated;
                player.ClientOnStoneUpdated += ClientHandleStoneUpdated;
                player.ClientOnFoodUpdated += ClientHandleFoodUpdated;
                player.ClientOnPopulationUpdated += ClientHandlePopulationUpdated;
                player.ClientOnArmySizeUpdated += ClientHandleArmySizeUpdated;
                
                player.ClientOnMaxGoldUpdated += ClientHandleMaxGoldUpdated;
                player.ClientOnMaxIronUpdated += ClientHandleMaxIronUpdated;
                player.ClientOnMaxSteelUpdated += ClientHandleMaxSteelUpdated;
                player.ClientOnMaxSkymetalUpdated += ClientHandleMaxSkymetalUpdated;
                player.ClientOnMaxWoodUpdated += ClientHandleMaxWoodUpdated;
                player.ClientOnMaxStoneUpdated += ClientHandleMaxStoneUpdated;
                player.ClientOnMaxFoodUpdated += ClientHandleMaxFoodUpdated;
                player.ClientOnMaxPopulationUpdated += ClientHandleMaxPopulationUpdated;
                player.ClientOnMaxArmySizeUpdated += ClientHandleMaxArmySizeUpdated;
            }
        }
    }

    private void OnDestroy()
    {
        if(player == null) { return; }
        
        player.ClientOnGoldUpdated -= ClientHandleGoldUpdated;
        player.ClientOnIronUpdated -= ClientHandleIronUpdated;
        player.ClientOnSteelUpdated -= ClientHandleSteelUpdated;
        player.ClientOnSkymetalUpdated -= ClientHandleSkymetalUpdated;
        player.ClientOnWoodUpdated -= ClientHandleWoodUpdated;
        player.ClientOnStoneUpdated -= ClientHandleStoneUpdated;
        player.ClientOnFoodUpdated -= ClientHandleFoodUpdated;
        player.ClientOnPopulationUpdated -= ClientHandlePopulationUpdated;
        player.ClientOnArmySizeUpdated -= ClientHandleArmySizeUpdated;
        
        player.ClientOnMaxGoldUpdated -= ClientHandleMaxGoldUpdated;
        player.ClientOnMaxIronUpdated -= ClientHandleMaxIronUpdated;
        player.ClientOnMaxSteelUpdated -= ClientHandleMaxSteelUpdated;
        player.ClientOnMaxSkymetalUpdated -= ClientHandleMaxSkymetalUpdated;
        player.ClientOnMaxWoodUpdated -= ClientHandleMaxWoodUpdated;
        player.ClientOnMaxStoneUpdated -= ClientHandleMaxStoneUpdated;
        player.ClientOnMaxFoodUpdated -= ClientHandleMaxFoodUpdated;
        player.ClientOnMaxPopulationUpdated -= ClientHandleMaxPopulationUpdated;
        player.ClientOnMaxArmySizeUpdated -= ClientHandleMaxArmySizeUpdated;
    }

    private void ClientHandleGoldUpdated(int resources)
    {
        goldText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Gold];
    }

    private void ClientHandleMaxGoldUpdated(int resources)
    {
        goldText.GetComponent<Text>().text = player.GetResources()[Resource.Gold] + "/" + resources;
    }

    private void ClientHandleIronUpdated(int resources)
    {
        ironText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Iron];
    }

    private void ClientHandleMaxIronUpdated(int resources)
    {
        ironText.GetComponent<Text>().text = player.GetResources()[Resource.Iron] + "/" + resources;
    }

    private void ClientHandleSteelUpdated(int resources)
    {
        steelText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Steel];
    }
    
    private void ClientHandleMaxSteelUpdated(int resources)
    {
        steelText.GetComponent<Text>().text = player.GetResources()[Resource.Steel] + "/" + resources;
    }
    
    private void ClientHandleSkymetalUpdated(int resources)
    {
        skymetalText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Skymetal];
    }
    
    private void ClientHandleMaxSkymetalUpdated(int resources)
    {
        skymetalText.GetComponent<Text>().text = player.GetResources()[Resource.Skymetal] + "/" + resources;
    }
    
    private void ClientHandleWoodUpdated(int resources)
    {
        woodText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Wood] ;
    }

    private void ClientHandleMaxWoodUpdated(int resources)
    {
        woodText.GetComponent<Text>().text = player.GetResources()[Resource.Wood]  + "/" + resources;
    }

    private void ClientHandleStoneUpdated(int resources)
    {
        stoneText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Stone];
    }

    private void ClientHandleMaxStoneUpdated(int resources)
    {
        stoneText.GetComponent<Text>().text = player.GetResources()[Resource.Stone] + "/" + resources;
    }

    private void ClientHandleFoodUpdated(int resources)
    {
        foodText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Food];
    }

    private void ClientHandleMaxFoodUpdated(int resources)
    {
        foodText.GetComponent<Text>().text = player.GetResources()[Resource.Food] + "/" + resources;
    }
    
    private void ClientHandlePopulationUpdated(int resources)
    {
        populationText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.Population];
    }

    private void ClientHandleMaxPopulationUpdated(int resources)
    {
        populationText.GetComponent<Text>().text = player.GetResources()[Resource.Population] + "/" + resources;
    }
    
    private void ClientHandleArmySizeUpdated(int resources)
    {
        armySizeText.GetComponent<Text>().text = resources + "/" + player.GetMaxResources()[Resource.ArmySize];
    }

    private void ClientHandleMaxArmySizeUpdated(int resources)
    {
        armySizeText.GetComponent<Text>().text = player.GetResources()[Resource.ArmySize] + "/" + resources;
    }
}
