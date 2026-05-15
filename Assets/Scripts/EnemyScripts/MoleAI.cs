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
        // Ищем игрока, только если цель не задана или это префаб
        if (target == null || target.gameObject.scene.name == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Если игрок пропал (например, убит), ищем его снова
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
        // Расширяем поиск точки на NavMesh
        if (NavMesh.SamplePosition(desiredPoint, out hit, ambushRadius * 2, NavMesh.AllAreas))
        {
            ambushPoint = hit.position;
            currentState = MoleState.Positioning;
            agent.SetDestination(ambushPoint);
        }
        else
        {
            // Если точку не нашли, попробуем еще раз в следующем кадре
            Debug.LogWarning($"{gameObject.name}: Не удалось найти точку на NavMesh!");
        }
    }

    private void UpdatePositioning()
    {
        // Если крот "застрял" или дошел до точки
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(EmergeRoutine());
        }
    }

    private IEnumerator EmergeRoutine()
    {
        // Защита от двойного запуска
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

        // Анимация ухода под землю здесь
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
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        Rigidbody rbProj = proj.GetComponent<Rigidbody>();
        if (rbProj != null)
        {
            Vector3 targetPos = target.position + Vector3.up * 1.2f;
            Vector3 spitDir = (targetPos - firePoint.position).normalized;
            rbProj.linearVelocity = spitDir * spitSpeed;
        }
        Destroy(proj, 5f);
    }

    public void takeDamage(float amount)
    {
        if (isDead) return;
        // Можно получать урон только когда не под землей
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

        // Показываем визуал, чтобы тело было видно при смерти
        if (visuals) visuals.SetActive(true);
        if (burrowEffect) burrowEffect.SetActive(false);

        Destroy(gameObject, 1.5f);
    }
}