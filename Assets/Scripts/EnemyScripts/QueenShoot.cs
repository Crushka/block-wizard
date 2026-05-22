using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class QueenShoot : MonoBehaviour
{
    private float _damage, _lifetime;
    private bool _isDead;
    private Rigidbody _rb;

    private Vector3 _rotationAxis;
    private float _rotationSpeed;

    public void Setup(float damage, float speed, float lifetime, float spread = 5f)
    {
        _damage = damage;
        _lifetime = lifetime;
        _rb = GetComponent<Rigidbody>();

        Vector3 dir = transform.forward;

        if (spread > 0)
        if (spread > 0)
        {
            dir = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            ) * dir;
        }
        _rb.linearVelocity = dir * speed;
        _rb.useGravity = false;
        transform.rotation = Quaternion.LookRotation(dir);

        _rotationAxis = Random.onUnitSphere;
        _rotationSpeed = Random.Range(200f, 500f);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null && (other.CompareTag("Player") || other.CompareTag("PlayerBody")))
        {
            damageable.takeDamage(_damage);
            Die();
        }
        else if (!(other.CompareTag("Enemy") || other.CompareTag("StreamProjectile")))
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}