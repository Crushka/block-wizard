using UnityEngine;
using System.Collections;

public class BeeBehaviour : EnemyBehaviour
{
    public bool isAttacking = false;
    public float nextAttackAvailiableTime = 5f;

    public bool IsAttacking => isAttacking;
    private BeeEnemy bee;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        bee = (BeeEnemy)enemy;
    } 

    public override void execute()
    {
        if (BossQueen.MinionsFrozen)
        {
            if (bee.rb != null) bee.rb.linearVelocity = Vector3.zero;
            return;
        }

        if (isAttacking || owner.target == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, owner.target.position);

        if(distance > bee.detectionRange)
        {
            return;
        }

        LookAtPlayer();
        if (!BossQueen.MinionsFrozen)
        {
            if (distance <= bee.attackRange && Time.time >= nextAttackAvailiableTime)
            {
                StartCoroutine(StingAttack());
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 playerPosXZ = new Vector3(owner.target.position.x, 0, owner.target.position.z);
        Vector3 myPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDist = Vector3.Distance(playerPosXZ, myPosXZ);

        if (horizontalDist > bee.minHorizontalDistance)
        {
            Vector3 dir = (owner.target.position - transform.position);
            dir.y = 0;
            transform.position += dir.normalized * bee.moveSpeed * Time.deltaTime;
        }

        MaintainHeight();
    }

    void MaintainHeight()
    {
        RaycastHit hit;
        float targetY;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 30f, bee.groundMask))
        {
            targetY = hit.point.y + bee.hoverHeight;
        }
        else
        {
            targetY = owner.target.position.y + bee.hoverHeight;
        }

        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * bee.heightAdjustSpeed);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void LookAtPlayer()
    {
        Vector3 direction = (owner.target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bee.rotationSpeed);
        }
    }

    IEnumerator StingAttack()
    {
        isAttacking = true;

        if (bee != null)
        {
             bee.ResetDamageFlag();
        }

        float elapsed = 0;

        while (elapsed < bee.windUpTime)
        {
            Vector3 targetPoint = owner.target.position + Vector3.up * 1.0f;
            Vector3 lookDir = (targetPoint - transform.position).normalized;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * bee.rotationSpeed * 2f);
            }
            transform.position += Random.insideUnitSphere * 0.03f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalTargetPoint = owner.target.position + Vector3.up * 1.0f;
        Vector3 dashDirection = (finalTargetPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dashDirection);

        float dashTimer = 0;
        while (dashTimer < bee.dashDuration)
        {
            bee.rb.linearVelocity = dashDirection * bee.dashSpeed;
            dashTimer += Time.deltaTime;
            yield return null;
        }
        StopDash();
    }

    public void StopDash()
    {
        bee.rb.linearVelocity = Vector3.zero;
        isAttacking = false;
        nextAttackAvailiableTime = Time.time + bee.attackCooldown;
    }
}
