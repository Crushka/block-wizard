using UnityEngine;
using UnityEngine.AI;

public class LarvaEnemy : Enemy
{
    [Header("Настройки Зародыша")]
    public float attackRange = 2f;
    public float dashForce = 15f;
    public float dashDamage = 10f;
    public float turnBeforeAttackSpeed = 15f;

    [Header("Настройки Движения (Крещендо)")]
    public float maxPulseSpeed = 8f;
    public float pulseDuration = 1.2f;
    public float restDuration = 0.6f;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Rigidbody rb;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent != null)
        {
            agent.stoppingDistance = 0.5f;
            agent.updateRotation = true;
            agent.acceleration = 1000f;
        }

        base.InitializeAI();
    }

    public override void takeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    public override void Regeneration() { }
}