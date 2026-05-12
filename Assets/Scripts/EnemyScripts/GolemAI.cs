using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GolemAI : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float health = 100f;
    public bool isDead = false;

    [Header("References")]
    public Transform target;
    public GameObject stonePrefab;
    public Transform throwPoint;

    [Header("Settings")]
    public float lookRadius = 20f;
    public float rangedRange = 15f;
    public float stopDistance = 2f;
    public float attackRate = 3f;
    public float windUpTime = 1.0f;
    public float stoneSpeed = 30f;

    [Header("Prediction")]
    [Range(0f, 1f)]
    [SerializeField] private float leadAccuracy = 0.8f;

    [Header("Meteor Attack Settings")]
    public GameObject slamMarkerPrefab;
    public float slamDamage = 40f;
    public float slamRadius = 5f;
    public float jumpCooldown = 10f;
    private float nextJumpTime = 0f;
    public LayerMask groundMask;
    public float centerMarkerSize = 0.8f;
    public float stoneMarkerSize = 0.05f;

    [Header("Circle Attack Settings")]
    public int rocksInFirstCircle = 4;
    public float firstCircleRadius = 4f;
    public int rocksInSecondCircle = 8;
    public float secondCircleRadius = 8f;

    private float nextAttackTime = 0f;
    private NavMeshAgent agent;
    private int enemyLayerMask;
    private bool isAttacking = false;

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    private CharacterController targetController;

    public static List<GolemAI> AllGolems = new List<GolemAI>();

    void OnEnable()
    {
        AllGolems.Add(this);
    }

    void OnDisable()
    {
        AllGolems.Remove(this);
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = stopDistance;
        enemyLayerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (target != null)
        {
            targetController = target.GetComponent<CharacterController>();
            lastPlayerPosition = target.position;
        }
    }

    private void Update()
    {
        if (targetController != null)
        {
            playerVelocity = targetController.velocity;
        }
        else
        {
            playerVelocity = (target.position - lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = target.position;
        }

        if (playerVelocity.magnitude > 50f) playerVelocity = Vector3.zero;

        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadius)
        {
            HandleBehavior(distance);
        }
        else
        {
            agent.isStopped = true;
        }

        HandleRotation(distance <= lookRadius);
    }

    private void HandleBehavior(float distance)
    {
        float healthPercent = health / maxHealth;

        if (healthPercent > 0.5f)
        {
            if (distance <= stopDistance + 0.5f)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                TryAttack(isRanged: false);
            }
            else if (distance <= rangedRange)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                TryAttack(isRanged: true);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
        else
        {
            if (Time.time >= nextJumpTime && !isAttacking)
            {
                StartCoroutine(MeteorSlamRoutine());
                nextJumpTime = Time.time + jumpCooldown;
            }
            else if (!isAttacking)
            {
                if (distance <= stopDistance + 0.5f)
                {
                    agent.isStopped = true;
                    TryAttack(isRanged: false);
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
            }
        }
    }

    private void TryAttack(bool isRanged)
    {
        if (Time.time >= nextAttackTime)
        {
            if (isRanged) StartCoroutine(ThrowStoneRoutine());
            else MeleeAttack();

            nextAttackTime = Time.time + attackRate;
        }
    }

    private void MeleeAttack() => Debug.Log("ГОЛЕМ: Ближний удар!");

    private IEnumerator MeteorSlamRoutine()
    {
        isAttacking = true;
        agent.enabled = false;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            transform.position += Vector3.up * 50f * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach (var r in rs) r.enabled = false;

        yield return new WaitForSeconds(0.5f);

        Vector3 centerPoint = GetGroundPosition(target.position);
        List<Vector3> dangerPoints = new List<Vector3>();
        dangerPoints.Add(centerPoint);

        for (int i = 0; i < rocksInFirstCircle; i++)
        {
            float angle = i * Mathf.PI * 2 / rocksInFirstCircle;
            Vector3 pos = centerPoint + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * firstCircleRadius;
            dangerPoints.Add(GetGroundPosition(pos));
        }
        for (int i = 0; i < rocksInSecondCircle; i++)
        {
            float angle = i * Mathf.PI * 2 / rocksInSecondCircle;
            Vector3 pos = centerPoint + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * secondCircleRadius;
            dangerPoints.Add(GetGroundPosition(pos));
        }

        List<GameObject> activeMarkers = new List<GameObject>();
        foreach (Vector3 pos in dangerPoints)
        {
            if (slamMarkerPrefab != null)
            {
                GameObject m = Instantiate(slamMarkerPrefab, pos, Quaternion.identity);

                if (pos == centerPoint)
                    m.transform.localScale = Vector3.one * centerMarkerSize;
                else
                    m.transform.localScale = Vector3.one * stoneMarkerSize;

                activeMarkers.Add(m);
            }
        }

        yield return new WaitForSeconds(2.0f);

        for (int i = 1; i < dangerPoints.Count; i++)
        {
            SpawnFallingStone(dangerPoints[i]);
        }
        foreach (var m in activeMarkers) Destroy(m);

        transform.position = centerPoint + Vector3.up * 40f;
        foreach (var r in rs) r.enabled = true;

        while (transform.position.y > centerPoint.y + 0.5f)
        {
            transform.position += Vector3.down * 75f * Time.deltaTime;
            yield return null;
        }

        transform.position = centerPoint;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (var col in hitColliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if(damageable != null && col.gameObject != this.gameObject)
            {
                damageable.takeDamage(slamDamage);
            }
        }

        yield return new WaitForSeconds(1f);
        agent.enabled = true;
        isAttacking = false;
    }

    private Vector3 GetGroundPosition(Vector3 startPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(startPos + Vector3.up * 10f, Vector3.down, out hit, 20f, groundMask))
        {
            return hit.point + Vector3.up * 0.02f;
        }
        return startPos;
    }

    private void SpawnFallingStone(Vector3 targetPos)
    {
        if (stonePrefab == null) return;

        GameObject stone = Instantiate(stonePrefab, targetPos + Vector3.up * 20f, Quaternion.identity);
        Rigidbody rb = stone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.down * 20f;
        }

        Destroy(stone, 3f);
    }


    private IEnumerator ThrowStoneRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Debug.Log("ГОЛЕМ: Начал замах... (игрок, беги!)");

        yield return new WaitForSeconds(windUpTime);

        if (!isDead && target != null)
        {
            ThrowStone();
        }

        isAttacking = false;
    }

    private void ThrowStone()
    {
        if (stonePrefab == null || throwPoint == null) return;

        float dist = Vector3.Distance(throwPoint.position, target.position);
        float travelTime = dist / stoneSpeed;

        // Считаем позицию игрока + упреждение
        Vector3 predictedTarget = target.position + (playerVelocity * travelTime * leadAccuracy);

        // ИСПРАВЛЕНИЕ: Целимся в живот (чуть ниже центра), чтобы не попадать в шляпу
        float yOffset = (targetController != null) ? (targetController.height * 0.3f) : 0.8f;
        predictedTarget += Vector3.up * yOffset;

        Vector3 throwDir = (predictedTarget - throwPoint.position).normalized;

        GameObject stone = Instantiate(stonePrefab, throwPoint.position, Quaternion.LookRotation(throwDir));
        Rigidbody rb = stone.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = throwDir * stoneSpeed;
        }

        Debug.Log("ГОЛЕМ: Бросок!");
        Destroy(stone, 5f);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        agent.isStopped = true;
        Destroy(gameObject, 0.5f);
    }

    private void HandleRotation(bool isChasing)
    {
        if (isDead) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    public void takeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("Голем получил урон: " + amount + ", Осталось: " + health);

        if(health <= 0)
        {
            Die();
        }
    }
}