using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamParent : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
