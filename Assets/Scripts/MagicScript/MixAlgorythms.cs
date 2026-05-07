using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

internal class MixAlgorithms
{

    private static readonly Dictionary<(ElementType, ElementType), Func<NodeBase>> Recipes =
        new Dictionary<(ElementType, ElementType), Func<NodeBase>>
    {
        { Order(ElementType.Fire, ElementType.Water), () => new SteamNode() },
        { Order(ElementType.Water, ElementType.Earth), () => new MudNode() },
        { Order(ElementType.Fire, ElementType.Air), () => new PlasmaNode() }
    };

    private static (ElementType, ElementType) Order(ElementType a, ElementType b)
        => a < b ? (a, b) : (b, a);

    static public void IncreaseValue(NodeBase node1, NodeBase node2, float k)
    {
        if (node1 != null)
        {
            node1.Damage *= k;
            node1.Range *= k;
            node1.Speed *= k;
        }

        if (node2 != null)
        {
            node2.Damage *= k;
            node2.Range *= k;
            node2.Speed *= k;
        }
    }

    static public NodeBase CheckWorkpiece(NodeBase node1, NodeBase node2)
    {
        var a = node1.NodeType;
        var b = node2.NodeType;

        if (a == ElementType.None) return node2;
        if (b == ElementType.None) return node1;
        if (a == ElementType.Unknown || b == ElementType.Unknown) return null;

        if (a == b)
        {
            return MergeStats(node1, node2, node1);
        }

        var key = Order(a, b);
        if (Recipes.TryGetValue(key, out var createNode))
        {
            NodeBase result = createNode();
            return MergeStats(node1, node2, result);
        }

        return null;
    }

    private static NodeBase MergeStats(NodeBase n1, NodeBase n2, NodeBase target)
    {
        target.Weight = (n1.Weight + n2.Weight) * 0.4f;
        target.Damage = (n1.Damage + n2.Damage) * target.Weight;
        target.Range = (n1.Range + n2.Range) * target.Weight;
        target.Speed = (n1.Speed + n2.Speed) * target.Weight;
        return target;
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
            IncreaseValue(null, node2, 1.5f);
        }
        else if (syn2)
        {
            IncreaseValue(node1, null, 1.5f);
            IncreaseValue(null, node2, 2f);
        }
        else
        {
            IncreaseValue(node1, node2, 0.4f);
        }
    }

    static private bool ChekSig(GraphNode node1, GraphNode node2)
    {
        return node1.Data.SynergyWith.Contains(node2.Data.NodeType) ||
               node2.Data.SynergyWith.Contains(node1.Data.NodeType);
    }
    static private bool ChekIng(GraphNode node1, GraphNode node2)
    {
        return node1.Data.IncompatibleWith.Contains(node2.Data.NodeType) ||
               node2.Data.IncompatibleWith.Contains(node1.Data.NodeType);
    }

    static public float CountBuff(List<GraphNode> nodes)
    {
        float buff = 2f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (ChekSig(nodes[i], nodes[j]))
                    buff += 0.5f;

                else if (ChekIng(nodes[i], nodes[j]))
                    buff -= 0.5f;
            }
        }

        return buff < 0 ? 0.1f : buff ;
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