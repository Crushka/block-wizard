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
    public float hoverDistance = 10f; // Дистанция от игрока во 2 фазе

    [Header("Настройки пулемета")]
    public float shootingDuration = 4f;
    public float fireRate = 0.1f;
    public float bulletDamage = 5f;
    public float bulletSpeed = 20f;

    [Header("Настройки призыва")]
    public int minionsToSummon = 3;

    private float currentSpeed;
    private int currentWaypointIndex = 0;
    public float health = 500f;

    void Start()
    {
        currentSpeed = baseSpeed;
        if (player == null) player = GameObject.FindGameObjectWithTag("PlayerBody").transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case QueenState.Phase1:
                MoveBetweenWaypoints();
                break;
            case QueenState.Phase2_Moving:
                MaintainDistanceToPlayer();
                break;
                // Состояния Shooting и Summoning управляются корутинами
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
        Debug.Log("Королева спускается...");

        Vector3 targetPos = new Vector3(transform.position.x, 4f, transform.position.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);
            RotateTowards(player.position);
            yield return null;
        }

        currentState = QueenState.Phase2_Moving;
        StartCoroutine(BossBrain());
    }
    #endregion

    #region ИИ Второй Фазы
    IEnumerator BossBrain()
    {
        while (currentState != QueenState.Phase1) // Пока жива
        {
            // Случайный выбор атаки
            float choice = Random.value;

            if (choice < 0.6f) // 60% шанс на пулемет
                yield return StartCoroutine(MachineGunAttack());
            else // 40% шанс на призыв
                yield return StartCoroutine(SummonAttack());

            // Пауза между атаками (просто летаем за игроком)
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
            // Поворачиваемся жалом к игроку (если жало сзади, инвертируем направление)
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Выстрел
            if (bulletPrefab && stingerMuzzle)
            {
                GameObject bullet = Instantiate(bulletPrefab, stingerMuzzle.position, stingerMuzzle.rotation);
                // Настраиваем пулю (предполагаем, что на ней висит ваш SpellProjectile или StreamProjectile)
                var proj = bullet.GetComponent<StreamProjecttile>();
                if (proj != null) proj.Setup(bulletDamage, bulletSpeed, 3f, 5f);
            }

            timer += fireRate;
            yield return new WaitForSeconds(fireRate);
        }
    }

    IEnumerator SummonAttack()
    {
        currentState = QueenState.Phase2_Summoning;
        Debug.Log("Королева призывает подмогу!");

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
        // Королева держится на расстоянии от игрока, летая вокруг него
        Vector3 targetPos = player.position + (transform.position - player.position).normalized * hoverDistance;
        targetPos.y = player.position.y + 4f; // Всегда чуть выше игрока

        transform.position = Vector3.MoveTowards(transform.position, targetPos, phase2Speed * Time.deltaTime);
        RotateTowards(player.position);
    }
    #endregion

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);
        }
    }

    public void IncreaseAnger() { currentSpeed += 3f; }

    public void takeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Королева побеждена!");
        StopAllCoroutines();
        // Можно добавить взрыв или падение
        Destroy(gameObject);
    }
}