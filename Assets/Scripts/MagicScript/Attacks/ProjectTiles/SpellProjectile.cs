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
    private Vector3 _flyDirection;
    private bool _isDestroyed = false;

    private Vector3 _rotationAxis;
    private float _rotationSpeed;

    public void Setup(float damage, float range, float speed)
    {
        _damage = damage;
        _range = range;
        _speed = speed;
        _startPos = transform.position;

        _flyDirection = transform.forward;
        _rotationAxis = UnityEngine.Random.onUnitSphere;
        _rotationSpeed = UnityEngine.Random.Range(100f, 500f);
        transform.rotation = UnityEngine.Random.rotation;
    }

    void Update()
    {
        transform.position += _flyDirection * _speed * Time.deltaTime;

        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(_startPos, transform.position) >= _range)
            Destroy(gameObject);
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