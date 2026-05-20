using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoleAI : MonoBehaviour, IDamageable
{
    public enum MoleState { Burrowing, Positioning, Emerging, Attacking, Submerging }

    [Header("Состояние")]
    public MoleState currentState = MoleState.Burrowing;
    public float health = 50f;
    private bool isDead = false;

    [Header("Ссылки")]
    public Transform target;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject visuals;
    public GameObject burrowEffect;

    private NavMeshAgent agent;
    private Collider moleCollider;

    [Header("Настройки дистанции")]
    public float triggerDistance = 12f;
    public float ambushRadius = 5f;
    public float burrowSpeed = 5f;
    public float minBurrowTime = 3f;
    private float currentBurrowTimer = 0f;

    [Header("Настройки атаки")]
    public float attackDuration = 4f;
    public float fireRate = 1.2f;
    public float spitSpeed = 15f;
    public float rotationSpeed = 10f;
    private float nextFireTime = 0f;

    [Header("Тайминги")]
    public float emergeTime = 1f;
    public float submergeTime = 1f;

    private Vector3 ambushPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        moleCollider = GetComponent<Collider>();

        if (agent)
        {
            agent.updateRotation = false;
            agent.angularSpeed = 0;
        }

        FindTarget();
        EnterBurrowing();
    }

    private void FindTarget()
    {
        if (target == null || target.gameObject.scene.name == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (target == null)
        {
            FindTarget();
            return;
        }

        if (currentState == MoleState.Attacking || currentState == MoleState.Emerging)
            LookAtTarget();

        switch (currentState)
        {
            case MoleState.Burrowing:
                UpdateBurrowing();
                break;
            case MoleState.Positioning:
                UpdatePositioning();
                break;
            case MoleState.Attacking:
                UpdateAttacking();
                break;
        }
    }

    private void EnterBurrowing()
    {
        currentState = MoleState.Burrowing;
        currentBurrowTimer = minBurrowTime;

        if (agent && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.speed = burrowSpeed;
        }

        if (visuals) visuals.SetActive(false);
        if (burrowEffect) burrowEffect.SetActive(true);
        if (moleCollider) moleCollider.enabled = false;
    }

    private void UpdateBurrowing()
    {
        agent.SetDestination(target.position);

        if (currentBurrowTimer > 0)
        {
            currentBurrowTimer -= Time.deltaTime;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= triggerDistance)
        {
            SetAmbushPoint();
        }
    }

    private void SetAmbushPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * ambushRadius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 desiredPoint = target.position + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPoint, out hit, ambushRadius * 2, NavMesh.AllAreas))
        {
            ambushPoint = hit.position;
            currentState = MoleState.Positioning;
            agent.SetDestination(ambushPoint);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Не удалось найти точку на NavMesh!");
        }
    }

    private void UpdatePositioning()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(EmergeRoutine());
        }
    }

    private IEnumerator EmergeRoutine()
    {
        if (currentState == MoleState.Emerging) yield break;
        currentState = MoleState.Emerging;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (burrowEffect) burrowEffect.SetActive(false);
        if (visuals) visuals.SetActive(true);

        yield return new WaitForSeconds(emergeTime);

        if (moleCollider) moleCollider.enabled = true;
        currentState = MoleState.Attacking;
        nextFireTime = Time.time + 0.5f;

        yield return new WaitForSeconds(attackDuration);

        if (!isDead) StartCoroutine(SubmergeRoutine());
    }

    private void UpdateAttacking()
    {
        if (Time.time >= nextFireTime)
        {
            Spit();
            nextFireTime = Time.time + fireRate;
        }
    }

    private IEnumerator SubmergeRoutine()
    {
        if (currentState == MoleState.Submerging) yield break;
        currentState = MoleState.Submerging;

        if (moleCollider) moleCollider.enabled = false;

        yield return new WaitForSeconds(submergeTime);

        if (!isDead) EnterBurrowing();
    }

    private void LookAtTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 100f * Time.deltaTime);
        }
    }

    private void Spit()
    {
        if (projectilePrefab == null || firePoint == null) return;

        float currentSpitSpeed = 28f;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        Rigidbody rbProj = proj.GetComponent<Rigidbody>();

        if (rbProj != null)
        {
            float dist = Vector3.Distance(firePoint.position, target.position);
            float travelTime = dist / currentSpitSpeed;

            Vector3 playerVelocity = Vector3.zero;
            CharacterController cc = target.GetComponent<CharacterController>();
            if (cc != null) playerVelocity = cc.velocity;

            float gravityCompensation = dist * 0.12f;
            Vector3 predictedPos = target.position + (playerVelocity * travelTime) + Vector3.up * (1.2f + gravityCompensation);

            Vector3 spitDir = (predictedPos - firePoint.position).normalized;
            rbProj.linearVelocity = spitDir * currentSpitSpeed;
        }
        Destroy(proj, 5f);
    }
    public void takeDamage(float amount)
    {
        if (isDead) return;
        if (currentState == MoleState.Burrowing || currentState == MoleState.Positioning || currentState == MoleState.Submerging) return;

        health -= amount;
        if (health <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} погиб!");

        StopAllCoroutines();

        if (agent && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        if (visuals) visuals.SetActive(true);
        if (burrowEffect) burrowEffect.SetActive(false);

        Destroy(gameObject, 1.5f);
    }
}