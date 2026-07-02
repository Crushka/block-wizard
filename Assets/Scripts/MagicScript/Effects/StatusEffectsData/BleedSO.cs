using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(menuName = "StatusEffects/Bleed")]
public class BleedSO : StatusEffectSO
{
    [Header("Bleed")]
    public float damagePerTick = 3f;

    protected override void OnTick(GameObject target)
    {
        var entity = target.GetComponentInParent<Entity>();
        if (entity != null)
        {
            entity.takeDamage(damagePerTick);
            Debug.Log($"[BleedSo] take damage {damagePerTick}");
        }
    }
}