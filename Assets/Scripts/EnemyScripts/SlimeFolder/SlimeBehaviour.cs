using System.Collections;
using UnityEngine;

public class SlimeBehaviour : EnemyBehaviour
{
    private SlimeEnemy slime;
    private bool isJumping = false;
    private bool hasDealtDamageInCurrentJump = false;
    private bool isAttackJump = false;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        slime = (SlimeEnemy)enemy;
        StopAllCoroutines();
        StartCoroutine(JumpCycle());
    }

    public override void execute() { }

    private IEnumerator JumpCycle()
    {
        while (slime != null && slime.HP > 0)
        {
            if (owner.target == null) { yield return new WaitForSeconds(1f); continue; }

            float distance = Vector3.Distance(transform.position, owner.target.position);

            if (distance <= slime.lookRadius && !isJumping)
            {
                yield return StartCoroutine(RotateToTarget());

                isAttackJump = (distance <= slime.attackRange);
                Vector3 targetPos = isAttackJump
                    ? owner.target.position
                    : transform.position + (owner.target.position - transform.position).normalized * 4f;

                yield return StartCoroutine(PerformDynamicJump(targetPos));
                yield return new WaitForSeconds(slime.jumpInterval);
            }
            yield return null;
        }
    }

    private IEnumerator PerformDynamicJump(Vector3 targetPoint)
    {
        isJumping = true;
        hasDealtDamageInCurrentJump = false;

        Vector3 startPos = transform.position;
        Vector3 diff = targetPoint - startPos;
        Vector3 diffXZ = new Vector3(diff.x, 0, diff.z);

        float gravity = Mathf.Abs(Physics.gravity.y) * slime.gravityMultiplier;
        float h = slime.jumpHeight;

        float vY = Mathf.Sqrt(2 * gravity * h);
        float timeUp = vY / gravity;
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(0.01f, h - diff.y) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 vXZ = diffXZ / totalTime;
        slime.rb.linearVelocity = vXZ + Vector3.up * vY;

        yield return new WaitForSeconds(0.1f);

        float timeoutTimer = 0;
        while (!IsGrounded() && timeoutTimer < 3f)
        {
            slime.rb.AddForce(Vector3.down * gravity * slime.rb.mass);
            timeoutTimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (isAttackJump && !hasDealtDamageInCurrentJump)
        {
            ApplyAreaDamage();
        }

        slime.rb.linearVelocity = Vector3.zero;
        isJumping = false;
    }

    public void OnJumpCollision(GameObject hitObject)
    {
        if (!isJumping || hasDealtDamageInCurrentJump) return;
            
        if (hitObject.CompareTag("Player") || hitObject.CompareTag("PlayerBody"))
        {
            DealDamageToTarget(hitObject);
        }
    }

    private void DealDamageToTarget(GameObject target)
    {
        if (hasDealtDamageInCurrentJump) return;

        IDamageable d = target.GetComponentInParent<IDamageable>();
        if (d != null)
        {
            d.takeDamage(slime.damage);
            hasDealtDamageInCurrentJump = true;

            PlayerController pc = target.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                Vector3 pushDir = (target.transform.position - transform.position);
                pushDir.y = 0;
                pushDir.Normalize();

                pc.AddKnockback(pushDir, slime.knockbackForce);

                Vector3 bounceDir = -pushDir + Vector3.up * 0.5f;
                slime.rb.linearVelocity = Vector3.zero;
                slime.rb.AddForce(bounceDir * 8f, ForceMode.Impulse);

                Debug.Log($"СЛАЙМ: Удар силой {slime.knockbackForce}");
            }
        }
    }

    private IEnumerator RotateToTarget()
    {
        float timer = 0;
        while (timer < 0.1f)
        {
            if (owner.target == null) yield break;
            Vector3 dir = (owner.target.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 20f);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void ApplyAreaDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slime.damageRadius);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Player") || col.CompareTag("PlayerBody"))
            {
                DealDamageToTarget(col.gameObject);
                break;
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.4f, slime.groundMask);
    }
}