using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

internal class MixAlgorithms
{
     
    static private void CheckSynergyAndIncrease(NodeBase node1, NodeBase node2)
    {
        bool syn1 = node1.SynergyWith.Contains(node2.NodeType);
        bool syn2 = node2.SynergyWith.Contains(node1.NodeType);

        if (syn1 && syn2) { IncreaseValue(node1, 1.08f); IncreaseValue(node2, 1.08f); }
        else if (syn1) { IncreaseValue(node1, 1.14f); IncreaseValue(node2, 1.08f); }
        else if (syn2) { IncreaseValue(node1, 1.05f); IncreaseValue(node2, 1.14f); }
    }
    static private void CheckIncompatibleAndIncrease(NodeBase node1, NodeBase node2)
    {
        bool syn1 = node1.IncompatibleWith.Contains(node2.NodeType);
        bool syn2 = node2.IncompatibleWith.Contains(node1.NodeType);

        if (syn1 && syn2) { IncreaseValue(node1, 0.93f); IncreaseValue(node2, 0.93f); }
        else if (syn1) { IncreaseValue(node1, 0.87f); IncreaseValue(node2, 0.95f); }
        else if (syn2) { IncreaseValue(node1, 0.95f); IncreaseValue(node2, 0.87f); }
    }
    static public void IncreaseValue(NodeBase node, float k)
    {
        node.Damage *= k;
        node.Range *= k;
        node.Speed *= k;
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
    private static readonly Dictionary<(ElementType, ElementType), Func<NodeBase>> Recipes =
        new Dictionary<(ElementType, ElementType), Func<NodeBase>>
    {
        { Order(ElementType.Fire, ElementType.Water), ()    => new SteamNode(1) },
        { Order(ElementType.Water, ElementType.Cold), ()    => new IceNode(1) },
        { Order(ElementType.Air, ElementType.Lightning), () => new SoundNode(1) },
        { Order(ElementType.Air, ElementType.Water), ()     => new FogNode(1) },
        { Order(ElementType.Fire, ElementType.Earth), ()    => new LavaNode(1) },
        { Order(ElementType.Lava, ElementType.Sound), ()    => new PlasmaNode(1) },
        { Order(ElementType.Water, ElementType.Steam), ()   => new AcidNode(1) },
    };
    private static (ElementType, ElementType) Order(ElementType a, ElementType b)
        => a < b ? (a, b) : (b, a);
    
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
            return MergeStats(node1, node2, result, false);
        }

        return null;
    }

    static public float Formula(float a, float b, float weight) { return Mathf.Sqrt(a * b) * weight; }
    static public float WeightFormula(float a, float b) { return Mathf.Lerp(Mathf.Sqrt(a * b), 2.0f, 0.15f); }
    private static NodeBase MergeStats(NodeBase n1, NodeBase n2, NodeBase target, bool mixVisuals = true)
    {
        target.Weight = WeightFormula(n1.Weight, n2.Weight);
        target.Damage = Formula(n1.Damage, n2.Damage, target.Weight);
        target.Range = Formula(n1.Range, n2.Range, target.Weight);
        target.Speed = Formula(n1.Speed, n2.Speed, target.Weight);
        if (mixVisuals)
        {
            NodeVisualMixer.MixVisuals(target, n1, n2, n1.Weight, n2.Weight);
        }
        
        return target;
    }

    static public float CountBuff(List<GraphNode> nodes)
    {
        int synergies = 0, conflicts = 0;
        for (int i = 0; i < nodes.Count - 1; i++)
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (ChekSig(nodes[i], nodes[j])) synergies++;
                else if (ChekIng(nodes[i], nodes[j])) conflicts++;
            }

        float buff = 1.0f + 0.2f * Mathf.Log(1 + synergies)
                          - 0.15f * Mathf.Log(1 + conflicts);
        return Mathf.Clamp(buff, 0.1f, 1.8f);
    }

    static public NodeBase MixNodes(NodeBase node1, NodeBase node2)
    {
        NodeBase workpiece = CheckWorkpiece(node1, node2);
        if (workpiece != null)
            return workpiece;


        CheckSynergyAndIncrease(node1, node2);
        CheckIncompatibleAndIncrease(node1, node2);
        var comp = NodeRecipe.Merge(
            node1.GetBaseComposition(),
            node2.GetBaseComposition()
        );

        return new CompositeNode(comp, node1, node2);
    }


}