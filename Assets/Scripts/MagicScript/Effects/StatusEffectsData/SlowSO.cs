using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/Slow")]
public class SlowSO : StatusEffectSO
{
    [Header("Slow")]
    public float slowMultiplier = 0.1f;

    private float _originalSpeed;

    public override void ApplyEffect(GameObject target)
    {
        base.ApplyEffect(target);
        var entity = target.GetComponentInParent<Entity>();
        if (entity != null)
        {
            _originalSpeed = entity.speed;
            entity.speed *= slowMultiplier;
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        var entity = target.GetComponentInParent<Entity>();
        if (entity != null)
            entity.speed = _originalSpeed;

        base.RemoveEffect(target);
    }

    // OnTick не нужен вообще
}