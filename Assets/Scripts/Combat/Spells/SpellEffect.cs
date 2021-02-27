using System.Collections;
using Mirror;
using UnityEngine;

public class SpellEffect : MonoBehaviour
{
    [SerializeField] private int secondsActivated = 10;
  
    public void Start()
    {
        StartCoroutine(DestroyAfterSeconds(secondsActivated));
    }

    [Server]
    public IEnumerator DestroyAfterSeconds(int seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        DestroySelf();
    }

    [Server]
    private void DestroySelf() 
    {
        NetworkServer.Destroy(gameObject);
    }
}
