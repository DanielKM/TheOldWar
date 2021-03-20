using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour, IPointerDownHandler
{
    public string levelToLoad = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        GameObject.Find("Lobby").GetComponent<LobbyMenu>().SelectLevel(levelToLoad);
    }
}
