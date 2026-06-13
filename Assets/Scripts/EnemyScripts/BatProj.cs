using UnityEngine;

public class BatProj : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private bool isInitialized = false;
    private bool _hasDealtDamage = false;

    public void SetupDirect(Vector3 dir, float moveSpeed, float dmg)
    {
        direction = dir;
        speed = moveSpeed;
        damage = dmg;
        isInitialized = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasDealtDamage) return;
        if (other.CompareTag("Enemy") || other.CompareTag("Bat")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        bool isPlayer = other.CompareTag("Player") ||
                        other.CompareTag("PlayerBody") ||
                        other.gameObject.layer == LayerMask.NameToLayer("Player");

        if (damageable != null && isPlayer)
        {
            _hasDealtDamage = true;

            damageable.takeDamage(damage);

            Debug.Log($"Снаряд попал в игрока! Урон: {damage}");

            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            _hasDealtDamage = true;
            Destroy(gameObject);
        }
    }
}