//using UnityEngine;
//using UnityEngine.AI;

//public class LarvaEnemy : Enemy
//{
//    [Header("Настройки Зародыша")]
//    public float attackRange = 4f; // Чуть больше, чтобы было место для разворота
//    public float dashForce = 18f;
//    public float dashDamage = 15f;
//    public float turnBeforeAttackSpeed = 15f;

//    [Header("Настройки Движения (Крещендо)")]
//    public float maxPulseSpeed = 8f;
//    public float pulseDuration = 1.0f;
//    public float restDuration = 0.5f;

//    [HideInInspector] public NavMeshAgent agent;
//    [HideInInspector] public Rigidbody rb;
//    [HideInInspector] public bool isDashingNow = false; // Флаг активной фазы атаки

//    private void Start() => InitializeAI();

//    public override void InitializeAI()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        rb = GetComponent<Rigidbody>();

//        if (agent == null)
//            Debug.LogError($"{name}: на LarvaEnemy нет компонента NavMeshAgent!");
//        if (rb == null)
//            Debug.LogError($"{name}: на LarvaEnemy нет компонента Rigidbody!");

//        if (agent != null)
//        {
//            agent.stoppingDistance = 0.1f; // Минимальная, чтобы не тупил
//            agent.updateRotation = true;
//            agent.acceleration = 1000f;
//        }

//        if (rb != null)
//        {
//            rb.isKinematic = true; // Пока ползем, физика не мешает
//            rb.useGravity = true;
//        }

//        base.InitializeAI();
//    }

//    // ЛОГИКА УДАРА ТЕЛОМ
//    private void OnCollisionEnter(Collision collision)
//    {
//        if (!isDashingNow) return;

//        // Если во время рывка задели игрока
//        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerBody"))
//        {
//            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
//            if (damageable != null)
//            {
//                damageable.takeDamage(dashDamage);
//                Debug.Log("Зародыш разбился об игрока!");
//                Die(); // Умирает при попадании
//            }
//        }
//        else if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
//        {
//            // Если врезался в стену во время рывка - тоже умирает (или просто останавливается)
//            Die();
//        }
//    }

//    public override void takeDamage(float amount)
//    {
//        HP -= amount;
//        if (HP <= 0) Die();
//    }

//    public override void Die()
//    {
//        StopAllCoroutines();
//        // Эффект брызг можно добавить тут
//        Destroy(gameObject);
//    }

//    public override void Regeneration() { }
//}

using UnityEngine;
using UnityEngine.AI;

public class LarvaEnemy : Enemy
{
    [Header("Настройки Зародыша")]
    public float attackRange = 4f;
    public float dashForce = 18f;
    public float dashDamage = 15f;
    public float turnBeforeAttackSpeed = 15f;

    [Header("Настройки Движения (Крещендо)")]
    public float maxPulseSpeed = 8f;
    public float pulseDuration = 1.0f;
    public float restDuration = 0.5f;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isDashingNow = false;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent == null)
            Debug.LogError($"{name}: на LarvaEnemy нет компонента NavMeshAgent!");
        if (rb == null)
            Debug.LogError($"{name}: на LarvaEnemy нет компонента Rigidbody!");

        if (agent != null)
        {
            agent.stoppingDistance = 0.1f;
            agent.updateRotation = true;
            agent.acceleration = 1000f;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }

        base.InitializeAI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDashingNow || isDead) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerBody"))
        {
            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.takeDamage(dashDamage);
                Debug.Log("Зародыш разбился об игрока!");
                Die();
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            // Врезался в стену — умирает
            Die();
        }
    }

    public override void takeDamage(float amount)
    {
        if (isDead) return; // Защита от повторного урона после смерти
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        if (isDead) return; // Защита от двойного вызова
        isDead = true;

        StopAllCoroutines();

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (baseAI != null) baseAI.enabled = false;

        // Эффект брызг можно добавить тут
        Destroy(gameObject);
    }

    public override void Regeneration() { }
}