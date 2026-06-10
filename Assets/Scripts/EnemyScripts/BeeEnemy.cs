using Unity.VisualScripting;
using UnityEngine;

public class BeeEnemy : Enemy
{
    [HideInInspector] public Rigidbody rb;
    [Header("Настройки обнаружения")]
    [SerializeField] public float detectionRange = 15f;

    [Header("Настройки движения")]
    [SerializeField] public float moveSpeed = 4f;
    [SerializeField] public float hoverHeight = 2.5f;
    [SerializeField] public float minHorizontalDistance = 2.5f;
    [SerializeField] public float heightAdjustSpeed = 3f;
    [SerializeField] public float rotationSpeed = 10f;
    [SerializeField] public LayerMask groundMask;

    [Header("Настройки атаки")]
    [SerializeField] public float attackRange = 5f;
    [SerializeField] public float attackCooldown = 2f;
    [SerializeField] public float windUpTime = 0.6f;
    [SerializeField] public float dashSpeed = 18f;
    [SerializeField] public float dashDamage = 20f;
    [SerializeField] public float dashDuration = 0.8f;
    private bool isDead = false;


    private void Start()
    {
        InitializeAI();
    }

    public override void InitializeAI()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            Debug.Log("Работает rb");
        }
        base.InitializeAI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        BeeBehaviour brain = baseAI as BeeBehaviour;
        if(brain != null && !brain.isAttacking)
        {
            return;
        }
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && collision.gameObject.CompareTag("PlayerBody") || collision.gameObject.CompareTag("Player"))
        {
            damageable.takeDamage(dashDamage);
            Die();
        }
        else if (!(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")))
        {
            brain.StopDash();
        }
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        if (baseAI != null)
        {
            baseAI.enabled = false;
        }
        Destroy(gameObject, 1.5f);
    }

    public override void takeDamage(float amount)
    {
        HP -= amount;
        if(HP <= 0)
        {
            Die();
        }
    }
}
