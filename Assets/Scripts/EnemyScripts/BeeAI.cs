using System.Collections;
using UnityEngine;

public class BeeAI : MonoBehaviour, IDamageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundMask;

    [Header("Настройки обнаружения")]
    [SerializeField] private float detectionRange = 15f;

    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float hoverHeight = 2.5f;
    [SerializeField] private float minHorizontalDistance = 2.5f;
    [SerializeField] private float heightAdjustSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Настройки атаки")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
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

        if (distance > detectionRange) return;

        LookAtPlayer();

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
        Vector3 playerPosXZ = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDist = Vector3.Distance(playerPosXZ, myPosXZ);

        if (horizontalDist > minHorizontalDistance)
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }

        MaintainHeight();
    }

    void MaintainHeight()
    {
        RaycastHit hit;
        float targetY;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 30f, groundMask))
        {
            targetY = hit.point.y + hoverHeight;
        }
        else
        {
            targetY = player.position.y + hoverHeight;
        }

        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * heightAdjustSpeed);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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
        float elapsed = 0;
        while (elapsed < windUpTime)
        {
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
        Vector3 finalTargetPoint = player.position + Vector3.up * 1.0f;
        Vector3 dashDirection = (finalTargetPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dashDirection);
        float dashTimer = 0;
        while (dashTimer < dashDuration)
        {
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
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null && (collision.gameObject.CompareTag("PlayerBody") || collision.gameObject.CompareTag("Player")))
        {
            damageable.takeDamage(dashDamage);
            Die();
        }
        else if (!(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")))
        {
            StopDash();
        }
    }

    public void takeDamage(float damage)
    {
        Debug.Log($"Пчела получила урон! Текущее HP: {health}");
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
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        Destroy(gameObject, 1.5f);
    }
}