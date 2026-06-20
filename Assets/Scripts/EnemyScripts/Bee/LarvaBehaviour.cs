//using System.Collections;
//using UnityEngine;

//public class LarvaBehaviour : EnemyBehaviour
//{
//    private LarvaEnemy larva;
//    private bool isAttacking = false;
//    private bool isPulsing = false;

//    public override void Init(Enemy enemy)
//    {
//        base.Init(enemy);
//        larva = (LarvaEnemy)enemy;
//    }

//    public override void execute()
//    {
//        if (owner.target == null)
//        {
//            owner.FindTarget();
//            if (owner.target == null) return;
//        }

//        if (BossQueen.MinionsFrozen || isAttacking || owner.target == null || larva.HP <= 0) return;

//        float distance = Vector3.Distance(transform.position, owner.target.position);

//        if (distance <= larva.attackRange)
//        {
//            isPulsing = false;
//            StopAllCoroutines();
//            StartCoroutine(LarvaStingRoutine());
//        }
//        else if (!isPulsing)
//        {
//            StartCoroutine(CrescendoMovementRoutine());
//        }
//        if (!BossQueen.MinionsFrozen)
//        {
//            if (distance <= larva.attackRange)
//            {
//                StartCoroutine(LarvaStingRoutine());
//            }
//            else
//            {
//                StartCoroutine(CrescendoMovementRoutine());
//            }
//        }
//    }

//    private IEnumerator CrescendoMovementRoutine()
//    {
//        isPulsing = true;
//        larva.agent.speed = 0;
//        yield return new WaitForSeconds(larva.restDuration);

//        float elapsed = 0;
//        while (elapsed < larva.pulseDuration)
//        {
//            if (isAttacking) yield break;

//            // Игрок мог исчезнуть прямо во время разгона (1 секунда — долгий цикл),
//            // без этой проверки следующая строка упадёт с исключением
//            if (owner.target == null) { isPulsing = false; yield break; }

//            if (larva.agent.isActiveAndEnabled)
//                larva.agent.SetDestination(owner.target.position);

//            float t = elapsed / larva.pulseDuration;
//            float curveStep = Mathf.Sin(t * Mathf.PI);
//            larva.agent.speed = curveStep * larva.maxPulseSpeed;

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        isPulsing = false;
//    }

//    private IEnumerator LarvaStingRoutine()
//    {
//        isAttacking = true;

//        // 1. Останавливаем и отключаем NavMesh, чтобы он не мешал физике
//        if (larva.agent.isActiveAndEnabled)
//        {
//            larva.agent.isStopped = true;
//            larva.agent.enabled = false;
//        }

//        // 2. Включаем физику (Rigidbody)
//        larva.rb.isKinematic = false;

//        // Запоминаем вектор К игроку, чтобы не потерять его после разворота
//        Vector3 directionToPlayer = (owner.target.position - transform.position).normalized;
//        directionToPlayer.y = 0;

//        // 3. ПОДГОТОВКА: Плавный разворот задом (жалом) к игроку
//        float turnTimer = 0;
//        while (turnTimer < 0.5f)
//        {
//            if (owner.target == null) yield break;

//            // Направление "ОТ игрока"
//            Quaternion lookAwayRot = Quaternion.LookRotation(-directionToPlayer);
//            transform.rotation = Quaternion.Slerp(transform.rotation, lookAwayRot, Time.deltaTime * larva.turnBeforeAttackSpeed);

//            turnTimer += Time.deltaTime;
//            yield return null;
//        }

//        // 4. АТАКА (Рывок задом)
//        larva.isDashingNow = true;

//        // ВАЖНО: Мы прикладываем силу в сторону ИГРОКА (directionToPlayer), 
//        // хотя сами смотрим в другую сторону. Это и создаст рывок задом.
//        Vector3 stingDashDir = directionToPlayer + Vector3.up * 0.1f; // Чуть-чуть вверх для уменьшения трения

