using UnityEngine;

public class SpellGraphTester : MonoBehaviour
{
    [ContextMenu(" Run Simple Cycle Test")]
    public void RunSimpleCycleTest()
    {
        try
        {
            var graph = new SpellGraph();
            graph.StartNode = graph.CreateNode(new NoneElement() { Weight = 1.0f});

            var hub = graph.CreateNode(new WaterNode() { Weight = 0.2f });

            var branchA = graph.CreateNode(new AirNode() { Weight = 0.3f });
            var branchB = graph.CreateNode(new WaterNode() { Weight = 0.5f });
            var branchC = graph.CreateNode(new LightningNode() { Weight = 0.7f });

            var nodeD = graph.CreateNode(new FireNode() { Weight = 0.4f });
            var nodeE = graph.CreateNode(new WaterNode() { Weight = 0.2f });
            var nodeF = graph.CreateNode(new AirNode() { Weight = 0.3f });
            var nodeG = graph.CreateNode(new EartNode() { Weight = 0.5f });
            var nodeH = graph.CreateNode(new LightningNode() { Weight = 1.7f });
            var nodeI = graph.CreateNode(new FireNode() { Weight = 0.4f });

            graph.Connect(graph.StartNode, hub);

            graph.Connect(hub, branchB);

            graph.Connect(branchA, nodeD);
            graph.Connect(branchA, nodeE);
            graph.Connect(branchA, branchB); 

            graph.Connect(branchB, nodeF);
            graph.Connect(branchB, nodeG);
            graph.Connect(branchB, nodeE); 

            graph.Connect(branchC, nodeH);
            graph.Connect(branchC, nodeI);
            graph.Connect(nodeH, nodeI);

            graph.Connect(nodeD, nodeG);
            graph.Connect(nodeE, nodeF);
            graph.Connect(nodeF, nodeH);



            var result = SpellGraphSolver.SlowGraph(graph);

            Debug.Log($"Итоговое заклинание: {result.NodeType}");
            Debug.Log($"Damage: {result.Damage:F1} |  Range: {result.Range:F1} | Speed: {result.Speed:F1}");

        
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Алгоритм упал: {e.Message}\n{e.StackTrace}");
        }
    }

    
}