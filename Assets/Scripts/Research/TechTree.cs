using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TechTree : MonoBehaviour
{
    public bool workerUnlocked = false;
    public bool archerUnlocked = false;
    public bool swordsmanUnlocked = false;
    public bool footmanUnlocked = false;
    public bool outriderUnlocked = false;
    public bool knightUnlocked = false;

    RTSPlayer player;
    private List<Building> playerBuildings = new List<Building>();
    UIController playerUI;
    Buttons playerTrainingButtons;

    public void UpdateUnlocks(Building updatedBuilding) 
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.name == "Scene_Menu") { return; }

        player = gameObject.GetComponent<RTSPlayer>();
        playerUI = GameObject.Find("UI").GetComponent<UIController>();
        playerTrainingButtons = playerUI.gameObject.GetComponent<Buttons>();

        bool townHallUnlocked = false;
        bool lumberYardUnlocked = false;
        bool barracksUnlocked = false;
        bool blacksmithUnlocked = false;
        bool stablesUnlocked = false;

        playerBuildings = player.GetMyBuildings();

        foreach(Building building in playerBuildings)
        {
            UnitType buildingType = building.GetComponent<UnitInformation>().unitType;

            if(buildingType == UnitType.TownHall) { townHallUnlocked = true; }

            if(buildingType == UnitType.Barracks) { barracksUnlocked = true; }

            if(buildingType == UnitType.Stables) { stablesUnlocked = true; }

            if(buildingType == UnitType.Blacksmith) { blacksmithUnlocked = true; }

            if(buildingType == UnitType.LumberYard) { lumberYardUnlocked = true; }
        }

        playerTrainingButtons.buttonOne.interactable = townHallUnlocked;
        playerTrainingButtons.barracksButtonOne.interactable = barracksUnlocked;
        playerTrainingButtons.barracksButtonTwo.interactable = blacksmithUnlocked;
        playerTrainingButtons.barracksButtonThree.interactable = lumberYardUnlocked;
        playerTrainingButtons.barracksButtonFour.interactable = stablesUnlocked;
        playerTrainingButtons.barracksButtonFive.interactable = stablesUnlocked && blacksmithUnlocked ? true : false;
    }
}