//        larva.rb.AddForce(stingDashDir * larva.dashForce, ForceMode.Impulse);

//        // Даем время на полет (0.6 сек хватит, чтобы врезаться)
//        yield return new WaitForSeconds(0.6f);

//        // Если ни в кого не попали — просто умираем (пчела без жала не живет)
//        larva.Die();
//    }
//}

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
        if (owner.target == null)
        {
            owner.FindTarget();
            if (owner.target == null) return;
        }

        // Единственная точка выхода — больше нет дублирующего блока ниже
        if (BossQueen.MinionsFrozen || isAttacking || owner.target == null || larva.HP <= 0) return;

        float distance = Vector3.Distance(transform.position, owner.target.position);

        if (distance <= larva.attackRange)
        {
            // Прерываем движение и начинаем атаку
            if (isPulsing)
            {
                isPulsing = false;
                StopAllCoroutines();
            }
            StartCoroutine(LarvaStingRoutine());
        }
        else if (!isPulsing)
        {
            // Запускаем движение только если оно ещё не идёт
            StartCoroutine(CrescendoMovementRoutine());
        }
    }

    private IEnumerator CrescendoMovementRoutine()
    {
        isPulsing = true;

        larva.agent.speed = 0;
        yield return new WaitForSeconds(larva.restDuration);

        float elapsed = 0;
        while (elapsed < larva.pulseDuration)
        {
            // Прерываемся если началась атака или цель пропала
            if (isAttacking || owner.target == null)
            {
                isPulsing = false;
                larva.agent.speed = 0;
                yield break;
            }

            // Прерываемся если игрок вошёл в зону атаки прямо во время разгона
            float dist = Vector3.Distance(transform.position, owner.target.position);
            if (dist <= larva.attackRange)
            {
                isPulsing = false;
                larva.agent.speed = 0;
                yield break;
            }

            if (larva.agent.isActiveAndEnabled)
                larva.agent.SetDestination(owner.target.position);

            float t = elapsed / larva.pulseDuration;
            float curveStep = Mathf.Sin(t * Mathf.PI);
            larva.agent.speed = curveStep * larva.maxPulseSpeed;

            elapsed += Time.deltaTime;
            yield return null;
        }

        larva.agent.speed = 0;
        isPulsing = false;
    }

    private IEnumerator LarvaStingRoutine()
    {
        isAttacking = true;

        // 1. Останавливаем NavMesh
        if (larva.agent != null && larva.agent.isActiveAndEnabled)
        {
            larva.agent.isStopped = true;
            larva.agent.enabled = false;
        }

        // 2. Включаем физику
        larva.rb.isKinematic = false;

        // Запоминаем направление к игроку до разворота
        Vector3 directionToPlayer = (owner.target.position - transform.position);
        directionToPlayer.y = 0;
        directionToPlayer.Normalize();

        // 3. Разворот задом к игроку
        float turnTimer = 0;
        while (turnTimer < 0.5f)
        {
            if (owner.target == null)
            {
                ResetAfterAttack();
                yield break;
            }

            Quaternion lookAwayRot = Quaternion.LookRotation(-directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAwayRot, Time.deltaTime * larva.turnBeforeAttackSpeed);

            turnTimer += Time.deltaTime;
            yield return null;
        }

        // 4. Рывок задом к игроку
        larva.isDashingNow = true;

        Vector3 stingDashDir = directionToPlayer + Vector3.up * 0.1f;
        larva.rb.AddForce(stingDashDir * larva.dashForce, ForceMode.Impulse);

        // Даём время на полёт
        yield return new WaitForSeconds(0.6f);

        // Не попали — умираем
        larva.Die();
    }

    // Сброс состояния если атака прервалась (цель пропала и т.д.)
    private void ResetAfterAttack()
    {
        larva.isDashingNow = false;
        isAttacking = false;
        larva.rb.isKinematic = true;

        if (larva.agent != null)
        {
            larva.agent.enabled = true;
            larva.agent.isStopped = false;
        }
    }
}