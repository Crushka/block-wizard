using UnityEngine;

public class SlimeEnemy : Enemy
{
    [Header("Настройки Прыжков")]
    public float jumpHeight = 3f;
    public float jumpInterval = 1.2f;
    public float lookRadius = 20f;
    public float attackRange = 8f;
    public float gravityMultiplier = 3f;

    [Header("Параметры Боя")]
    public float damage = 20f;
    public float knockbackForce = 100f;
    public float damageRadius = 2f;

    [Header("Физика")]
    public LayerMask groundMask;
    [HideInInspector] public Rigidbody rb;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        base.InitializeAI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (baseAI is SlimeBehaviour brain)
        {
            brain.OnJumpCollision(collision.gameObject);
        }
    }

    public override void takeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0) Die();
    }

    public override void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject, 0.2f);
    }

    public override void Regeneration() { }
}