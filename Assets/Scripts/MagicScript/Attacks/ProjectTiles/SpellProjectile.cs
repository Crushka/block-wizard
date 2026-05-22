using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpellProjectile : MonoBehaviour
{
    private float _damage, _range, _speed;
    private Vector3 _startPos;
    private Vector3 _flyDirection;
    private bool _isDestroyed = false;

    private Vector3 _rotationAxis;
    private float _rotationSpeed;

    [Header("Visual – заполняется через SetupVisual()")]
    private TrailRenderer _trail;
    private ParticleSystem _particles;
    private Light _light;

    public void Setup(float damage, float range, float speed)
    {
        Debug.Log($"[SpellProjectile] Setup получил параметры: dmg={damage}, range={range}, speed={speed}");
        _damage = damage;
        _range = range;
        _speed = speed;
        _startPos = transform.position;

        _flyDirection = transform.forward;
        _rotationAxis = Random.onUnitSphere;
        _rotationSpeed = Random.Range(100f, 500f);
        transform.rotation = Random.rotation;
    }

    public void SetupVisual(NodeBase node)
    {
        var vfx = GetComponentInChildren<VisualEffect>();
        if (vfx != null)
        {
            if (vfx.HasGradient("MainColor"))
            {
                Color emissiveColor = node.PrimaryColor * node.EmissionIntensity;
                emissiveColor.a = 1f;

                var gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                    new GradientColorKey(emissiveColor, 0f),
                    new GradientColorKey(node.PrimaryColor, 0.5f),
                    new GradientColorKey(node.PrimaryColor, 1f)
                    },
                    new GradientAlphaKey[]
                    {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                    }
                );

                vfx.SetGradient("MainColor", gradient);
            }
        }

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (rend.GetComponent<VisualEffect>() != null) continue;

            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", node.PrimaryColor * node.EmissionIntensity);
                }

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", node.PrimaryColor);

                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", node.PrimaryColor);
            }
        }
    }

    void Update()
    {
        transform.position += _flyDirection * _speed * Time.deltaTime;

        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(_startPos, transform.position) >= _range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        Debug.Log($"нанесен урон: {other.gameObject.name}");
        if (damageable != null && !other.CompareTag("Player"))
        {
            damageable.takeDamage(_damage);
        }
        Die();
    }
    private void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Destroy(gameObject);
    }
}