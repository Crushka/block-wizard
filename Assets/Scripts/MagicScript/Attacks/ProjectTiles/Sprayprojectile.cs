using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SprayProjectile : MonoBehaviour
{
    private float _damage, _lifetime;
    private bool _isDead;
    private Rigidbody _rb;

    private Vector3 _rotationAxis;
    private float _rotationSpeed;



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
        transform.rotation = Random.rotation;
        dir += transform.up * 0.15f;
        dir.Normalize();

        _rb.linearVelocity = dir * speed;


        _rb.useGravity = true;

        Destroy(gameObject, lifetime);
    }

    public void SetupVisual(NodeBase node)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    float originalAlpha = mat.GetColor("_BaseColor").a;
                    Color c = node.PrimaryColor;
                    c.a = originalAlpha;
                    mat.SetColor("_BaseColor", c);
                }

                if (mat.HasProperty("_Color"))
                {
                    float originalAlpha = mat.GetColor("_Color").a;
                    Color c = node.PrimaryColor;
                    c.a = originalAlpha;
                    mat.SetColor("_Color", c);
                }

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", node.PrimaryColor * node.EmissionIntensity);
                }
            }
        }

        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.startColor = node.PrimaryColor;
            trail.endColor = new Color(node.PrimaryColor.r,
                                         node.PrimaryColor.g,
                                         node.PrimaryColor.b, 0f);
            trail.time = 0.15f * node.TrailLength;
        }

        var ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = node.PrimaryColor;
            main.startSize = new ParticleSystem.MinMaxCurve(
                0.05f * node.ParticleSize,
                0.15f * node.ParticleSize);
        }
    }

    void Update()
    {
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Тег этого : {other.tag}");

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !other.CompareTag("Player"))
        {
            damageable.takeDamage(_damage);
            Debug.Log($"нанесен урон: {other.gameObject.name}");
            Die();
            return;
        }
    }


    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Destroy(gameObject);
    }
}