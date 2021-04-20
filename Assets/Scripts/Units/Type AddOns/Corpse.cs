using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    GameobjectLists goLists = null;
    // Start is called before the first frame update
    void Start()
    {
        goLists = GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>();
        goLists.units.Add(this.gameObject.GetComponent<Unit>());
    }

    void OnDestroy()
    {
        goLists.units.Remove(this.gameObject.GetComponent<Unit>());
    }
}
