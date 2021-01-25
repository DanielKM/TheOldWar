using System.Collections;
using Mirror;
using UnityEngine;

public class DomeShield : MonoBehaviour
{
    [SerializeField] private int secondsActivated = 5;
  
    public void Start()
    {
        StartCoroutine(DomeShieldStart(secondsActivated));
    }

    [Server]
    public IEnumerator DomeShieldStart(int seconds)
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
