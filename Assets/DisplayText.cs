using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayText : MonoBehaviour
{
    [SerializeField] private GameObject textParent = null;

    private void OnMouseEnter()
    {
        textParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        textParent.SetActive(false);
    }
}
