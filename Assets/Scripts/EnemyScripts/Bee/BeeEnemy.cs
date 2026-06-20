//using UnityEngine;

//public class BeeEnemy : Enemy
//{
//    [HideInInspector] public Rigidbody rb;

//    [Header("Настройки обнаружения")]
//    [SerializeField] public float detectionRange = 15f;

//    [Header("Настройки движения")]
//    [SerializeField] public float moveSpeed = 4f;
//    [SerializeField] public float hoverHeight = 2.5f;
//    [SerializeField] public float minHorizontalDistance = 2.5f;
//    [SerializeField] public float heightAdjustSpeed = 3f;
//    [SerializeField] public float rotationSpeed = 10f;
//    [SerializeField] public LayerMask groundMask;

//    [Header("Настройки атаки")]
//    [SerializeField] public float attackRange = 5f;
//    [SerializeField] public float attackCooldown = 2f;
//    [SerializeField] public float windUpTime = 0.6f;
//    [SerializeField] public float dashSpeed = 18f;
//    [SerializeField] public float dashDamage = 20f;
//    [SerializeField] public float dashDuration = 0.8f;


//    // ПРЕДОХРАНИТЕЛЬ ОТ МНОГОКРАТНОГО УРОНА:
//    private bool _hasDealtDamage = false;

//    private void Start()
//    {
//        InitializeAI();
//    }

//    public override void InitializeAI()
//    {
//        rb = GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.useGravity = false;
//            rb.constraints = RigidbodyConstraints.FreezeRotation;
//        }
//        base.InitializeAI();
//    }

//    // ИСПРАВЛЕННЫЙ МЕТОД СТОЛКНОВЕНИЙ
//    private void OnCollisionEnter(Collision collision)
//    {
//        if (isDead || _hasDealtDamage) return;

//        BeeBehaviour brain = baseAI as BeeBehaviour;
//        if (brain == null || !brain.IsAttacking) return;

//        // Ищем IDamageable с защитой от вложенности
//        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

//        bool isPlayer = collision.gameObject.CompareTag("PlayerBody") ||
//                        collision.gameObject.CompareTag("Player");

//        if (damageable != null && isPlayer)
//        {
//            _hasDealtDamage = true; // Блокируем повторные удары в этом же прыжке

//            damageable.takeDamage(dashDamage);
//            Debug.Log($"Пчела ужалила игрока на {dashDamage} урона!");
//            Die();
//        }
//        else if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
//        {
//            brain.StopDash();
//        }
//    }

//    public void ResetDamageFlag() => _hasDealtDamage = false;

//    public override void Die()
//    {
//        if (isDead) return;
//        isDead = true;
//        StopAllCoroutines();

//        if (rb != null)
//        {
//            rb.useGravity = true;
//            rb.constraints = RigidbodyConstraints.None;
//            rb.linearVelocity = Vector3.zero;
//        }

//        if (baseAI != null) baseAI.enabled = false;

//        Destroy(gameObject, 1.5f);
//    }

//    public override void takeDamage(float amount)
//    {
//        if (isDead) return;
//        HP -= amount;
//        if (HP <= 0) Die();
//    }

//    public override void Regeneration() { }
//}

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

    // ПРЕДОХРАНИТЕЛЬ ОТ МНОГОКРАТНОГО УРОНА:
    private bool _hasDealtDamage = false;

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
            // Важно: Linear Damping должен быть 5-8 в инспекторе,
            // чтобы пчела плавно тормозила, а не скользила бесконечно
        }
        base.InitializeAI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || _hasDealtDamage) return;

        BeeBehaviour brain = baseAI as BeeBehaviour;
        if (brain == null || !brain.IsAttacking) return;

        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

        bool isPlayer = collision.gameObject.CompareTag("PlayerBody") ||
                        collision.gameObject.CompareTag("Player");

        if (damageable != null && isPlayer)
        {
            _hasDealtDamage = true;

            damageable.takeDamage(dashDamage);
            Debug.Log($"Пчела ужалила игрока на {dashDamage} урона!");
            Die();
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            brain.StopDash();
        }
    }

    public void ResetDamageFlag() => _hasDealtDamage = false;

    public override void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.linearVelocity = Vector3.zero;
        }

        if (baseAI != null) baseAI.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    public override void takeDamage(float amount)
    {
        if (isDead) return;
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Regeneration() { }
}
