using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : MonoBehaviour
{
    public float checkRate = 1.0f;

    private ResourceNode[] resources;

    private RTSPlayer player;

    void Awake()
    {
        player = GetComponent<RTSPlayer>();
    }

    void Start()
    {
        resources = FindObjectsOfType<ResourceNode>();

        InvokeRepeating("Check", 0.0f, checkRate);
    }

    void Check()
    {
        // If afford, make new unit
    }
}
