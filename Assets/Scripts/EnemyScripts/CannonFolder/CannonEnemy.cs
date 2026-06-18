using UnityEngine;

public class CannonEnemy : Enemy
{
    [Header("Настройки пушки")]
    public float chargeTime = 2.5f;
    public float fireRate = 2.0f;
    public float projectileSpeed = 30f;
    public float damage = 40f;
    public float activationRange = 40f;
    public float viewAngle = 45f;
    [Header("Точность")]
    [Range(0, 1)]
    public float leadAccuracy = 1.0f;

    [Header("Ссылки")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private void Start() => InitializeAI();

    public override void InitializeAI()
    {
        HP = 999999f;
        base.InitializeAI();
    }

    public override void takeDamage(float amount)
    {
        Debug.Log("Эта пушка слишком прочная!");
    }

    public override void Die() {}
    public override void Regeneration() { }
        
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {   
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * activationRange);

        Vector3 rightBoundary = Quaternion.AngleAxis(viewAngle * 0.5f, Vector3.up) * transform.forward;
        Vector3 leftBoundary = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, rightBoundary * activationRange);
        Gizmos.DrawRay(transform.position, leftBoundary * activationRange);
    }
#endif
}