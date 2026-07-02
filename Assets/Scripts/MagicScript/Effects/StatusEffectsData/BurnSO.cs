using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(menuName = "StatusEffects/Burn")]
public class BurnSO : StatusEffectSO
{
    [Header("Burn")]
    public float damagePerTick = 3f;

    protected override void OnTick(GameObject target)
    {
        var entity = target.GetComponentInParent<Entity>();
        if (entity != null)
        {
            entity.takeDamage(damagePerTick);
            Debug.Log($"[BurnSO] take damage {damagePerTick}");
        }
    }
}