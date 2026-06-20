using System.Collections;
using UnityEngine;

public class BatBehaviour : EnemyBehaviour
{
    private BatEnemy bat;
    private Vector3 targetMovePoint;
    private bool isCharging = false;
    private float nextFireTime = 0f;
    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;
    private float bobPhase;

    private const int VELOCITY_BUFFER_SIZE = 6;
    private Vector3[] velocityBuffer = new Vector3[VELOCITY_BUFFER_SIZE];
    private int velocityBufferIndex = 0;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        bat = (BatEnemy)enemy;

        bobPhase = Random.Range(0f, 100f);

        if (owner.target != null) lastPlayerPos = owner.target.position;

        PickNewRandomPoint();
    }

    public override void execute()
    {
        if (owner.target == null)
        {
            owner.FindTarget();
            if (owner.target == null) return;
        }

        if (owner == null || owner.HP <= 0) return;

        CalculatePlayerVelocity();

        float distToPlayer = Vector3.Distance(transform.position, owner.target.position);

        if (distToPlayer > bat.detectionRange)
        {
            MoveTowardsPoint(owner.target.position);
            MaintainHeight();
            return;
        }

        LookAtPlayer();

        if (!isCharging)
        {
            MoveTowardsPoint(targetMovePoint);

            float distToPointXZ = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(targetMovePoint.x, targetMovePoint.z));

            if (distToPointXZ < bat.stopDistance)
            {
                if (Time.time >= nextFireTime)
                    StartCoroutine(ChargeAndShootRoutine());
                else if (!IsInvoking(nameof(PickNewRandomPoint)))
                    Invoke(nameof(PickNewRandomPoint), Random.Range(0.3f, 0.8f));
            }
        }

        MaintainHeight();
    }

    private void MoveTowardsPoint(Vector3 point)
    {
        Vector3 currentPos = transform.position;
        Vector3 horizontalTarget = new Vector3(point.x, currentPos.y, point.z);
        transform.position = Vector3.MoveTowards(currentPos, horizontalTarget, bat.dashSpeed * Time.deltaTime);
    }

    private void CalculatePlayerVelocity()
    {
        if (Time.deltaTime <= 0) return;

        Vector3 rawVelocity = (owner.target.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = owner.target.position;

        if (rawVelocity.magnitude > 50f) rawVelocity = Vector3.zero;

        velocityBuffer[velocityBufferIndex] = rawVelocity;
        velocityBufferIndex = (velocityBufferIndex + 1) % VELOCITY_BUFFER_SIZE;

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < VELOCITY_BUFFER_SIZE; i++) sum += velocityBuffer[i];
        playerVelocity = sum / VELOCITY_BUFFER_SIZE;
    }

    private IEnumerator ChargeAndShootRoutine()
    {
        isCharging = true;

        float timer = 0;
        while (timer < bat.chargeTime)
        {
            LookAtPlayer();
            transform.position += Random.insideUnitSphere * 0.03f;
            timer += Time.deltaTime;

            float currentBob = Mathf.Sin(Time.time * bat.bobSpeed * 2f + bobPhase) * (bat.bobAmount * 0.5f);
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + currentBob * Time.deltaTime,
                transform.position.z);

            yield return null;
        }

        if (bat.projectilePrefab != null && bat.firePoint != null)
        {
            Vector3 targetAimPos = owner.target.position + Vector3.up * 1f;

            Vector3 predictedPos = targetAimPos;
            for (int i = 0; i < 3; i++)
            {
                float dist = Vector3.Distance(bat.firePoint.position, predictedPos);
                float travelTime = dist / bat.bulletSpeed;
                predictedPos = targetAimPos + playerVelocity * travelTime * bat.leadAccuracy;
            }

            Vector3 fireDir = (predictedPos - bat.firePoint.position).normalized;

            GameObject proj = Instantiate(bat.projectilePrefab, bat.firePoint.position, Quaternion.LookRotation(fireDir));
            BatProj projScript = proj.GetComponent<BatProj>();
            if (projScript != null)
                projScript.SetupDirect(fireDir, bat.bulletSpeed, bat.bulletDamage, owner.target);
        }

        isCharging = false;
        nextFireTime = Time.time + bat.fireRate;
        PickNewRandomPoint();
    }

    private void LookAtPlayer()
    {
        Vector3 targetPos = owner.target.position;

        if (isCharging)
        {
            float travelTime = Vector3.Distance(transform.position, owner.target.position) / bat.bulletSpeed;
            targetPos += playerVelocity * travelTime * bat.leadAccuracy;
        }

        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * bat.rotationSpeed);
        }
    }

    private void PickNewRandomPoint()
    {
        if (owner.target == null || isCharging) return;

        Vector3 toMe = transform.position - owner.target.position;
        toMe.y = 0;
        float currentAngle = (toMe.sqrMagnitude > 0.01f)
            ? Mathf.Atan2(toMe.z, toMe.x) * Mathf.Rad2Deg
            : Random.Range(0f, 360f);

        float newAngle = currentAngle + Random.Range(-100f, 100f);
        float rad = newAngle * Mathf.Deg2Rad;
        Vector2 randomDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float randomDist = Random.Range(bat.minRadius, bat.maxRadius);
        Vector3 offset = new Vector3(randomDir.x * randomDist, 0, randomDir.y * randomDist);
        targetMovePoint = owner.target.position + offset;
    }

    private void MaintainHeight()
    {
        RaycastHit hit;
        float targetY;
        if (Physics.SphereCast(transform.position + Vector3.up * 1f, 0.5f, Vector3.down, out hit, 30f, bat.groundMask))
        {
            targetY = hit.point.y + bat.hoverHeight;
        }
        else
        {
            targetY = owner.target.position.y + bat.hoverHeight;
        }

        float bobbingOffset = Mathf.Sin(Time.time * bat.bobSpeed + bobPhase) * bat.bobAmount;
        targetY += bobbingOffset;

        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * bat.heightAdjustSpeed);

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}