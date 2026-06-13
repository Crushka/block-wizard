using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Автохил")]
    [Tooltip("Сколько HP восстанавливается за один тик")]
    [SerializeField] private float healAmount = 5f;

    [Tooltip("Интервал между тиками хила (в секундах)")]
    [SerializeField] private float healInterval = 3f;

    [Tooltip("Задержка хила после получения урона (в секундах)")]
    [SerializeField] private float healDelay = 5f;

    private float _healTimer = 0f;
    private float _timeSinceLastDamage = 0f;

    public float GetHealth() { return health; }

    private void Update()
    {
        if (health <= 0 || health >= maxHealth) return;

        _timeSinceLastDamage += Time.deltaTime;

        if (_timeSinceLastDamage < healDelay) return;

        _healTimer += Time.deltaTime;
        if (_healTimer >= healInterval)
        {
            _healTimer = 0f;
            health = Mathf.Min(health + healAmount, maxHealth);
            Debug.Log($"[AutoHeal] HP: {health:F1}/{maxHealth}");
        }
    }

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
        _timeSinceLastDamage = 0f; 
        _healTimer = 0f; 
        if (health <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("Игрок погиб");

        if (PlayerPersistence.Instance != null)
            PlayerPersistence.Instance.IsDead = true;

        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.PlayerHP = 0;

        if (DeadScreenUI.Instance != null)
        {
            if (PlayerPersistence.Instance != null)
                PlayerPersistence.Instance.IsDead = true;
            DeadScreenUI.Instance.ShowDeadScreen();
        }
        else
        {
            Debug.LogError("[PlayerHealth] На сцене не найден DeadScreenUI.Instance!");
        }

        Destroy(transform.root.gameObject);
    }
}