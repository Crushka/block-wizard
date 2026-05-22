using UnityEngine;

public class BatAI : MonoBehaviour, IDamageable
{
    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundMask;

    [Header("Настройки обнаружения")]
    [SerializeField] private float detectionRange = 20f;

    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float speedMultiplier = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float leadAccuracy = 0.6f;

    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float heightAdjustSpeed = 2f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float minHorizontalDist = 5f;
    [SerializeField] private float hoverHeight = 4f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Состояние")]
    public float health = 100f;
    private bool isDead = false;
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
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectionRange) return;

        CalculatePlayerVelocity();
        LookAtPlayer();


        Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDist = Vector3.Distance(playerXZ, myXZ);

        if (horizontalDist > stopDistance)
        {
            MoveBat();
        }
        else
        {
            MaintainHeight();
        }

        TryShoot();
    }

    void MoveBat()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        MaintainHeight();
    }

    void MaintainHeight()
    {
        RaycastHit hit;
        float targetY;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 40f, groundMask))
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

    void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Vector3 playerCenter = player.position + Vector3.up * 1.0f;

            bool blocked = Physics.Linecast(firePoint.position, playerCenter, obstacleMask);
            if (!blocked)
            {
                Shoot(playerCenter);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot(Vector3 targetCenter)
    {
        if (projectilePrefab == null) return;

        float travelTime = GetArcTime(firePoint.position, targetCenter, arcHeight, speedMultiplier);

        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
        float predictTime = Mathf.Min(travelTime, 1.2f);
        Vector3 predictedTarget = targetCenter + (horizontalVel * predictTime * leadAccuracy);

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
        return (displacementXZ / totalTime) + velocityY;
    }

    void CalculatePlayerVelocity()
    {
        Vector3 currentPlayerPos = player.position;
        float dt = Time.deltaTime > 0 ? Time.deltaTime : 0.01f;
        playerVelocity = (currentPlayerPos - lastPlayerPosition) / dt;
        lastPlayerPosition = currentPlayerPos;
        if (playerVelocity.magnitude > 20f) playerVelocity = Vector3.zero;
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
        if (health <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
        Destroy(gameObject, 1.5f);
    }
}