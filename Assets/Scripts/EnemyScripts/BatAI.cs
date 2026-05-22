using UnityEngine;

public class BatAI : MonoBehaviour, IDamageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundMask;

    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float speedMultiplier = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float leadAccuracy = 1f;

    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float heightAdjustSpeed = 2f;
    [SerializeField] private float stopDistance = 4f;
    [SerializeField] private float reengageDistance = 6f;
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Состояние")]
    public float health = 100f;
    private bool isDead = false;
    private bool isInAttackRange = false;
    private float nextFireTime = 0f;

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null) player = playerObj.transform;
        }

        if (groundMask == 0) Debug.LogWarning("BatAI: Ground Mask не установлена!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        CalculatePlayerVelocity();

        float distance = Vector3.Distance(transform.position, player.position);

        LookAtPlayer();

        if (distance >= reengageDistance) isInAttackRange = false;

        if (!isInAttackRange && distance > stopDistance)
        {
            MoveBat();
        }
        else
        {
            isInAttackRange = true;
            MaintainHeight();
            TryShoot();
        }
    }

    void CalculatePlayerVelocity()
    {
        Vector3 currentPlayerPos = player.position;
        float dt = Time.deltaTime > 0 ? Time.deltaTime : 0.01f;
        playerVelocity = (currentPlayerPos - lastPlayerPosition) / dt;
        lastPlayerPosition = currentPlayerPos;

        if (playerVelocity.magnitude > 50f) playerVelocity = Vector3.zero;
    }

    void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Vector3 playerCenter = player.position + Vector3.up * 1.0f;

            bool blocked = Physics.Linecast(firePoint.position, playerCenter, obstacleMask);
            Debug.DrawLine(firePoint.position, playerCenter, blocked ? Color.red : Color.green);

            if (!blocked)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    void Shoot()
    {
        if (projectilePrefab == null) return;
        Vector3 diff = player.position - firePoint.position;
        float groundDist = new Vector3(diff.x, 0, diff.z).magnitude;

        float testTime = GetArcTime(firePoint.position, player.position, arcHeight, speedMultiplier);

        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
        Vector3 predictedTarget = player.position + (horizontalVelocity * testTime * leadAccuracy);

        Debug.DrawLine(firePoint.position, predictedTarget, Color.red, 1f);
        Debug.DrawRay(predictedTarget, Vector3.up * 2f, Color.red, 1f);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = CalculateArcVelocity(firePoint.position, predictedTarget, arcHeight, speedMultiplier);
        }

        Destroy(projectile, 5f);
    }

    float GetArcTime(Vector3 start, Vector3 target, float height, float speedScale)
    {
        float gravity = Physics.gravity.y * speedScale;
        float displacementY = target.y - start.y;
        float finalHeight = Mathf.Max(displacementY + 0.5f, height);

        float tPeak = Mathf.Sqrt(Mathf.Max(0, -2 * finalHeight / gravity));
        float tRemaining = Mathf.Sqrt(Mathf.Max(0, 2 * (displacementY - finalHeight) / gravity));
        return tPeak + tRemaining;
    }

    Vector3 CalculateArcVelocity(Vector3 start, Vector3 target, float height, float speedScale)
    {
        float gravity = Physics.gravity.y * speedScale;

        float displacementY = target.y - start.y;
        Vector3 displacementXZ = new Vector3(target.x - start.x, 0, target.z - start.z);

        float finalHeight = Mathf.Max(displacementY + 0.5f, height);

        float vY_val = -2 * gravity * finalHeight;
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(Mathf.Max(0, vY_val));

        float tPeak = Mathf.Sqrt(Mathf.Max(0, -2 * finalHeight / gravity));
        float tRemaining = Mathf.Sqrt(Mathf.Max(0, 2 * (displacementY - finalHeight) / gravity));
        float totalTime = tPeak + tRemaining;

        if (totalTime < 0.01f) return displacementXZ.normalized;

        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }

    void MoveBat()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        Vector3 moveStep = dir.normalized * moveSpeed * Time.deltaTime;
        transform.position += moveStep;
        MaintainHeight();
    }

    void MaintainHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 20f, groundMask))
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

    public void takeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;
        Debug.Log("Мышь получила урон, HP: " + health);
        if (health <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Мышь погибла!");

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = true;

        Destroy(gameObject, 1.5f);
    }
}