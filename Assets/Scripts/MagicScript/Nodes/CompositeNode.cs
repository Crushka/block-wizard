using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CompositeNode : NodeBase
{
    private readonly NodeComposition _composition;

    public override ElementType NodeType => ElementType.Unknown;
    public override List<ElementType> SynergyWith { get; } = new();
    public override List<ElementType> IncompatibleWith { get; } = new();

    public CompositeNode(NodeComposition composition, NodeBase node1, NodeBase node2)
    {
        _composition = composition;

        Weight = (node1.Weight + node2.Weight) * 0.5f;
        Damage = (node1.Damage + node2.Damage) * Weight;
        Range = (node1.Range + node2.Range) * Weight;
        Speed = (node1.Speed + node2.Speed) * Weight;
        // UnityEngine.Debug.Log($"dma1: {node1.Damage} dmg2: {node2.Damage}");
        
    }

    public override AttackType GetDominantAttack() => _composition.GetDominantAttack();

    public override NodeComposition GetBaseComposition() => _composition;
}