
internal class MixAlgorithms
{
    static private void IncreaseValue(NodeBase node1, NodeBase node2, float k)
    {
        if (node1 != null)
        {
            node1.Damage *= k;
            node1.Range *= k;
            node1.Speed *= k;
            node1.Weight *= k;
            node1.TreeWeight *= k;
        }

        if (node2 != null)
        {
            node2.Damage *= k;
            node2.Range *= k;
            node2.Speed *= k;
            node2.Weight *= k;
            node2.TreeWeight *= k;
        }
    }

    static private NodeBase CheckWorkpiece(NodeBase node1, NodeBase node2)
    {
        var a = node1.NodeType;
        var b = node2.NodeType;

        if ((a == ElementType.Fire && b == ElementType.Water) ||
            (a == ElementType.Water && b == ElementType.Fire))
            return new SteamNode();

        if ((a == ElementType.Water && b == ElementType.Earth) ||
            (a == ElementType.Earth && b == ElementType.Water))
            return new MudNode();

        if ((a == ElementType.Fire && b == ElementType.Air) ||
            (a == ElementType.Air && b == ElementType.Fire))
            return new PlasmaNode();

        return null;
    }

    static private void CheckSynergyAndIncrease(NodeBase node1, NodeBase node2)
    {
        bool syn1 = node1.SynergyWith.Contains(node2.NodeType);
        bool syn2 = node2.SynergyWith.Contains(node1.NodeType);

        if (syn1 && syn2)
        {
            IncreaseValue(node1, node2, 1.3f);
        }
        else if (syn1)
        {
            IncreaseValue(node1, null, 2f);
            IncreaseValue(null, node2, 1.3f);
        }
        else if (syn2)
        {
            IncreaseValue(node1, null, 1.3f);
            IncreaseValue(null, node2, 2f);
        }
        else
        {
            IncreaseValue(node1, node2, 0.5f);
        }
    }

    static public NodeBase MixNodes(NodeBase node1, NodeBase node2)
    {
        NodeBase workpiece = CheckWorkpiece(node1, node2);
        if (workpiece != null)
            return workpiece;

        CheckSynergyAndIncrease(node1, node2);

        var comp = NodeComposition.Merge(
            node1.GetBaseComposition(),
            node2.GetBaseComposition()
        );

        return new CompositeNode(comp, node1, node2);
    }
}