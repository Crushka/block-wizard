using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private AttackFactory attackFactory;
    [SerializeField] private Transform spawnPoint;

    private IAttack _currentAttack;

    void Start()
    {
        PrepareSpell(BuildTestGraph());
    }

    public void PrepareSpell(SpellGraph graph)
    {
        NodeBase result = SpellGraphSolver.SlowGraph(graph);
        _currentAttack = attackFactory.GetAttack(result);
    }

    public void Cast() => _currentAttack?.Cast(spawnPoint);
    public void StopCast() => _currentAttack?.Stop();

    private SpellGraph BuildTestGraph()
    {
        var graph = new SpellGraph();
        graph.StartNode = graph.CreateNode(new NoneElement() { Weight = 1.0f }, 0, 1f);

        var hub = graph.CreateNode(new WaterNode() { Weight = 2.3f }, 1, 2.3f);

        var branchA = graph.CreateNode(new WaterNode() { Weight = 1.8f }, 2, 1.8f);
        var branchB = graph.CreateNode(new WaterNode() { Weight = 1.8f }, 2, 1.8f);
        var branchC = graph.CreateNode(new WaterNode() { Weight = 1.4f }, 3, 1.4f);

        var nodeD = graph.CreateNode(new WaterNode() { Weight = 2f }, 4, 2f);

        graph.Connect(graph.StartNode, hub);

        graph.Connect(hub, branchA);
        graph.Connect(hub, branchB);

        graph.Connect(branchA, branchC);
        graph.Connect(branchB, branchC);
        //graph.Connect(branchA, nodeD);
        graph.Connect(branchC, nodeD);

        graph.CalculateWeight();


        return graph;
    }

   
}