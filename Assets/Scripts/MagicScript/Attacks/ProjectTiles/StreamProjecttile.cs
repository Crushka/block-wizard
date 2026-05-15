using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class StreamProjecttile : MonoBehaviour
{
    private float _damage, _lifetime;
    private bool _isDead;
    private Rigidbody _rb;


    public void Setup(float damage, float speed, float lifetime, float spread = 8f)
    {
        _damage = damage;
        _lifetime = lifetime;
        _rb = GetComponent<Rigidbody>();


        Vector3 dir = transform.forward;
        dir = Quaternion.Euler(
            Random.Range(-speed, spread),
            Random.Range(-speed, spread),
            0f
        ) * dir;

        dir += transform.up * 0.15f;
        dir.Normalize();

        transform.rotation = Quaternion.LookRotation(dir);

        _rb.linearVelocity = dir * speed;

        _rb.useGravity = false;

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
                   
                    Color c = node.PrimaryColor;
        
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

    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"trigger {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("StreamProjectile") || collision.gameObject.CompareTag("Untagged")) return;
        Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Destroy(gameObject);
    }
}