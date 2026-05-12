using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    private float _damage, _range, _speed;
    private Vector3 _startPos;
    private bool _isDestroyed = false;

    public void Setup(float damage, float range, float speed)
    {
        _damage = damage;
        _range = range;
        _speed = speed;
        _startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);

        if (Vector3.Distance(_startPos, transform.position) >= _range)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable != null && !other.CompareTag("Player"))
        {
            damageable.takeDamage(_damage);
        }
        Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        Die();
    }
    private void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Destroy(gameObject);
    }
}