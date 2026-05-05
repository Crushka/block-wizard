using System.Collections.Generic;
using System.Diagnostics;

internal class MixAlgorithms
{
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

    static private NodeBase CheckWorkpiece(NodeBase node1, NodeBase node2)
    {
        

        var a = node1.NodeType;
        var b = node2.NodeType;

        if (a == ElementType.None) return node2;
        if (b == ElementType.None) return node1;
        

        UnityEngine.Debug.Log($"{a} {b}");

        NodeBase result = null;

        if (a == ElementType.Unknown || b == ElementType.Unknown)
            return null;
   
        else if (a == b)
        {
            result = node1;
            IncreaseValue(result, null, 0);
        }
        else if ((a == ElementType.Fire && b == ElementType.Water) ||
            (a == ElementType.Water && b == ElementType.Fire))
            result = new SteamNode();
        else if ((a == ElementType.Water && b == ElementType.Earth) ||
                 (a == ElementType.Earth && b == ElementType.Water))
            result = new MudNode();
        else if ((a == ElementType.Fire && b == ElementType.Air) ||
                 (a == ElementType.Air && b == ElementType.Fire))
            result = new PlasmaNode();

        if (result != null)
        {
            // Переносим накопленные статы вместо дефолтных
            result.Damage = node1.Damage + node2.Damage;
            result.Range = node1.Range + node2.Range;
            result.Speed = (node1.Speed + node2.Speed) * 0.5f;
            result.Weight = node1.Weight + node2.Weight;
        }

        return result;
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
            IncreaseValue(node1, node2, 3f);
        }
    }

    static private bool ChekSig(GraphNode node1, GraphNode node2)
    {
        return node1.Data.SynergyWith.Contains(node2.Data.NodeType) ||
               node2.Data.SynergyWith.Contains(node1.Data.NodeType) ;
    }

    static public float CountBuff(List<GraphNode> nodes)
    {
        float buff = 1f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                buff += ChekSig(nodes[i], nodes[j]) ? 0.25f : -0.4f ;
            }
        }

        return buff ;
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