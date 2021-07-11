using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Unit unit = null;

    private RTSPlayer player;
    UIController UI = null;

    public bool testing;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();

        testing = GameObject.Find("Testing").GetComponent<Testing>().testing;

        if(!testing) 
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI.pointerExitedButton = false;

        UI.OpenUnitCostPanel(unit.GetPrice(), unit.GetDescription(), player);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI.pointerExitedButton = true;

        UI.CloseUnitCostPanel();
    }

    public void Update()
    {
        if(testing)
        {
            if(player == null)
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }
        }
    }
}
