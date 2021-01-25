using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class CursorHit : NetworkBehaviour
{
    [SerializeField] private float destroyAfterSeconds = 5f;

    void OnEnable()
    {        
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    private void DestroySelf()
    {
        // NetworkServer.Destroy(gameObject);

        Destroy(gameObject);
    }

    private void DeactivateSelf() 
    {
        gameObject.SetActive(false);
    }

    private void OnDisable() 
    {
        CancelInvoke();
    }
}
