using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resources : MonoBehaviour
{

    public enum ResourceTypes { Gold, Iron, Steel, Skymetal, Wood, Stone, Food, Population }
    public List<ResourceAmount> resourceAmounts;

    public static Resources Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else 
        {
            Instance = this;
        }
    }

    public ResourceAmount GetResourceAmount(ResourceTypes resource)
    {
        return resourceAmounts.Find(r=>r.resourceType == resource);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class ResourceAmount
{
    public Resources.ResourceTypes resourceType;
    public int resourceAmount;
}

