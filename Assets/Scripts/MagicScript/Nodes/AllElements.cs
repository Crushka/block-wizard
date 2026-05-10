using System.Collections.Generic;

public abstract class ElementNode : NodeBase { }


//элементы 1 уровня (для игрока)
public class NoneElement : ElementNode // 0
{
    public override ElementType NodeType => ElementType.None;

    public NoneElement()
    {
        Damage = 0f;
        Range = 0f;
        Speed = 0f;
        Weight = 0f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { };
    public override List<ElementType> IncompatibleWith { get; } = new() {  };

    public override AttackType GetDominantAttack() => AttackType.Thunder;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        
        return comp;
    }
}
public class FireNode : ElementNode // 1
{
    public override ElementType NodeType => ElementType.Fire;

    public FireNode()
    {
        Damage = 20f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.3f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Air, ElementType.Lightning };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Stream;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Fire, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(EffectType.Burn, 0.8f);
        return comp;
    }
}

public class WaterNode : ElementNode // 2
{
    public override ElementType NodeType => ElementType.Water;

    public WaterNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Ice, ElementType.Poison, ElementType.Lightning };
    public override List<ElementType> IncompatibleWith { get; } = new()
        {  };

    public override AttackType GetDominantAttack() => AttackType.Beam;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Beam, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class EartNode : ElementNode // 3
{
    public override ElementType NodeType => ElementType.Earth;

    public EartNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Poison, ElementType.Fire };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Air, ElementType.Lightning };

    public override AttackType GetDominantAttack() => AttackType.Ball;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class AirNode : ElementNode // 4
{
    public override ElementType NodeType => ElementType.Air;

    public AirNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Fire, ElementType.Lightning };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Earth, ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Thunder;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Air, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class LightningNode : ElementNode // 5
{
    public override ElementType NodeType => ElementType.Lightning;

    public LightningNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Earth, ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Lightning, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}


// элемента 2 уровня (только свмещением первых)
public class SteamNode : ElementNode
{
    public override ElementType NodeType => ElementType.Steam;

    public SteamNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 1.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class MudNode : ElementNode
{
    public override ElementType NodeType => ElementType.Mud;

    public MudNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 1.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}

public class PlasmaNode : ElementNode
{
    public override ElementType NodeType => ElementType.Plasma;

    public PlasmaNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 1.2f;
        Weight = 0.5f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(EffectType.Slow, 0.6f);
        return comp;
    }
}




//элемнты 3 уровня 