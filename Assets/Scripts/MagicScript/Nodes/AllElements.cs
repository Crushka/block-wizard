//using System.Collections.Generic;
//using UnityEngine;
//public abstract class ElementNode : NodeBase { }


////элементы 1 уровня (для игрока)
//public class NoneElement : ElementNode // 0
//{
//    public override ElementType NodeType => ElementType.None;

//    public NoneElement()
//    {
//        Damage = 0f;
//        Range = 0f;
//        Speed = 0f;
//        Weight = 0f;
//    }

//    public override List<ElementType> SynergyWith { get; } = new() { };
//    public override List<ElementType> IncompatibleWith { get; } = new() {  };

//    public override AttackType GetDominantAttack() => AttackType.Thunder;

//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();

//        return comp;
//    }
//}
//public class FireNode : ElementNode // 1
//{
//    public override ElementType NodeType => ElementType.Fire;

//    public FireNode()
//    {
//        Damage = 23f;
//        Range = 10f;
//        Speed = 9f;
//        Weight = 1.23f;
//        PrimaryColor = new Color(0.98f, 0.39f, 0.0f);
//        EmissionIntensity = 1.4f;
//        TrailLength = 1.3f;
//        ParticleSize = 1.2f;
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Air, ElementType.Lightning };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        { ElementType.Ice, ElementType.Earth };

//    public override AttackType GetDominantAttack() => AttackType.Stream;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Fire, 1.0f);
//        comp.Add(AttackType.Stream, 1.0f);
//        comp.Add(StatusEffectType.Burn, 0.8f);
//        return comp;
//    }
//}

//public class WaterNode : ElementNode // 2
//{
//    public override ElementType NodeType => ElementType.Water;

//    public WaterNode()
//    {
//        Damage = 17f;
//        Range = 8f;
//        Speed = 9f;
//        Weight = 1.3f;
//        PrimaryColor = new Color(0.35f, 0.82f, 0.95f); 
//        EmissionIntensity = 0.8f;
//        TrailLength = 1.1f;
//        ParticleSize = 0.9f;
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Ice, ElementType.Poison, ElementType.Lightning, ElementType.Earth };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        {  };

//    public override AttackType GetDominantAttack() => AttackType.Spray;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Spray, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class EartNode : ElementNode // 3
//{
//    public override ElementType NodeType => ElementType.Earth;

//    public EartNode()
//    {
//        Damage = 30f;
//        Range = 25f;
//        Speed = 9f;
//        Weight = 1.2f;
//        PrimaryColor = new Color(0.71f, 0.38f, 0.13f); // коричневый
//        EmissionIntensity = 0.6f;
//        TrailLength = 0.7f;
//        ParticleSize = 1.5f;   // крупные обломки
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Poison, ElementType.Air, ElementType.Cold };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        { ElementType.Air, ElementType.Lightning };

//    public override AttackType GetDominantAttack() => AttackType.Ball;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Ball, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class AirNode : ElementNode // 4
//{
//    public override ElementType NodeType => ElementType.Air;

//    public AirNode()
//    {
//        Damage = 10f;
//        Range = 30f;
//        Speed = 9f;
//        Weight = 1.3f;
//        PrimaryColor = new Color(0.94f, 1.0f, 1.0f);   // бледно-голубой
//        EmissionIntensity = 0.9f;
//        TrailLength = 1.5f;   // длинный след у воздуха
//        ParticleSize = 0.6f;   // мелкие частицы
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Fire, ElementType.Lightning, ElementType.Water };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        { ElementType.Earth, ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Stream;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Air, 1.0f);
//        comp.Add(AttackType.Stream, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class ColdNode : ElementNode // 4
//{
//    public override ElementType NodeType => ElementType.Cold;

//    public ColdNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 9f;
//        Weight = 1.4f;
//        PrimaryColor = new Color(0.31f, 0.85f, 0.89f);   // бледно-голубой
//        EmissionIntensity = 0.7f;
//        TrailLength = 1.5f;   // длинный след у воздуха
//        ParticleSize = 0.8f;   // мелкие частицы
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Fire, ElementType.Lightning };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        { ElementType.Earth };

//    public override AttackType GetDominantAttack() => AttackType.Spray;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Air, 1.0f);
//        comp.Add(AttackType.Spray, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class LightningNode : ElementNode // 5
//{
//    public override ElementType NodeType => ElementType.Lightning;

