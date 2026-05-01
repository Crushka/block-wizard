using System.Collections.Generic;

public class CompositeNode : NodeBase
{
    private readonly NodeComposition _composition;

    public override ElementType NodeType => ElementType.Unknown;
    public override List<ElementType> SynergyWith { get; } = new();
    public override List<ElementType> IncompatibleWith { get; } = new();

    public CompositeNode(NodeComposition composition, NodeBase node1, NodeBase node2)
    {
        _composition = composition;

        Damage = (node1.Damage + node2.Damage) / 2f;
        Range = (node1.Range + node2.Range) / 2f;
        Speed = (node1.Speed + node2.Speed) / 2f;
        Weight = (node1.Weight + node2.Weight) / 2f;
    }

    public override NodeComposition GetBaseComposition() => _composition;
}