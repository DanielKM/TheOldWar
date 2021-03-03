using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule 
{
    public PointerEventData GetLastPointerEventDataPublic (int id)
    {
        return GetLastPointerEventData(id);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
