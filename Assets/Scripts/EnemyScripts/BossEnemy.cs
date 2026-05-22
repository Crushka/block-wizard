using UnityEngine;


public class BossEnemy : MonoBehaviour, IDamageable
{
    [Header("Идентификатор")]
    [Tooltip("Уникальный id этого босса. Должен быть одинаковым во всех сценах/сессиях.")]
    [SerializeField] private string bossId = "boss_forest_01";

    [Header("Параметры")]
    [SerializeField] private float maxHealth = 500f;

    private float _health;
    private bool _isDead = false;

    void Awake()
    {
        _health = maxHealth;

        
        var gsm = GameStateManager.Instance;
        if (gsm != null && gsm.DeadBossIds.Contains(bossId))
        {
            Debug.Log($"[BossEnemy] '{bossId}' уже мёртв — деактивируем.");
            gameObject.SetActive(false);
        }
    }

  

    public void takeDamage(float amount)
    {
        if (_isDead) return;

        _health -= amount;
        Debug.Log($"[BossEnemy '{bossId}'] HP: {_health:F0}/{maxHealth}");

        if (_health <= 0f)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log($"[BossEnemy] '{bossId}' убит.");

        var gsm = GameStateManager.Instance;
        if (gsm != null)
        {
            if (!gsm.DeadBossIds.Contains(bossId))
                gsm.DeadBossIds.Add(bossId);

           
            SaveSystem.Save(gsm);
        }

        OnDeath();
    }

    protected virtual void OnDeath()
    {
        gameObject.SetActive(false);
    }


    public string BossId => bossId;
    public float HealthFraction => _health / maxHealth;
    public bool IsDead => _isDead;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f,
            $"Boss: '{bossId}'\nHP: {maxHealth}");
    }
#endif
}
