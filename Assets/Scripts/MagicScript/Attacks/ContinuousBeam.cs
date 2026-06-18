using UnityEngine;

public abstract class ContinuousBeam : MonoBehaviour
{
    protected float _damage;
    protected float _range;

    [Header("Настройки луча и урона")]
    [SerializeField] protected float _damageInterval = 0.25f;
    [SerializeField] protected float _beamRadius = 0.5f;

    private float _damageTimer;
    private LayerMask _enemyMask;
    private LayerMask _solidObstacleMask;
    private LayerMask _combinedMask;

    protected Vector3 _hitPoint;

    protected virtual void Awake()
    {
        _enemyMask = LayerMask.GetMask("Enemy");
        // Укажи здесь слои твоего окружения (стены, пол), чтобы луч сквозь них не проходил
        _solidObstacleMask = LayerMask.GetMask("Default", "Environment", "NonePlayebleObject");
        _combinedMask = _enemyMask | _solidObstacleMask;
    }

    public virtual void Setup(float damage, float range)
    {
        _damage = damage;
        _range = range;
    }

    public abstract void SetupVisual(NodeBase node);

    /// <summary>
    /// Вызывается каждый кадр из класса атаки. 
    /// Снаряд сам знает, где он находится и куда направлен благодаря своему Transform.
    /// </summary>
    public void TickBeam()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // Пускаем сферу по направлению объекта
        if (Physics.SphereCast(origin, _beamRadius, direction, out RaycastHit hit, _range, _combinedMask))
        {
            _hitPoint = hit.point;
            HandleIntervalDamage(hit.collider);
        }
        else
        {
            _hitPoint = origin + direction * _range;
            _damageTimer = 0f;
        }

        // Передаем локальные/мировые точки в дочерний класс для визуала
        OnUpdateVisual(origin, _hitPoint);
    }

    private void HandleIntervalDamage(Collider hitCollider)
    {
        _damageTimer -= Time.deltaTime;
        if (_damageTimer <= 0f)
        {
            _damageTimer = _damageInterval;

            IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.takeDamage(_damage);
            }
        }
    }

    protected abstract void OnUpdateVisual(Vector3 start, Vector3 end);
}