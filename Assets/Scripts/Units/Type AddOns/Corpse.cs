using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>().units.Add(this.gameObject.GetComponent<Unit>());
    }

    void OnDestroy()
    {
        GameObject.Find("UnitHandlers").GetComponent<GameobjectLists>().units.Remove(this.gameObject.GetComponent<Unit>());
    }
}
