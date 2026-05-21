using UnityEngine;

public class SpellGraphTester : MonoBehaviour
{
    [ContextMenu(" Run Simple Cycle Test")]
    public void RunSimpleCycleTest()
    {
        try
        {
            var graph = new SpellGraph();
            graph.StartNode = graph.CreateNode(new NoneElement() { Weight = 1.0f}, 0, 1f);

            var hub = graph.CreateNode(new WaterNode() { Weight = 2.3f },  1, 2.3f);

            var branchA = graph.CreateNode(new WaterNode() { Weight = 1.8f }, 2, 1.8f);
            var branchB = graph.CreateNode(new EartNode() { Weight = 1.8f }, 2, 1.8f);
            var branchC = graph.CreateNode(new EartNode() { Weight = 1.4f }, 3, 1.4f);

            //var nodeD = graph.CreateNode(new WaterNode() { Weight = 2f }, 4, 2f);
            //var nodeE = graph.CreateNode(new WaterNode() { Weight = 0.4f });
            //var nodeF = graph.CreateNode(new AirNode() { Weight = 0.4f });
            //var nodeG = graph.CreateNode(new EartNode() { Weight = 0.6f });
            //var nodeH = graph.CreateNode(new LightningNode() { Weight = 0.8f });
            //var nodeI = graph.CreateNode(new FireNode() { Weight = 0.5f });

            graph.Connect(graph.StartNode, hub);
            graph.Connect(graph.StartNode, branchA);

            graph.Connect(hub, branchB);

            graph.Connect(branchA, branchC);
            //graph.Connect(branchB, branchC);
            //graph.Connect(branchA, nodeD);
            //graph.Connect(branchC, nodeD);

            //graph.Connect(branchA, nodeD);
            //graph.Connect(branchA, nodeE);
            //graph.Connect(branchA, branchB); 

            //graph.Connect(branchB, nodeF);
            //graph.Connect(branchB, nodeG);
            //graph.Connect(branchB, nodeE); 

            //graph.Connect(branchC, nodeH);
            //graph.Connect(branchC, nodeI);
            //graph.Connect(nodeH, nodeI);

            //graph.Connect(nodeD, nodeG);
            //graph.Connect(nodeE, nodeF);
            //graph.Connect(nodeF, nodeH);

            graph.CalculateWeight();

            var result = SpellGraphSolver.SlowGraph(graph);

            Debug.Log($"вес графа: {graph.Weight}");
            Debug.Log($"итоговое заклинание: {result.NodeType}");
            Debug.Log($"damage: {result.Damage} |  range: {result.Range} | speed: {result.Speed}");
            Debug.Log($" {result.GetBaseComposition()}");

        }
        catch (System.Exception e)
        {
            Debug.LogError($" Алгоритм упал: {e.Message}\n{e.StackTrace}");
        }
    }

    
}