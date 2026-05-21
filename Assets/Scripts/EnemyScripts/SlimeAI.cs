using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour, IDamageable
{
    [Header("Характеристики")]
    public float health = 100f;
    public float damage = 15f;
    public bool isDead = false;

    [Header("Движение")]
    public Transform target;
    public float lookRadius = 15f;

    public float stopDistance = 2.5f;

    public float moveSpeed = 3.5f;

    [Header("Атака")]
    public float attackRate = 2f;
    public float lungeForce = 10f;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private int enemyLayerMask;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        agent.updateRotation = false;
        agent.stoppingDistance = stopDistance;
        agent.speed = moveSpeed;

        enemyLayerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        if (target == null)
            target = GameObject.FindGameObjectWithTag("PlayerBody")?.transform;
    }

    private void Update()
    {
        if (isDead || target == null || isAttacking) return;

        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadius)
        {
            HandleBehavior(distance);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        HandleRotation(distance <= lookRadius);
    }

    private void HandleBehavior(float distance)
    {
        if (distance <= stopDistance + 0.5f)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(SlimeAttackRoutine());
                nextAttackTime = Time.time + attackRate;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    private IEnumerator SlimeAttackRoutine()
    {
        isAttacking = true;
        agent.enabled = false;

        Debug.Log("СЛИЗЕНЬ: Подготовка к прыжку!");
        yield return new WaitForSeconds(0.5f);

        if (target != null)
        {
            Vector3 attackDir = (target.position - transform.position).normalized;

            attackDir.y = 0.2f; 


            rb.AddForce(attackDir * lungeForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.8f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward, 1.5f);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                IDamageable d = col.GetComponent<IDamageable>();
                d?.takeDamage(damage);
                Debug.Log("СЛИЗЕНЬ: Ударил игрока!");
            }
        }


        yield return new WaitForSeconds(0.5f); 


        agent.enabled = true;
        isAttacking = false;
    }

    private void HandleRotation(bool isChasing)
    {
        if (isAttacking) return;

        RaycastHit hit;
        Vector3 surfaceNormal = Vector3.up;

        if (Physics.Raycast(transform.position + Vector3.up * 1.0f, Vector3.down, out hit, 3f, enemyLayerMask))
        {
            surfaceNormal = hit.normal;
        }

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 forwardOnSlope = Vector3.ProjectOnPlane(isChasing ? directionToTarget : transform.forward, surfaceNormal);

        if (forwardOnSlope != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forwardOnSlope, surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void takeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("Слизень получил урон: " + amount + ", Осталось: " + health);

        if (health <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        agent.enabled = false;
        StopAllCoroutines();

        Destroy(gameObject, 0.5f);
    }
}