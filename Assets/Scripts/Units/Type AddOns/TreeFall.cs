using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [SerializeField]
    public Health health;
    [SerializeField]
    public ExplodePartManager explode;
	public bool exploded = false;

    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        // do math
        if(exploded) { return; }

        if(currentHealth <= 70)
        {
            ExplodeTree();
        }
    }

    public void ExplodeTree()
    {
        if(exploded) { return; }

        explode.Explode();
        exploded = true;
    }
}
