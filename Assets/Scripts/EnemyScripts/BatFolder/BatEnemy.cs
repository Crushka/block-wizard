using UnityEngine;

public class BatEnemy : Enemy
{
    [Header("Настройки обнаружения")]
    public float detectionRange = 25f;

    [Header("Настройки движения (Рывки)")]
    public float dashSpeed = 18f;
    public float heightAdjustSpeed = 5f;
    public float stopDistance = 0.5f;
    public float minRadius = 6f;
    public float maxRadius = 12f;
    public float hoverHeight = 5f;
    public float rotationSpeed = 10f;

    [Header("Настройки атаки")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2.5f;
    public float chargeTime = 1.0f;
    public float bulletSpeed = 30f;
    public float bulletDamage = 12f;
    [Range(0f, 1f)]
    public float leadAccuracy = 0.8f;

    [Header("Физика")]
    public LayerMask groundMask;
    [HideInInspector] public Rigidbody rb;

    [Header("Визуал покачивания (Bobbing)")]
    public float bobSpeed = 3f;
    public float bobAmount = 0.5f;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;
        base.InitializeAI();
    }

    public override void takeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
        if (baseAI != null) baseAI.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public override void Regeneration() { }
}