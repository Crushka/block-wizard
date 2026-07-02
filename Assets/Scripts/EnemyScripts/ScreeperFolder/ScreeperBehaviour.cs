using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ScreeperBehaviour : EnemyBehaviour
{
    private enum State { Idle, Stalking, Action, Escaping }
    [SerializeField] private State currentState = State.Idle;

    private ScreeperEnemy screeper;
    private float nextAttackTime = 0f;
    private Vector3 escapeTarget;
    private float escapeStartTime;
    public float minComfortDistance = 5f;
    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;

    private const int VELOCITY_BUFFER_SIZE = 6;
    private Vector3[] velocityBuffer = new Vector3[VELOCITY_BUFFER_SIZE];
    private int velocityBufferIndex = 0;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        screeper = (ScreeperEnemy)enemy;
        currentState = State.Idle;
        if (owner.target != null) lastPlayerPos = owner.target.position;

        if (screeper.agent != null)
        {
            screeper.agent.stoppingDistance = 0.5f;
            screeper.agent.acceleration = 500f;
            screeper.agent.angularSpeed = 1000f;
            screeper.agent.updateRotation = false;
        }
    }

    public override void execute()
    {
        if (owner.target == null)
        {
            owner.FindTarget();
            if (owner.target == null) return;
        }

        if (screeper.HP <= 0) return;

        CalculatePlayerVelocity();

        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Stalking:
                HandleStalking();
                break;
            case State.Escaping:
                HandleEscaping();
                break;
        }
    }

    private void HandleIdle()
    {
        if (screeper.agent.isActiveAndEnabled && !screeper.agent.isStopped)
        {
            screeper.agent.isStopped = true;
            screeper.agent.ResetPath();
            screeper.agent.velocity = Vector3.zero;
        }

        float dist = Vector3.Distance(transform.position, owner.target.position);
        if (dist <= screeper.detectionRange)
        {
            currentState = State.Stalking;
        }
    }

    private void HandleStalking()
    {
        float dist = Vector3.Distance(transform.position, owner.target.position);
        RotateTowardsPredictedPos();

        bool hasLOS = HasLineOfSight();
        bool readyToShoot = Time.time >= nextAttackTime;

        if (dist < minComfortDistance)
        {
            PickEscapePointStrict();
            currentState = State.Escaping;
            return;
        }

        if (dist > screeper.detectionRange * 1.3f)
        {
            if (screeper.agent.isActiveAndEnabled)
            {
                screeper.agent.isStopped = true;
                screeper.agent.ResetPath();
                screeper.agent.velocity = Vector3.zero;
            }
            currentState = State.Idle;
            return;
        }

        if (readyToShoot && dist <= screeper.shootingRange && hasLOS)
        {
            StartCoroutine(AttackSequence());
            return;
        }

        if (screeper.agent.isActiveAndEnabled)
        {
            if (hasLOS && dist <= (screeper.shootingRange * 0.95f))
            {
                if (!screeper.agent.isStopped)
                {
                    screeper.agent.isStopped = true;
                    screeper.agent.ResetPath();
                    screeper.agent.velocity = Vector3.zero;
                }
            }
            else
            {
                screeper.agent.isStopped = false;
                screeper.agent.stoppingDistance = minComfortDistance;
                screeper.agent.SetDestination(owner.target.position);
            }
        }
    }

    private void HandleEscaping()
    {
        if (!screeper.agent.isActiveAndEnabled) return;

        screeper.agent.isStopped = false;
        screeper.agent.speed = screeper.speed * 2.0f;
        screeper.agent.SetDestination(escapeTarget);

        Vector3 moveDir = (escapeTarget - transform.position).normalized;
        moveDir.y = 0;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(moveDir), Time.deltaTime * 40f);
        }

        if (!screeper.agent.pathPending && screeper.agent.remainingDistance < 1.2f)
        {
            ResetToStalking();
            return;
        }

        if (Time.time - escapeStartTime > 3f)
        {
            ResetToStalking();
        }
    }

    private void PickEscapePointStrict()
    {
        Vector3 dirAwayFromPlayer = (transform.position - owner.target.position).normalized;
        dirAwayFromPlayer.y = 0;
        Vector3 rawTarget = transform.position + dirAwayFromPlayer * screeper.hideDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTarget, out hit, 15f, NavMesh.AllAreas))
            escapeTarget = hit.position;
        else
            escapeTarget = transform.position + Quaternion.Euler(0, 90, 0) * dirAwayFromPlayer * 20f;

        screeper.agent.stoppingDistance = 0.3f;
        escapeStartTime = Time.time;
    }

    private IEnumerator AttackSequence()
    {
        currentState = State.Action;
        screeper.agent.isStopped = true;
        screeper.agent.velocity = Vector3.zero;
        screeper.agent.ResetPath();

        yield return new WaitForSeconds(0.15f);

        // Проверка перед выстрелом
        if (owner.target == null)
        {
            ResetToStalking();
            yield break;
        }

        FireProjectileWithLead();
        screeper.PlayLaughSound();

        float laughTimer = 0;
        Vector3 startPos = transform.position;
        while (laughTimer < screeper.laughDuration)
        {
            // Проверка на каждом кадре — цель могла быть уничтожена во время смеха
            if (owner.target == null)
            {
                transform.position = startPos;
                ResetToStalking();
                yield break;
            }

            transform.position = startPos + Random.insideUnitSphere * 0.08f;
            RotateTowardsPredictedPos();
            laughTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;

        // Проверка перед выбором точки побега
        if (owner.target == null)
        {
            ResetToStalking();
            yield break;
        }

        PickEscapePointStrict();
        currentState = State.Escaping;
    }

    private void CalculatePlayerVelocity()
    {
        if (owner.target == null || Time.deltaTime <= 0) return;

        Vector3 rawVelocity = (owner.target.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = owner.target.position;

        if (rawVelocity.magnitude > 50f) rawVelocity = Vector3.zero;

        velocityBuffer[velocityBufferIndex] = rawVelocity;
        velocityBufferIndex = (velocityBufferIndex + 1) % VELOCITY_BUFFER_SIZE;

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < VELOCITY_BUFFER_SIZE; i++) sum += velocityBuffer[i];
        playerVelocity = sum / VELOCITY_BUFFER_SIZE;
    }

    private void RotateTowardsPredictedPos()
    {
        if (owner.target == null) return;

        Vector3 targetAimPos = owner.target.position;

        Vector3 predictedPos = targetAimPos;
        for (int i = 0; i < 3; i++)
        {
            float dist = Vector3.Distance(screeper.firePoint.position, predictedPos);
            float travelTime = dist / screeper.bulletSpeed;
            predictedPos = targetAimPos + playerVelocity * travelTime * screeper.leadAccuracy;
        }

        Vector3 dir = (predictedPos - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
        }
    }

    private bool HasLineOfSight()
    {
        if (screeper.firePoint == null || owner.target == null) return false;
        RaycastHit hit;
        Vector3 origin = screeper.firePoint.position;
        Vector3 targetPos = owner.target.position;
        Vector3 dir = (targetPos - origin).normalized;
        int layerMask = ~LayerMask.GetMask("Enemy");
        if (Physics.Raycast(origin, dir, out hit, screeper.shootingRange, layerMask))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("PlayerBody"))
                return true;
        }
        return false;
    }

    private void FireProjectileWithLead()
    {
        if (screeper.projectilePrefab == null || screeper.firePoint == null || owner.target == null) return;

        Vector3 targetAimPos = owner.target.position;

        Vector3 predictedPos = targetAimPos;
        for (int i = 0; i < 3; i++)
        {
            float dist = Vector3.Distance(screeper.firePoint.position, predictedPos);
            float travelTime = dist / screeper.bulletSpeed;
            predictedPos = targetAimPos + playerVelocity * travelTime * screeper.leadAccuracy;
        }

        Vector3 fireDir = (predictedPos - screeper.firePoint.position).normalized;

        GameObject proj = Instantiate(screeper.projectilePrefab, screeper.firePoint.position, Quaternion.LookRotation(fireDir));
        BatProj p = proj.GetComponent<BatProj>();

        if (p != null)
        {
            p.SetupDirect(fireDir, screeper.bulletSpeed, screeper.bulletDamage, owner.target);
        }
    }

    private void ResetToStalking()
    {
        screeper.agent.speed = screeper.speed;
        nextAttackTime = Time.time + 3f;
        currentState = State.Stalking;
    }
}