//    public LightningNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 9f;
//        Weight = 1.2f;
//        PrimaryColor      = new Color(0.93f, 0.91f, 0.27f);    // электрический бело-голубой
//        EmissionIntensity = 1.8f;   // самый яркий
//        TrailLength       = 1.6f;
//        ParticleSize      = 0.7f;
//    }

//    public override List<ElementType> SynergyWith { get; } = new()
//        { ElementType.Water };
//    public override List<ElementType> IncompatibleWith { get; } = new()
//        { ElementType.Earth, ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Thunder;
//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Lightning, 1.0f);
//        comp.Add(AttackType.Thunder, 1.0f);
//        comp.Add(AttackType.Spray, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}


//// элемента 2 уровня (только свмещением первых)
//public class SteamNode : ElementNode
//{
//    public override ElementType NodeType => ElementType.Steam;

//    public SteamNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 9f;
//        Weight = 1.3f;
//        PrimaryColor = new Color(0.95f, 0.95f, 0.95f); 
//        EmissionIntensity = 0.9f;
//        TrailLength = 1.5f;  
//        ParticleSize = 0.6f;  
//    }

//    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
//    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Stream;

//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Stream, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class IceNode : ElementNode
//{
//    public override ElementType NodeType => ElementType.Ice;

//    public IceNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 1.2f;
//        Weight = 1.5f;
//        PrimaryColor = new Color(0.36f, 0.45f, 0.90f);  
//        EmissionIntensity = 0.9f;
//        TrailLength = 1.5f; 
//        ParticleSize = 0.6f;  
//    }

//    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
//    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Spike;

//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Spike, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class MudNode : ElementNode
//{
//    public override ElementType NodeType => ElementType.Mud;

//    public MudNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 1.2f;
//        Weight = 1.5f;
//        PrimaryColor = new Color(0.51f, 0.45f, 0.05f);
//        EmissionIntensity = 0.9f;
//        TrailLength = 1.5f; 
//        ParticleSize = 0.6f; 
//    }

//    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
//    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Spray;

//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Spray, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

//public class PlasmaNode : ElementNode
//{
//    public override ElementType NodeType => ElementType.Plasma;

//    public PlasmaNode()
//    {
//        Damage = 15f;
//        Range = 8f;
//        Speed = 1.2f;
//        Weight = 0.5f;
//        PrimaryColor = new Color(1.0f, 0.68f, 0.02f); 
//        EmissionIntensity = 0.9f;
//        TrailLength = 1.5f;
//        ParticleSize = 0.6f;
//    }

//    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
//    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

//    public override AttackType GetDominantAttack() => AttackType.Beam;

//    public override NodeComposition GetBaseComposition()
//    {
//        var comp = new NodeComposition();
//        comp.Add(ElementType.Water, 1.0f);
//        comp.Add(AttackType.Beam, 1.0f);
//        comp.Add(StatusEffectType.Slow, 0.6f);
//        return comp;
//    }
//}

////элемнты 3 уровня 
///

using System.Collections.Generic;
using UnityEngine;
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
    public override List<ElementType> IncompatibleWith { get; } = new() { };

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
        Damage = 12f;
        Range = 7f;
        Speed = 5f;
        Weight = 1.12f;
        PrimaryColor = new Color(0.98f, 0.39f, 0.0f);
        EmissionIntensity = 1.4f;
        TrailLength = 1.3f;
        ParticleSize = 1.2f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Air, ElementType.Lightning, ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Ice, ElementType.Earth };

    public override AttackType GetDominantAttack() => AttackType.Stream;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Fire, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Burn, 0.8f);
        return comp;
    }
}

public class WaterNode : ElementNode // 2
{
    public override ElementType NodeType => ElementType.Water;

