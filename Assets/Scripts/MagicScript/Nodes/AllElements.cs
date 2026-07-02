using System.Collections.Generic;
using UnityEngine;
public abstract class ElementNode : NodeBase {

    protected ElementNode(int num){ Weight = 1.08f * Mathf.Log(1 + num); }
    protected ElementNode() { }
    
}

public enum ElementType
{
    Fire,
    Water,
    Earth,
    Air,
    Cold,
    Lightning,

    Steam,
    Ice,
    Sound,
    Fog,
    Lava,
    
    Acid,
    Plasma,
   



    Unknown,
    None
}

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
    public override NodeBase Clone()
    {
        var copy = new NoneElement();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new() { };
    public override List<ElementType> IncompatibleWith { get; } = new() { };

    public override AttackType GetDominantAttack() => AttackType.Thunder;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();

        return comp;
    }
}
public class FireNode : ElementNode // 1
{
    public override ElementType NodeType => ElementType.Fire;

    public FireNode(int num) : base(num)
    {
        Damage = 7f;
        Range = 11f;
        Speed = 6f;
        PrimaryColor = new Color(1.0f, 0.3f, 0.0f);
        EmissionIntensity = 0.9f;
        TrailLength = 1.3f;
        ParticleSize = 1.2f;
    }
    public FireNode() { }
    public override NodeBase Clone()
    {
        var copy = new FireNode();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Air, ElementType.Lightning, ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Ice, ElementType.Earth };

    public override AttackType GetDominantAttack() => AttackType.Stream;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Fire, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Burn, 0.8f);
        return comp;
    }
}

public class WaterNode : ElementNode // 2
{
    public override ElementType NodeType => ElementType.Water;

    public WaterNode(int num) : base(num)
    {
        Damage = 6f;
        Range = 9f;
        Speed = 6f;
        PrimaryColor = new Color(0.0f, 0.4f, 1.0f);
        EmissionIntensity = 0.3f;
        TrailLength = 1.1f;
        ParticleSize = 0.9f;
    }
    public WaterNode() { }
    public override NodeBase Clone()
    {
        var copy = new WaterNode();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Ice, ElementType.Lightning, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new()
    { };

    public override AttackType GetDominantAttack() => AttackType.Spray;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class EartNode : ElementNode // 3
{
    public override ElementType NodeType => ElementType.Earth;

    public EartNode(int num) : base(num)
    {
        Damage = 9f;
        Range = 7f;
        Speed = 4f;
        PrimaryColor = new Color(0.45f, 0.25f, 0.1f);
        EmissionIntensity = 0.1f;
        TrailLength = 0.7f;
        ParticleSize = 1.5f;   // крупные обломки
    }
    public EartNode() { }
    public override NodeBase Clone()
    {
        var copy = new EartNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Air, ElementType.Cold };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Lightning };

    public override AttackType GetDominantAttack() => AttackType.Ball;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(StatusEffectType.Bleed, 0.6f);
        return comp;
    }
}

public class AirNode : ElementNode // 4
{
    public override ElementType NodeType => ElementType.Air;

    public AirNode(int num) : base(num)
    {
        Damage = 6f;
        Range = 16f;
        Speed = 7f;
        PrimaryColor = new Color(0.85f, 0.95f, 1.0f);
        EmissionIntensity = 0.2f;
        TrailLength = 1.5f;   // длинный след у воздуха
        ParticleSize = 0.6f;   // мелкие частицы
    }

    public AirNode() { }
    public override NodeBase Clone()
    {
        var copy = new AirNode();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Fire, ElementType.Lightning, ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Stream;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Air, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class ColdNode : ElementNode // 4
{
    public override ElementType NodeType => ElementType.Cold;

    public ColdNode(int num) : base(num)
    {
        Damage = 7f;
        Range = 9f;
        Speed = 6f;
        PrimaryColor = new Color(0.2f, 0.6f, 1.0f);
        EmissionIntensity = 0.4f;
        TrailLength = 1.5f;   // длинный след у воздуха
        ParticleSize = 0.8f;   // мелкие частицы
    }

    public ColdNode() { }
    public override NodeBase Clone()
    {
        var copy = new ColdNode();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Lightning };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Fire,ElementType.Earth };

    public override AttackType GetDominantAttack() => AttackType.Spray;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Air, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Bleed, 0.6f);
        return comp;
    }
}

