using System.Collections.Generic;

public abstract class ElementNode : NodeBase { }

public class FireNode : ElementNode
{
    public override ElementType NodeType => ElementType.Fire;

    public FireNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.2f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Fire, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Burn, 0.8f);
        return comp;
    }
}

public class WaterNode : ElementNode
{
    public override ElementType NodeType => ElementType.Water;

    public WaterNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}
public class SteamNode : ElementNode
{
    public override ElementType NodeType => ElementType.Water;

    public SteamNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class MudNode : ElementNode
{
    public override ElementType NodeType => ElementType.Water;

    public MudNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class PlasmaNode : ElementNode
{
    public override ElementType NodeType => ElementType.Water;

    public PlasmaNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}