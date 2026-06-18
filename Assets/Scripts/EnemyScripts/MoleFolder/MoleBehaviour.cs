using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoleBehaviour : EnemyBehaviour
{
    public enum MoleState { Burrowing, Positioning, Emerging, Attacking, Submerging }
    private MoleState currentState = MoleState.Burrowing;

    private MoleEnemy mole;
    private float currentBurrowTimer = 0f;
    private float nextFireTime = 0f;
    private Vector3 ambushPoint;
    public bool CanBeHit => (currentState == MoleState.Attacking || currentState == MoleState.Emerging);

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        mole = (MoleEnemy)enemy;
        EnterBurrowing();
    }

    public override void execute()
    {
        if (owner.target == null || mole.HP <= 0) return;

        float distance = Vector3.Distance(transform.position, owner.target.position);

        if (distance > mole.detectionRange)
        {
            HandleOutOfRange();
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

    private void HandleOutOfRange()
    {
        if (currentState == MoleState.Attacking)
        {
            StopAllCoroutines();
            StartCoroutine(SubmergeRoutine());
        }
        if (mole.agent && mole.agent.isActiveAndEnabled)
        {
            mole.agent.isStopped = true;
        }
    }

    private void EnterBurrowing()
    {
        currentState = MoleState.Burrowing;
        currentBurrowTimer = mole.minBurrowTime;

        if (mole.agent && mole.agent.isActiveAndEnabled)
        {
            mole.agent.isStopped = false;
            mole.agent.speed = mole.burrowSpeed;
        }

        if (mole.visuals) mole.visuals.SetActive(false);
        if (mole.burrowEffect) mole.burrowEffect.SetActive(true);
        if (mole.moleCollider) mole.moleCollider.enabled = false;
    }

    private void UpdateBurrowing()
    {
        mole.agent.SetDestination(owner.target.position);

        if (currentBurrowTimer > 0)
        {
            currentBurrowTimer -= Time.deltaTime;
            return;
        }

        float dist = Vector3.Distance(transform.position, owner.target.position);
        if (dist <= mole.triggerDistance)
        {
            SetAmbushPoint();
        }
    }

    private void SetAmbushPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * mole.ambushRadius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 desiredPoint = owner.target.position + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPoint, out hit, mole.ambushRadius * 2, NavMesh.AllAreas))
        {
            ambushPoint = hit.position;
            currentState = MoleState.Positioning;
            mole.agent.SetDestination(ambushPoint);
        }
    }

    private void UpdatePositioning()
    {
        if (!mole.agent.pathPending && mole.agent.remainingDistance <= mole.agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(EmergeRoutine());
        }
    }

    private IEnumerator EmergeRoutine()
    {
        if (currentState == MoleState.Emerging) yield break;
        currentState = MoleState.Emerging;

        mole.agent.isStopped = true;
        mole.agent.velocity = Vector3.zero;

        if (mole.burrowEffect) mole.burrowEffect.SetActive(false);
        if (mole.visuals) mole.visuals.SetActive(true);

        yield return new WaitForSeconds(mole.emergeTime);

        currentState = MoleState.Attacking;
        nextFireTime = Time.time + 0.5f;

        yield return new WaitForSeconds(mole.attackDuration);

        StartCoroutine(SubmergeRoutine());
    }

    private void UpdateAttacking()
    {
        if (Time.time >= nextFireTime)
        {
            Spit();
            nextFireTime = Time.time + mole.fireRate;
        }
    }

    private IEnumerator SubmergeRoutine()
    {
        currentState = MoleState.Submerging;
        yield return new WaitForSeconds(mole.submergeTime);
        EnterBurrowing();
    }

    private void LookAtTarget()
    {
        Vector3 direction = (owner.target.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, mole.rotationSpeed * 100f * Time.deltaTime);
        }
    }

    private void Spit()
    {
        if (mole.projectilePrefab == null || mole.firePoint == null) return;

        float currentSpitSpeed = 28f;
        GameObject proj = Instantiate(mole.projectilePrefab, mole.firePoint.position, transform.rotation);
        Rigidbody rbProj = proj.GetComponent<Rigidbody>();

        if (rbProj != null)
        {
            float dist = Vector3.Distance(mole.firePoint.position, owner.target.position);
            float travelTime = dist / currentSpitSpeed;

            Vector3 playerVelocity = Vector3.zero;
            CharacterController cc = owner.target.GetComponent<CharacterController>();
            if (cc != null) playerVelocity = cc.velocity;

            float gravityCompensation = dist * 0.12f;
            Vector3 predictedPos = owner.target.position + (playerVelocity * travelTime) + Vector3.up * (1.2f + gravityCompensation);

            Vector3 spitDir = (predictedPos - mole.firePoint.position).normalized;
            rbProj.linearVelocity = spitDir * currentSpitSpeed;
        }
    }
}