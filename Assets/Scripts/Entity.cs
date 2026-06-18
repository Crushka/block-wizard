using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable
{
    public float maxHP = 0;
    public float HP = 0;
    public float speed = 0;
    public float reg_points = 0;
    //List<Effect> effects;
    //state
    public virtual void Regeneration()
    {
        HP += 0;
    }

    public abstract void takeDamage(float amount);

    public abstract void Die();

    //public abstract void GetEffect();
}
