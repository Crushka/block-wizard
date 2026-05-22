using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float maxHealth = 100f;

    public float GetHealth() { return health; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DathTrigger"))
        {
            Die();
        }
    }

    public void takeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("Игрок погиб");

        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.PlayerHP = 0;

        if (DeadScreenUI.Instance != null)
        {
            DeadScreenUI.Instance.ShowDeadScreen();
        }
        else
        {
            Debug.LogError("[PlayerHealth] На сцене не найден DeadScreenUI.Instance!");
        }


        Destroy(transform.root.gameObject);
    }
}