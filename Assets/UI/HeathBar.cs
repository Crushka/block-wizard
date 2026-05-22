using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeathBar : MonoBehaviour
{
    [SerializeField] private Image healtBar;
    [SerializeField] private PlayerHealth playerHealth;

    private void Update()
    {
        if (healtBar != null && playerHealth != null)
        {
            healtBar.fillAmount = playerHealth.health / playerHealth.maxHealth;
        }
    }
}