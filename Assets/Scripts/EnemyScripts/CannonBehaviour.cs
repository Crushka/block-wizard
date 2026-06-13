using System.Collections;
using UnityEngine;

public class CannonBehaviour : EnemyBehaviour
{
    private CannonEnemy cannon;
    private bool isWorking = false;
    private float nextFireTime = 0f;

    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        cannon = (CannonEnemy)enemy;
        if (owner.target != null) lastPlayerPos = owner.target.position;
    }

    public override void execute()
    {
        if (owner.target == null) return;

        CalculatePlayerVelocity();

        float dist = Vector3.Distance(transform.position, owner.target.position);

        if (dist <= cannon.activationRange)
        {
            // ПОВОРАЧИВАЕМ ВСЕГДА
            AimFirePoint();

            if (IsPlayerInVisionCone() && !isWorking && Time.time >= nextFireTime)
            {
                StartCoroutine(AutomaticFireRoutine());
            }
        }
    }

    private void AimFirePoint()
    {
        if (cannon.firePoint == null) return;

        Vector3 targetCenter = owner.target.position;
        float dist = Vector3.Distance(cannon.firePoint.position, targetCenter);
        float travelTime = dist / cannon.projectileSpeed;
        Vector3 predictedPos = targetCenter + (playerVelocity * travelTime * cannon.leadAccuracy);

        Vector3 dir = (predictedPos - cannon.firePoint.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            // ШАГ 2: Вращаем
            cannon.firePoint.rotation = Quaternion.Slerp(cannon.firePoint.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private void CalculatePlayerVelocity()
    {
        playerVelocity = (owner.target.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = owner.target.position;
        if (playerVelocity.magnitude > 50f) playerVelocity = Vector3.zero;
    }

    private IEnumerator AutomaticFireRoutine()
    {
        isWorking = true;

        // Ждем зарядку (Я УДАЛИЛ ТРЯСКУ, чтобы кольцо не смещалось)
        yield return new WaitForSeconds(cannon.chargeTime);

        FireProjectile();

        isWorking = false;
        nextFireTime = Time.time + cannon.fireRate;
    }

    private bool IsPlayerInVisionCone()
    {
        Vector3 dirToPlayer = (owner.target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        return angle <= (cannon.viewAngle * 0.5f);
    }

    private void FireProjectile()
    {
        if (cannon.projectilePrefab == null || cannon.firePoint == null) return;

        // Спавним снаряд точно в позиции и с поворотом нашего firePoint
        GameObject proj = Instantiate(cannon.projectilePrefab, cannon.firePoint.position, cannon.firePoint.rotation);

        BatProj projScript = proj.GetComponent<BatProj>();
        if (projScript != null)
        {
            // Снаряд полетит строго по синей стрелке (Z) нашего firePoint
            projScript.SetupDirect(cannon.firePoint.forward, cannon.projectileSpeed, cannon.damage);
        }
    }
}