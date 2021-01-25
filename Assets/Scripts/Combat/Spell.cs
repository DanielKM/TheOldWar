using System.Collections;
using Mirror;
using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int spellCost = 0;
  
    [TextArea]
    public string description;

    private Camera mainCamera;
    private RTSPlayer player;
    private UIController UI = null;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        
        mainCamera = Camera.main;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public int GetPrice()
    {
        return spellCost;
    }

    public string GetDescription()
    {
        return description;
    }
}
