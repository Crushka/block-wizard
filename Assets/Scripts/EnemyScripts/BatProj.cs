using UnityEngine;

public class BatProj : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private bool isInitialized = false;

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
        if (other.CompareTag("Enemy") || other.CompareTag("Bat")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && ((other.CompareTag("Player") || other.CompareTag("PlayerBody")||LayerMask.NameToLayer("Player") == other.gameObject.layer)))
        {
            damageable.takeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}