using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private GameObject progressIcon = null;
    [SerializeField] private Image healthBarImage = null;

    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
        
        if(!progressIcon) { return; }
        
        progressIcon.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);

        if(!progressIcon) { return; }

        progressIcon.SetActive(false);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth/maxHealth;
    }
}
