using System.Collections;
using UnityEngine;

public class BossQueen : BossAI
{
    [Header("Настройки Босса")]
    public float damageCap = 40f;
    public float speedBonusPerCage = 3f;
    public LayerMask groundMask;

    [Header("Ссылки для атак")]
    public Transform stingerMuzzle;
    public GameObject bulletPrefab;
    public GameObject minionPrefab;
    public Transform[] waypoints;
    public Transform phase2LandingSpot;
    public int minionsToSummon;
    [Header("Настройки Лазера")]
    public LineRenderer laserLine;
    public float laserFlyHeight = 10f;
    public float laserDamage = 2f;
    public float laserStartOffset = 7f;
    public float laserTrackingSpeed = 5f;
    public static bool MinionsFrozen = false;

    [Header("Параметры баланса")]
    public float phase2Speed = 6f;
    public float hoverDistance = 10f;
    public float bulletDamage = 5f;
    public float bulletSpeed = 20f;

    private void Start()
    {
        InitializeAI();

        if (baseAI != null) baseAI.enabled = true;
        if (bossAI != null) bossAI.enabled = false;
    }

    protected void Update()
    {
        if (HP <= 0) return;

        if (baseAI != null && baseAI.enabled)
        {
            baseAI.execute();
        }

        else if (bossAI != null && bossAI.enabled)
        {
            bossAI.execute();
        }
    }

    public void ActivatePhase2()
    {
        if (baseAI != null) baseAI.enabled = false;
        StartCoroutine(TransitionToPhase2Routine());
    }

    private IEnumerator TransitionToPhase2Routine()
    {
        Debug.Log("Переход во 2 фазу...");
        Vector3 targetPos = phase2LandingSpot != null ? phase2LandingSpot.position : transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 10f * Time.deltaTime);
            yield return null;
        }

        if (rb != null) rb.isKinematic = false;

        if (bossAI != null)
        {
            bossAI.enabled = true;
            if (bossAI is QueenCombatBehaviour combat)
            {
                combat.StartCombat();
            }
        }
    }

    public void IncreaseAnger()
    {
        // Увеличиваем базовую скорость, которая находится в Entity
        this.speed += speedBonusPerCage;
        Debug.Log($"Злость королевы растет! Новая скорость полета: {this.speed}");
    }


    public override void takeDamage(float amount)
    {
        float actualDamage = amount > damageCap ? damageCap : amount;
        HP -= actualDamage;
        Debug.Log($"Королева: {HP} HP. Получено: {actualDamage}");

        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        Debug.Log("Королева повержена!");
        Destroy(gameObject);
    }

    public override void Regeneration() { }
}
