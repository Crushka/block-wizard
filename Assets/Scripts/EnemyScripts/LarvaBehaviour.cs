using System.Collections;
using UnityEngine;

public class LarvaBehaviour : EnemyBehaviour
{
    private LarvaEnemy larva;
    private bool isAttacking = false;
    private bool isPulsing = false;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        larva = (LarvaEnemy)enemy;
    }

    public override void execute()
    {
        if (owner.target == null || larva.HP <= 0 || isAttacking) return;

        if (!isPulsing)
        {
            StartCoroutine(CrescendoMovementRoutine());
        }
    }

    private IEnumerator CrescendoMovementRoutine()
    {
        isPulsing = true;

        float restTimer = 0;
        while (restTimer < larva.restDuration)
        {
            if (TryCheckAttack()) yield break;

            larva.agent.speed = 0;
            restTimer += Time.deltaTime;
            yield return null;
        }

        float elapsed = 0;
        while (elapsed < larva.pulseDuration)
        {
            if (TryCheckAttack()) yield break;

            if (larva.agent.isActiveAndEnabled)
                larva.agent.SetDestination(owner.target.position);

            float t = elapsed / larva.pulseDuration;
            float curveStep = Mathf.Sin(t * Mathf.PI);

            larva.agent.speed = curveStep * larva.maxPulseSpeed;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isPulsing = false;
    }

    private bool TryCheckAttack()
    {
        if (owner.target == null) return false;

        float distance = Vector3.Distance(transform.position, owner.target.position);
        if (distance <= larva.attackRange)
        {
            isPulsing = false;
            StopAllCoroutines();
            StartCoroutine(LarvaStingRoutine());
            return true;
        }
        return false;
    }

    private IEnumerator LarvaStingRoutine()
    {
        isAttacking = true;
        isPulsing = false;

        if (larva.agent.isActiveAndEnabled)
        {
            larva.agent.isStopped = true;
            larva.agent.speed = 0;
        }

        float turnTimer = 0;
        while (turnTimer < 0.5f)
        {
            if (owner.target == null) yield break;
            Vector3 dirToPlayer = (owner.target.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(-dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * larva.turnBeforeAttackSpeed);
            turnTimer += Time.deltaTime;
            yield return null;
        }

        Vector3 dashDir = transform.forward;
        larva.agent.enabled = false;
        larva.rb.isKinematic = false;

        larva.rb.AddForce(dashDir * larva.dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.25f);
        CheckStingHit();

        yield return new WaitForSeconds(0.1f);
        larva.Die();
    }

    private void CheckStingHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.6f, 1.3f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("PlayerBody"))
            {
                hit.GetComponentInParent<IDamageable>()?.takeDamage(larva.dashDamage);
                Debug.Log("Зародыш успешно ужалил!");
                break;
            }
        }
    }
}