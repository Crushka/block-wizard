using UnityEngine;
using UnityEngine.AI;

public class ScreeperEnemy : Enemy
{
    [Header("Настройки Скрипера")]
    public float shootingRange = 20f;
    public float hideDistance = 15f;
    public float laughDuration = 1.0f;

    [Header("Звуки")]
    public AudioSource audioSource;
    //public AudioClip shootSound;
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
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    public override void Regeneration() { }
}