public class LightningNode : ElementNode // 5
{
    public override ElementType NodeType => ElementType.Lightning;

    public LightningNode(int num) : base(num)
    {
        Damage = 8f;
        Range = 14f;
        Speed = 9f;
        PrimaryColor = new Color(1.0f, 0.95f, 0.5f);
        EmissionIntensity = 1.4f;
        TrailLength = 1.6f;
        ParticleSize = 0.7f;
    }

    public LightningNode() { }
    public override NodeBase Clone()
    {
        var copy = new LightningNode();
        CopyFieldsTo(copy);
        return copy;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Earth, ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Thunder;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Lightning, 1.0f);
        comp.Add(AttackType.Thunder, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Burn, 0.6f);
        return comp;
    }
}


// элемента 2 уровня (только совмещением первых)
public class SteamNode : ElementNode
{
    public override ElementType NodeType => ElementType.Steam;

    public SteamNode(int num) : base(num)
    {
        Damage = 11f;
        Range = 8f;
        Speed = 8f;
        PrimaryColor = new Color(0.8f, 0.8f, 0.85f);
        EmissionIntensity = 0.1f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public SteamNode() { }
    public override NodeBase Clone()
    {
        var copy = new SteamNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Stream;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class IceNode : ElementNode
{
    public override ElementType NodeType => ElementType.Ice;

    public IceNode(int num) : base(num)
    {
        Damage = 13f;
        Range = 13f;
        Speed = 7f;
        PrimaryColor = new Color(0.7f, 0.9f, 1.0f);
        EmissionIntensity = 0.3f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public IceNode() { }
    public override NodeBase Clone()
    {
        var copy = new IceNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spike;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spike, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class FogNode : ElementNode
{
    public override ElementType NodeType => ElementType.Fog;

    public FogNode(int num) : base(num)
    {
        Damage = 10f;
        Range = 7f;
        Speed = 8f;
        PrimaryColor = new Color(0.6f, 0.65f, 0.7f);
        EmissionIntensity = 0.1f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public FogNode() { }
    public override NodeBase Clone()
    {
        var copy = new FogNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}
public class SoundNode : ElementNode
{
    public override ElementType NodeType => ElementType.Sound;

    public SoundNode(int num) : base(num)
    {
        Damage = 12f;
        Range = 6f;
        Speed = 11f;
        PrimaryColor = new Color(0.9f, 0.2f, 0.6f);
        EmissionIntensity = 0.6f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public SoundNode() { }
    public override NodeBase Clone()
    {
        var copy = new SoundNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class LavaNode : ElementNode
{
    public override ElementType NodeType => ElementType.Lava;

    public LavaNode(int num) : base(num)
    {
        Damage = 14f;
        Range = 10f;
        Speed = 8f;
        PrimaryColor = new Color(1.0f, 0.1f, 0.0f);
        EmissionIntensity = 0.95f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public LavaNode() { }
    public override NodeBase Clone()
    {
        var copy = new LavaNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

//3

public class AcidNode : ElementNode
{
    public override ElementType NodeType => ElementType.Acid;

    public AcidNode(int num) : base(num)
    {
        Damage = 15f;
        Range = 8f;
        Speed = 7f;
        PrimaryColor = new Color(0.5f, 1.0f, 0.0f);
        EmissionIntensity = 0.5f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public AcidNode() { }
    public override NodeBase Clone()
    {
        var copy = new AcidNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };
    public override AttackType GetDominantAttack() => AttackType.Beam;
    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Beam, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}
public class PlasmaNode : ElementNode
{
    public override ElementType NodeType => ElementType.Plasma;

    public PlasmaNode(int num) : base(num)
    {
        Damage = 16f;
        Range = 11f;
        Speed = 9f;
        PrimaryColor = new Color(0.9f, 0.1f, 0.9f);
        EmissionIntensity = 1.0f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public PlasmaNode() { }
    public override NodeBase Clone()
    {
        var copy = new PlasmaNode();
        CopyFieldsTo(copy);
        return copy;
    }
    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Beam;

    public override NodeRecipe GetBaseComposition()
    {
        var comp = new NodeRecipe();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Beam, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}