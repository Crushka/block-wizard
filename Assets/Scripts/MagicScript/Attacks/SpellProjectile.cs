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

    public void Setup(float damage, float range, float speed)
    {
        _damage = damage;
        _range = range;
        _speed = speed;
        _startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        if (Vector3.Distance(_startPos, transform.position) >= _range)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        

        Destroy(gameObject);
    }
}