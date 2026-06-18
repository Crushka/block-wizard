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

    protected bool _isDestroyed = false;

    public abstract void Setup(float damage, float speed, float range, float lifetime = 0f, float spread = 0f);
    public abstract void SetupVisual(NodeBase _node);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Projectile")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.takeDamage(_damage);
            Die();
            return;
        }
    }

    private void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Destroy(gameObject);
    }
}
