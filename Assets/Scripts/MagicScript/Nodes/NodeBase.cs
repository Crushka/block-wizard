using System.Collections.Generic;
using UnityEngine;

public abstract class NodeBase
{
    public abstract ElementType NodeType { get; }
    public ElementType GetElementType() { return NodeType; }

    public virtual float Damage { get; set; }
    public virtual float Range { get; set; } 
    public virtual float Speed { get; set; } 
    public virtual float Weight { get; set; }

    public virtual Color PrimaryColor { get; set; } = Color.white;
    public virtual float EmissionIntensity { get; set; } = 1f;  
    public virtual float TrailLength { get; set; } = 1f;
    public virtual float ParticleSize { get; set; } = 1f;

    public abstract AttackType GetDominantAttack();

    public abstract List<ElementType> SynergyWith { get; }
    public abstract List<ElementType> IncompatibleWith { get; }

    public abstract NodeComposition GetBaseComposition();
}