    public WaterNode()
    {
        Damage = 8f;
        Range = 4f;
        Speed = 7f;
        Weight = 1.73f;
        PrimaryColor = new Color(0.35f, 0.82f, 0.95f);
        EmissionIntensity = 0.8f;
        TrailLength = 1.1f;
        ParticleSize = 0.9f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Ice, ElementType.Poison, ElementType.Lightning, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new()
    { };

    public override AttackType GetDominantAttack() => AttackType.Spray;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class EartNode : ElementNode // 3
{
    public override ElementType NodeType => ElementType.Earth;

    public EartNode()
    {
        Damage = 14f;
        Range = 12f;
        Speed = 12f;
        Weight = 1.14f;
        PrimaryColor = new Color(0.71f, 0.38f, 0.13f); // коричневый
        EmissionIntensity = 0.6f;
        TrailLength = 0.7f;
        ParticleSize = 1.5f;   // крупные обломки
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Poison, ElementType.Air, ElementType.Cold };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Lightning };

    public override AttackType GetDominantAttack() => AttackType.Ball;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Ball, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class AirNode : ElementNode // 4
{
    public override ElementType NodeType => ElementType.Air;

    public AirNode()
    {
        Damage = 5f;
        Range = 7f;
        Speed = 18f;
        Weight = 1.20f;
        PrimaryColor = new Color(0.94f, 1.0f, 1.0f);   // бледно-голубой
        EmissionIntensity = 0.9f;
        TrailLength = 1.5f;   // длинный след у воздуха
        ParticleSize = 0.6f;   // мелкие частицы
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Fire, ElementType.Lightning, ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Stream;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Air, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class ColdNode : ElementNode // 4
{
    public override ElementType NodeType => ElementType.Cold;

    public ColdNode()
    {
        Damage = 15f;
        Range = 8f;
        Speed = 6f;
        Weight = 1.08f;
        PrimaryColor = new Color(0.31f, 0.85f, 0.89f);   // бледно-голубой
        EmissionIntensity = 0.7f;
        TrailLength = 1.5f;   // длинный след у воздуха
        ParticleSize = 0.8f;   // мелкие частицы
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Lightning };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Fire,ElementType.Earth };

    public override AttackType GetDominantAttack() => AttackType.Spray;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Air, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class LightningNode : ElementNode // 5
{
    public override ElementType NodeType => ElementType.Lightning;

    public LightningNode()
    {
        Damage = 16f;
        Range = 4f;
        Speed = 7f;
        Weight = 1.02f;
        PrimaryColor = new Color(0.93f, 0.91f, 0.27f);    // электрический бело-голубой
        EmissionIntensity = 1.8f;   // самый яркий
        TrailLength = 1.6f;
        ParticleSize = 0.7f;
    }

    public override List<ElementType> SynergyWith { get; } = new()
        { ElementType.Water };
    public override List<ElementType> IncompatibleWith { get; } = new()
        { ElementType.Earth, ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Thunder;
    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Lightning, 1.0f);
        comp.Add(AttackType.Thunder, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}


// элемента 2 уровня (только совмещением первых)
public class SteamNode : ElementNode
{
    public override ElementType NodeType => ElementType.Steam;

    public SteamNode()
    {
        Damage = 10f;
        Range = 8f;
        Speed = 19f;
        Weight = 1.09f;
        PrimaryColor = new Color(0.95f, 0.95f, 0.95f);
        EmissionIntensity = 0.9f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Stream;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Stream, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class IceNode : ElementNode
{
    public override ElementType NodeType => ElementType.Ice;

    public IceNode()
    {
        Damage = 13f;
        Range = 9f;
        Speed = 7f;
        Weight = 1.09f;
        PrimaryColor = new Color(0.36f, 0.45f, 0.90f);
        EmissionIntensity = 0.9f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spike;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spike, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class MudNode : ElementNode
{
    public override ElementType NodeType => ElementType.Mud;

    public MudNode()
    {
        Damage = 12f;
        Range = 8f;
        Speed = 9f;
        Weight = 1.10f;
        PrimaryColor = new Color(0.51f, 0.45f, 0.05f);
        EmissionIntensity = 0.9f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Spray;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Spray, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}

public class PlasmaNode : ElementNode
{
    public override ElementType NodeType => ElementType.Plasma;

    public PlasmaNode()
    {
        Damage = 16f;
        Range = 4f;
        Speed = 7f;
        Weight = 1.02f;
        PrimaryColor = new Color(1.0f, 0.68f, 0.02f);
        EmissionIntensity = 0.9f;
        TrailLength = 1.5f;
        ParticleSize = 0.6f;
    }

    public override List<ElementType> SynergyWith { get; } = new() { ElementType.Fire, ElementType.Earth };
    public override List<ElementType> IncompatibleWith { get; } = new() { ElementType.Ice };

    public override AttackType GetDominantAttack() => AttackType.Beam;

    public override NodeComposition GetBaseComposition()
    {
        var comp = new NodeComposition();
        comp.Add(ElementType.Water, 1.0f);
        comp.Add(AttackType.Beam, 1.0f);
        comp.Add(StatusEffectType.Slow, 0.6f);
        return comp;
    }
}