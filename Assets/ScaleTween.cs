using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTween : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // LeanTween.scale(gameObject, new Vector3(0,0,0), 0.5f).setOnComplete(DestroyMe);
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }

    public void ScaleAway()
    {
        LeanTween.scale(gameObject, new Vector3(0,0,0), 0.5f);
    }

    public void ScaleUp()
    {
        LeanTween.scale(gameObject, new Vector3(1,1,1), 0.5f);
    }
}
