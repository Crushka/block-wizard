using UnityEngine;

public class BatProj : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private Transform target;

    private bool isHoming = false;
    [Header("Настройки наводки")]
    public float homingStrength = 15f;
    public float homingDuration = 0.5f;     // Наводится первые 0.5 сек
    public float disableHomingDist = 5f;    // Выключает наводку за 5 метров до цели

    private float aliveTime = 0f;
    private Rigidbody rb;

    public void SetupDirect(Vector3 dir, float moveSpeed, float dmg, Transform homingTarget = null)
    {
        direction = dir;
        speed = moveSpeed;
        damage = dmg;
        target = homingTarget;
        isHoming = target != null;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * speed;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        if (!isHoming || target == null) return;

        aliveTime += Time.fixedDeltaTime;
        float distToTarget = Vector3.Distance(transform.position, target.position);

        // Условие отключения наводки (чтобы не было "заворота" в конце)
        if (aliveTime > homingDuration || distToTarget < disableHomingDist)
        {
            isHoming = false;
            return;
        }

        // Логика наведения (в грудь игрока)
        Vector3 targetCenter = target.position;
        Vector3 dirToTarget = (targetCenter - transform.position).normalized;

        // Плавный, но уверенный поворот вектора
        direction = Vector3.RotateTowards(direction, dirToTarget, homingStrength * Time.fixedDeltaTime, 0f);

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            rb.MoveRotation(Quaternion.LookRotation(direction));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Bat") || other.isTrigger) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && (other.CompareTag("Player") || other.CompareTag("PlayerBody")))
        {
            damageable.takeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}