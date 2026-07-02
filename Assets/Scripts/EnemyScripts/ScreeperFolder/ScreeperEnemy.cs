using UnityEngine;
using UnityEngine.AI;

public class ScreeperEnemy : Enemy
{
    [Header("Настройки Скрипера")]
    public float detectionRange = 25f;  // Радиус обнаружения игрока — до этого стоит на месте
    public float shootingRange = 20f;   // Дальность стрельбы (должна быть <= detectionRange)
    public float hideDistance = 15f;
    public float laughDuration = 1.0f;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip laughSound;

    [Header("Снаряд")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float bulletSpeed = 40f;
    public float bulletDamage = 15f;

    [Header("Точность упреждения")]
    [Range(0f, 1f)]
    public float leadAccuracy = 0.8f;

    [HideInInspector] public NavMeshAgent agent;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        base.InitializeAI();
        agent = GetComponent<NavMeshAgent>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnDrawGizmosSelected()
    {
        // Радиус обнаружения — синий
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Зона стрельбы — зелёный
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }

    public void PlayLaughSound()
    {
        if (audioSource != null && laughSound != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.2f);
            audioSource.PlayOneShot(laughSound);
        }
    }

    public override void takeDamage(float amount)
    {
        if (isDead) return;
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        if (agent != null) agent.enabled = false;
        Destroy(gameObject);
    }

    public override void Regeneration() { }
}