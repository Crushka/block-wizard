using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float maxHealth = 100f;

    public float GetHealth() { return health; }

    private void OnTriggerEnter(Collider other)
    {
        // Маленькая опечатка в твоем коде: "DathTrigger" -> проверь, совпадает ли с Layer в Unity
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

        // ВМЕСТО deadScreen.SetActive(true) обращаемся к выжившему Синглтону:
        if (DeadScreenUI.Instance != null)
        {
            DeadScreenUI.Instance.ShowDeadScreen();
        }
        else
        {
            Debug.LogError("[PlayerHealth] На сцене не найден DeadScreenUI.Instance!");
        }

        // Логику курсора мы перенесли внутрь ShowDeadScreen(), здесь она больше не нужна

        Destroy(transform.root.gameObject);
    }
}