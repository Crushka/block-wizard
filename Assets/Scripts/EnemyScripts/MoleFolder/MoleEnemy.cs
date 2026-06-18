using UnityEngine;
using UnityEngine.AI;

public class MoleEnemy : Enemy
{
    [Header("Ссылки")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject visuals;
    public GameObject burrowEffect;

    [Header("Настройки обнаружения")]
    public float detectionRange = 15f;

    [Header("Настройки дистанции и времени")]
    public float triggerDistance = 12f;
    public float ambushRadius = 5f;
    public float burrowSpeed = 5f;
    public float minBurrowTime = 3f;
    public float attackDuration = 4f;
    public float fireRate = 1.2f;
    public float rotationSpeed = 10f;
    public float emergeTime = 1f;
    public float submergeTime = 1f;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Collider moleCollider;

    private void Start()
    {
        InitializeAI();
    }

    public override void InitializeAI()
    {
        agent = GetComponent<NavMeshAgent>();
        moleCollider = GetComponent<Collider>();

        if (agent)
        {
            agent.updateRotation = false;
            agent.angularSpeed = 0;
        }
        base.InitializeAI();
    }

    public override void takeDamage(float amount)
    {
        MoleBehaviour brain = baseAI as MoleBehaviour;
        if (brain != null && !brain.CanBeHit) return;

        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        if (agent) agent.enabled = false;
        if (baseAI != null) baseAI.enabled = false;

        if (visuals) visuals.SetActive(true);
        if (burrowEffect) burrowEffect.SetActive(false);

        Destroy(gameObject, 1.5f);
    }

    public override void Regeneration() { }
}