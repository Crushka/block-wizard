using System.Collections;
using UnityEngine;

public class BatBehaviour : EnemyBehaviour
{
    private BatEnemy bat;
    private Vector3 targetMovePoint;
    private bool isCharging = false;
    private float nextFireTime = 0f;
    private float yVelocity = 0f;
    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;
    private float bobPhase;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        bat = (BatEnemy)enemy;

        bobPhase = Random.Range(0f, 100f);

        PickNewRandomPoint();
    }

    public override void execute()
    {
        if (owner.target == null || bat.HP <= 0) return;

        CalculatePlayerVelocity();

        float distToPlayer = Vector3.Distance(transform.position, owner.target.position);
        if (distToPlayer > bat.detectionRange) return;

        LookAtPlayer();

        if (!isCharging)
        {
            Vector3 currentPos = transform.position;
            Vector3 horizontalTarget = new Vector3(targetMovePoint.x, currentPos.y, targetMovePoint.z);

            transform.position = Vector3.MoveTowards(currentPos, horizontalTarget, bat.dashSpeed * Time.deltaTime);

            float distToPointXZ = Vector2.Distance(new Vector2(currentPos.x, currentPos.z),
                                                   new Vector2(targetMovePoint.x, targetMovePoint.z));

            if (distToPointXZ < bat.stopDistance)
            {
                if (Time.time >= nextFireTime)
                {
                    StartCoroutine(ChargeAndShootRoutine());
                }
                else if (!IsInvoking(nameof(PickNewRandomPoint)))
                {
                    Invoke(nameof(PickNewRandomPoint), Random.Range(0.3f, 0.8f));
                }
            }
        }

        MaintainHeight();
    }

    private void CalculatePlayerVelocity()
    {
        playerVelocity = (owner.target.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = owner.target.position;

        if (playerVelocity.magnitude < 0.1f) playerVelocity = Vector3.zero;
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
            float frequencyMultiplier = 2f;
            float currentBob = Mathf.Sin(Time.time * bat.bobSpeed * frequencyMultiplier + bobPhase) * (bat.bobAmount * 0.5f);
            yield return null;
        }

        if (bat.projectilePrefab != null && bat.firePoint != null)
        {
            Vector3 targetBasePos = owner.target.position + Vector3.up * 1f;
            float distance = Vector3.Distance(bat.firePoint.position, targetBasePos);

            float travelTime = distance / bat.bulletSpeed;

            Vector3 predictedPos = targetBasePos + (playerVelocity * travelTime * bat.leadAccuracy);

            Vector3 fireDir = (predictedPos - bat.firePoint.position).normalized;

            GameObject proj = Instantiate(bat.projectilePrefab, bat.firePoint.position, Quaternion.LookRotation(fireDir));

            BatProj projScript = proj.GetComponent<BatProj>();
            if (projScript != null)
                projScript.SetupDirect(fireDir, bat.bulletSpeed, bat.bulletDamage);
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

        Vector2 randomDir = Random.insideUnitCircle.normalized;
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