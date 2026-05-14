using System.Collections.Generic;

public abstract class NodeBase
{
    public abstract ElementType NodeType { get; }
    public ElementType GetElementType() { return NodeType; }

    public virtual float Damage { get; set; }
    public virtual float Range { get; set; } 
    public virtual float Speed { get; set; } 
    public virtual float Weight { get; set; }

    public abstract AttackType GetDominantAttack();

    public abstract List<ElementType> SynergyWith { get; }
    public abstract List<ElementType> IncompatibleWith { get; }

    public abstract NodeComposition GetBaseComposition();
}