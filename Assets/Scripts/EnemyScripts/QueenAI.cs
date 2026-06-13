using UnityEngine;
using System.Collections;

public class QueenAI : MonoBehaviour, IDamageable
{
    public enum QueenState { Phase1, Transition, Phase2_Moving, Phase2_Shooting, Phase2_Summoning }
    public QueenState currentState = QueenState.Phase1;

    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform stingerMuzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform[] waypoints;

    [Header("Настройки движения")]
    public float baseSpeed = 10f;
    public float phase2Speed = 6f;
    public float rotationSpeed = 5f;
    public float hoverDistance = 10f;

    [Header("Настройки пулемета")]
    public float shootingDuration = 4f;
    public float fireRate = 0.1f;
    public float bulletDamage = 5f;
    public float bulletSpeed = 20f;

    [Header("Настройки призыва")]
    public int minionsToSummon = 3;

    [Header("Вторая фаза")]
    [SerializeField] private Transform phase2LandingSpot;
    [SerializeField] private float transitionMoveSpeed = 5f;

    private float currentSpeed;
    private int currentWaypointIndex = 0;
    public float health = 3000f;

    // Проверяет жив ли игрок (не null и объект не уничтожен)
    private bool PlayerAlive => player != null && player.gameObject != null;

    void Start()
    {
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        // Ищем игрока если потеряли ссылку
        if (!PlayerAlive)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerBody");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (!PlayerAlive) return;

        switch (currentState)
        {
            case QueenState.Phase1:
                MoveBetweenWaypoints();
                break;
            case QueenState.Phase2_Moving:
                MaintainDistanceToPlayer();
                break;
        }
    }

    #region Фаза 1
    private void MoveBetweenWaypoints()
    {
        if (waypoints.Length < 2) return;
        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);
        RotateTowards(target.position);

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
    #endregion

    #region Переход во 2 фазу
    public void ActivatePhase2()
    {
        if (currentState != QueenState.Phase1) return;
        StartCoroutine(TransitionToPhase2());
    }

    IEnumerator TransitionToPhase2()
    {
        currentState = QueenState.Transition;
        Vector3 targetPos = phase2LandingSpot != null ? phase2LandingSpot.position : transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, transitionMoveSpeed * Time.deltaTime);

            if (PlayerAlive)
            {
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
            }

            yield return null;
        }

        if (rb != null) rb.isKinematic = false;
        currentState = QueenState.Phase2_Moving;
        StartCoroutine(BossBrain());
    }
    #endregion

    #region ИИ Второй Фазы
    IEnumerator BossBrain()
    {
        while (currentState != QueenState.Phase1)
        {
            // Если игрок мёртв — ждём пока не появится новый
            if (!PlayerAlive)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            float choice = Random.value;

            if (choice < 0.6f)
                yield return StartCoroutine(MachineGunAttack());
            else
                yield return StartCoroutine(SummonAttack());

            currentState = QueenState.Phase2_Moving;
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }

    IEnumerator MachineGunAttack()
    {
        currentState = QueenState.Phase2_Shooting;
        float timer = 0;

        while (timer < shootingDuration)
        {
            // Прерываем атаку если игрок погиб
            if (!PlayerAlive)
            {
                currentState = QueenState.Phase2_Moving;
                yield break;
            }

            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            Vector3 flatDir = new Vector3(dirToPlayer.x, 0, dirToPlayer.z);

            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDir);
                Quaternion offset = Quaternion.Euler(0, -90, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * offset, Time.deltaTime * rotationSpeed);
            }

            if (bulletPrefab && stingerMuzzle)
            {
                Vector3 fireDir = (player.position + Vector3.up * 1.0f - stingerMuzzle.position).normalized;
                Quaternion bulletRotation = Quaternion.LookRotation(fireDir);

                GameObject bullet = Instantiate(bulletPrefab, stingerMuzzle.position, bulletRotation);

                var queenShootComponent = bullet.GetComponent<QueenShoot>();
                if (queenShootComponent != null)
                    queenShootComponent.Setup(bulletDamage, bulletSpeed, 3f, 5f);
            }

            timer += fireRate;
            yield return new WaitForSeconds(fireRate);
        }
    }

    IEnumerator SummonAttack()
    {
        currentState = QueenState.Phase2_Summoning;

        for (int i = 0; i < minionsToSummon; i++)
        {
            if (minionPrefab)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
                spawnPos.y = Mathf.Max(spawnPos.y, 1f);
                Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1f);
    }

    private void MaintainDistanceToPlayer()
    {
        if (!PlayerAlive) return;

        Vector3 targetPos = player.position + (transform.position - player.position).normalized * hoverDistance;
        targetPos.y = player.position.y + 4f;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, phase2Speed * Time.deltaTime);
        RotateTowards(player.position);
    }
    #endregion

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            Quaternion offset = Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * offset, Time.deltaTime * rotationSpeed);
        }
    }

    public void IncreaseAnger() { currentSpeed += 3f; }

    public void takeDamage(float amount)
    {
        health -= amount > 40 ? 40 : amount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Королева побеждена!");
        StopAllCoroutines();
        Destroy(gameObject);
    }
}