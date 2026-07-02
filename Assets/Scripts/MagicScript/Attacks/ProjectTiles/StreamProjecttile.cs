using GLTFast.Schema;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class StreamProjecttile : ProjectTile
{
    private float _lifetime;
    private bool _isDead;
    private Rigidbody _rb;

    protected override float DamageMultiplier => 0.68f;

    protected override void OnInit(float lifetime, float spread)
    {
        _lifetime = lifetime;
        _rb = GetComponent<Rigidbody>();


        Vector3 dir = transform.forward;
        dir = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        ) * dir;

        dir += transform.up * 0.15f;
        dir.Normalize();

        transform.rotation = Quaternion.LookRotation(dir);

        _rb.linearVelocity = dir * _speed;

        _rb.useGravity = false;

        Destroy(gameObject, lifetime);
    }

    public override void SetupVisual(NodeBase node)
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

}