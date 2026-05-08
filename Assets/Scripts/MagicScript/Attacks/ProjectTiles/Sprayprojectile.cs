using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SprayProjectile : MonoBehaviour
{
    private float _damage;
    private float _lifetime;
    private bool _isDead;
    private Rigidbody _rb;

   
    public void Setup(float damage, float speed, float lifetime, float spread = 8f)
    {
        _damage = damage;
        _lifetime = lifetime;
        _rb = GetComponent<Rigidbody>();


        Vector3 dir = transform.forward;
        dir = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        ) * dir;

        dir += transform.up * 0.15f;
        dir.Normalize();

        _rb.linearVelocity = dir * speed;

        _rb.useGravity = true;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SprayProjectile") || other.CompareTag("Player") || other.CompareTag("Untagged")) return;
        Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("SprayProjectile")) return;
        Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Destroy(gameObject);
    }
}