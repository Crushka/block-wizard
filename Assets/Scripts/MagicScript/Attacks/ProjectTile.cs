using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ProjectTile: MonoBehaviour
{
    protected float _damage;
    protected float _speed;
    protected float _range;
    protected StatusEffectType effectType;
    protected float buildupAmount = 25f;

    protected bool _isDestroyed = false;

    protected virtual float DamageMultiplier => 1f;
    public virtual void Setup(float damage, float range, float speed,
                              StatusEffectType effect, float lifetime = 0f, float spread = 0f)
    {
        _damage = damage * DamageMultiplier;
        _range = range;
        _speed = speed;
        effectType = effect;

        OnInit(lifetime, spread);
    }

    protected virtual void OnInit(float lifetime, float spread) { }
    public abstract void SetupVisual(NodeBase _node);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Projectile")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        StatusEffectManager effectManager = other.GetComponentInParent<StatusEffectManager>();

        if (damageable != null && effectManager != null)
        {
            damageable.takeDamage(_damage);
            effectManager.TriggerBuildup(effectType, buildupAmount);

            Die();
            return;
        }
        else Debug.Log($"[ProjectTile] ОШИБКА: damageable: {damageable != null} effectManager: {effectManager != null}");
    }

    private void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Destroy(gameObject);
    }
}
