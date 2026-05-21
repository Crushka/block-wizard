using System.Collections;
using UnityEngine;

public class BeeAI : MonoBehaviour, IDamageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundMask;

    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float hoverHeight = 2.5f;
    [SerializeField] private float heightAdjustSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Настройки атаки")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f; // Пауза перед следующей попыткой
    [SerializeField] private float windUpTime = 0.6f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDamage = 20f;
    [SerializeField] private float dashDuration = 0.8f;

    [Header("Состояние")]
    public float health = 30f;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackAvailableTime = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            // Важно для рывка, чтобы пчела не крутилась от физики
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null || isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        LookAtPlayer();

        // Атакуем, если подошли близко и время перезарядки прошло
        if (distance <= attackRange && Time.time >= nextAttackAvailableTime)
        {
            StartCoroutine(StingAttack());
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;

        // Двигаемся только если не слишком близко, чтобы не "тереться" об игрока во время КД
        if (Vector3.Distance(transform.position, player.position) > 2f)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }
        MaintainHeight();
    }

    void MaintainHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 10f, groundMask))
        {
            float targetY = hit.point.y + hoverHeight;
            float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * heightAdjustSpeed);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    IEnumerator StingAttack()
    {
        isAttacking = true;

        // 1. ПОДГОТОВКА (Wind-up)
        float elapsed = 0;
        while (elapsed < windUpTime)
        {
            // Плавно наводимся на центр игрока
            Vector3 targetPoint = player.position + Vector3.up * 1.0f;
            Vector3 lookDir = (targetPoint - transform.position).normalized;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed * 2f);
            }

            transform.position += Random.insideUnitSphere * 0.03f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. ФИКСАЦИЯ ЦЕЛИ (Ключевой момент)
        // Мы берем ТОЧКУ, где игрок находится СЕЙЧАС, и вычисляем вектор один раз
        Vector3 finalTargetPoint = player.position + Vector3.up * 1.0f;
        Vector3 dashDirection = (finalTargetPoint - transform.position).normalized;

        // Поворачиваем пчелу строго по этому вектору
        transform.rotation = Quaternion.LookRotation(dashDirection);

        // 3. РЫВОК (Dash)
        float dashTimer = 0;
        while (dashTimer < dashDuration)
        {
            // Принудительно ставим скорость каждый кадр, чтобы пчела не тормозила
            rb.linearVelocity = dashDirection * dashSpeed;

            dashTimer += Time.deltaTime;
            yield return null;
        }

        StopDash();
    }

    private void StopDash()
    {
        if (isDead) return;

        rb.linearVelocity = Vector3.zero;
        isAttacking = false;
        nextAttackAvailableTime = Time.time + attackCooldown;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttacking || isDead) return;

        // Ищем компонент урона в объекте, в который врезались, ИЛИ в его родителях
        // Это решит проблему, если пчела попала в "ногу", а скрипт на "голове"
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

        if (damageable != null && (collision.gameObject.CompareTag("PlayerBody") || collision.gameObject.CompareTag("Player")))
        {
            damageable.takeDamage(dashDamage);
            Die();
        }
        else if (!(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")))
        {
            // Врезались в стену или пол
            StopDash();
        }
    }

    public void takeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;
        if (health <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None; // Чтобы тушка падала реалистично
        rb.linearVelocity = Vector3.zero;

        Destroy(gameObject, 1.5f);
    }
}