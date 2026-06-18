using System.Collections;
using UnityEngine;

public class QueenCombatBehaviour : EnemyBehaviour
{
    private BossQueen queen;
    private bool isExecutingAction = false;

    [Header("Тайминги лазера")]
    public float laserCooldown = 15f;
    private float nextLaserTime = 0f;
    private int attacksCount = 0;

    public override void Init(Enemy enemy)
    {
        base.Init(enemy);
        queen = (BossQueen)enemy;
        nextLaserTime = Time.time + laserCooldown;
    }

    public void StartCombat()
    {
        // Очищаем старые рутины, если они были
        StopAllCoroutines();
        StartCoroutine(BossBrainRoutine());
    }

    public override void execute()
    {
        if (!enabled || queen == null || queen.HP <= 0) return;

        // Королева больше НЕ вызывает MaintainDistance. 
        // Она просто стоит на месте и поворачивается к игроку.
        if (!isExecutingAction)
        {
            RotateTowardsPlayer();
        }
    }

    private IEnumerator BossBrainRoutine()
    {
        while (queen.HP > 0)
        {
            yield return new WaitForSeconds(Random.Range(2f, 3f));

            if (attacksCount >= 2 && Time.time >= nextLaserTime)
            {
                yield return StartCoroutine(LaserAttackRoutine());
                attacksCount = 0;
                nextLaserTime = Time.time + laserCooldown;
            }
            else
            {
                float choice = Random.value;
                if (choice < 0.6f)
                    yield return StartCoroutine(MachineGunAttack());
                else
                    yield return StartCoroutine(SummonAttack());

                attacksCount++;
            }
        }
    }

    private IEnumerator LaserAttackRoutine()
    {
        isExecutingAction = true;
        BossQueen.MinionsFrozen = true;
        Debug.Log("ЖНЕЦ: Подготовка главного калибра...");

        // 1. ВЗЛЕТ
        Vector3 startPos = transform.position;
        Vector3 airPos = startPos + Vector3.up * queen.laserFlyHeight;
        while (Vector3.Distance(transform.position, airPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, airPos, 10f * Time.deltaTime);
            RotateTowardsPlayer();
            yield return null;
        }

        // 2. ФОРМИРОВАНИЕ НАЧАЛЬНОЙ ТОЧКИ (Как в Mass Effect)
        // Лазер ударит ПЕРЕД игроком (на основе его взгляда или просто вектора от королевы)
        Vector3 playerForward = owner.target.forward;
        playerForward.y = 0;

        // Точка удара: Позиция игрока + смещение вперед
        Vector3 laserPoint = owner.target.position + (playerForward * queen.laserStartOffset);

        if (queen.laserLine) queen.laserLine.enabled = true;

        float attackTimer = 0;
        float duration = 6f;

        while (attackTimer < duration)
        {
            // 1. Двигаем "целевую точку" за игроком (горизонтально)
            laserPoint = Vector3.MoveTowards(laserPoint, owner.target.position, queen.laserTrackingSpeed * Time.deltaTime);

            // 2. Ищем реальный пол под этой точкой с помощью Raycast
            RaycastHit hit;
            Vector3 finalLaserEndpoint = laserPoint; // По умолчанию точка игрока

            // Пускаем луч чуть выше лазерной точки строго вниз, чтобы найти землю
            // Используем groundMask, чтобы лазер не спотыкался о самих пчел
            if (Physics.Raycast(laserPoint + Vector3.up * 5f, Vector3.down, out hit, 20f, queen.groundMask))
            {
                finalLaserEndpoint = hit.point;
            }

            // 3. Отрисовываем лазер
            if (queen.laserLine)
            {
                queen.laserLine.SetPosition(0, queen.stingerMuzzle.position);
                queen.laserLine.SetPosition(1, finalLaserEndpoint); // Теперь точно в землю!
            }

            // 4. Урон (проверяем дистанцию между игроком и точкой попадания в землю)
            if (Vector3.Distance(owner.target.position, finalLaserEndpoint) < 2.0f)
            {
                owner.target.GetComponentInParent<IDamageable>()?.takeDamage(queen.laserDamage * Time.deltaTime * 60f);
            }

            // Поворот корпуса за точкой на земле
            Vector3 dirToPoint = (finalLaserEndpoint - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dirToPoint.x, 0, dirToPoint.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * Quaternion.Euler(0, -90, 0), Time.deltaTime * 5f);

            attackTimer += Time.deltaTime;
            yield return null;
        }

        // 4. ЗАВЕРШЕНИЕ
        if (queen.laserLine) queen.laserLine.enabled = false;
        BossQueen.MinionsFrozen = false;

        // ВОЗВРАТ НА ЗЕМЛЮ
        while (Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, 10f * Time.deltaTime);
            RotateTowardsPlayer();
            yield return null;
        }

        isExecutingAction = false;
    }

    private void RotateTowardsPlayer()
    {
        if (owner.target == null) return;
        Vector3 dir = (owner.target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * Quaternion.Euler(0, -90, 0), Time.deltaTime * 5f);
    }

    private IEnumerator MachineGunAttack()
    {
        isExecutingAction = true;
        float timer = 0;
        while (timer < 4f)
        {
            // Поворот на игрока
            Vector3 dirToPlayer = (owner.target.position - transform.position).normalized;
            Vector3 flatDir = new Vector3(dirToPlayer.x, 0, dirToPlayer.z);
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDir);
                Quaternion offset = Quaternion.Euler(0, -90, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * offset, Time.deltaTime * 5f);
            }

            // Выстрел
            if (queen.bulletPrefab && queen.stingerMuzzle)
            {
                Vector3 fireDir = (owner.target.position + Vector3.up * 1f - queen.stingerMuzzle.position).normalized;
                GameObject bullet = Instantiate(queen.bulletPrefab, queen.stingerMuzzle.position, Quaternion.LookRotation(fireDir));

                var shoot = bullet.GetComponent<QueenShoot>();
                if (shoot != null) shoot.Setup(queen.bulletDamage, queen.bulletSpeed, 3f, 5f);
            }

            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        isExecutingAction = false;
    }

    private IEnumerator SummonAttack()
    {
        isExecutingAction = true;
        for (int i = 0; i < queen.minionsToSummon; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = Mathf.Max(spawnPos.y, 2f);
            Instantiate(queen.minionPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
        isExecutingAction = false;
    }
}