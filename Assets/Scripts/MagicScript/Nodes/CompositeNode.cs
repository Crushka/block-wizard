using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CompositeNode : NodeBase
{
    private readonly NodeRecipe _composition;

    public override ElementType NodeType => ElementType.Unknown;
    public override List<ElementType> SynergyWith { get; } = new();
    public override List<ElementType> IncompatibleWith { get; } = new();

    public CompositeNode(NodeRecipe composition, NodeBase node1, NodeBase node2)
    {
        _composition = composition;

        Weight = MixAlgorithms.WeightFormula(node1.Weight, node2.Weight);
        Damage = MixAlgorithms.Formula(node1.Damage, node2.Damage, Weight);
        Range = MixAlgorithms.Formula(node1.Range, node2.Range, Weight);
        Speed = MixAlgorithms.Formula(node1.Speed, node2.Speed, Weight);

        NodeVisualMixer.MixVisuals(this, node1, node2, node1.Weight, node2.Weight);
    }
    private CompositeNode(NodeRecipe composition)
    {
        _composition = composition;
    }

    public override AttackType GetDominantAttack() => _composition.GetDominantAttack();

    public override NodeRecipe GetBaseComposition() => _composition;

    public override NodeBase Clone()
    {
        var copy = new CompositeNode(_composition.Clone());
        CopyFieldsTo(copy);
        return copy;
    }
}