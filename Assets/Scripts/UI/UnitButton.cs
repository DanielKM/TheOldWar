using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Unit unit = null;

    private RTSPlayer player;
    UIController UI = null;

    private void Start()
    {
        UI = GameObject.Find("UI").GetComponent<UIController>();

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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
}
