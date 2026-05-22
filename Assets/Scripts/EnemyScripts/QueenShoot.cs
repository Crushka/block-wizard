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

        // 1. Берем направление, куда смотрит сам снаряд. 
        // Оно уже задано королевой при Instantiate через bulletRotation.
        Vector3 dir = transform.forward;

        // 2. Добавляем небольшой случайный разброс
        if (spread > 0)
        {
            dir = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            ) * dir;
        }

        // 3. Устанавливаем скорость полета
        _rb.linearVelocity = dir * speed;

        // 4. ВЫКЛЮЧАЕМ ГРАВИТАЦИЮ - снаряд летит прямо
        _rb.useGravity = false;

        // Поворачиваем сам снаряд "лицом" по вектору его полета (с учетом разброса)
        transform.rotation = Quaternion.LookRotation(dir);

        // Настройка визуального вращения (для красоты, не влияет на полет)
        _rotationAxis = Random.onUnitSphere;
        _rotationSpeed = Random.Range(200f, 500f);

        Destroy(gameObject, lifetime);
    }

    // ... (остальные методы SetupVisual, Update, OnTriggerEnter, Die без изменений) ...

    void Update()
    {
        // Вращаем снаряд вокруг своей оси для динамики
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
        // Убедитесь, что "Enemy" - это тег, который назначен Королеве и мелким пчелам
        // Если снаряд касается другого Enemy, он не должен наносить себе урон
        else if (!(other.CompareTag("Enemy") || other.CompareTag("StreamProjectile")))
        {
            // Если попали в стену, пол, или что-то не-вражеское, пуля уничтожается
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}