
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AttackFactory : MonoBehaviour
{
    [SerializeField] private GameObject sprayPrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject thunderPrefab;

    private readonly Dictionary<NodeBase, IAttack> _cache = new();

    public IAttack GetAttack(NodeBase node)
    {
        if (_cache.TryGetValue(node, out IAttack cached))
            return cached;

        IAttack attack = ParseNode(node);
        _cache[node] = attack;
        return attack;
    }

    private IAttack ParseNode(NodeBase node)
    {
        Debug.Log($"[AttackFactory] dominant attack: {node.GetDominantAttack()}");
        switch (node.GetDominantAttack())
        {
            case AttackType.Spray:
                var spray = new SprayAttack();
                spray.Init(node, sprayPrefab);
                return spray;

            case AttackType.Ball:
                var ball = new BallAttack();
                ball.Init(node, ballPrefab);
                return ball;

            case AttackType.Thunder:
                var thunder = new ThunderAttack();
                thunder.Init(node, thunderPrefab);
                return thunder;

            default:
                var def = new BallAttack();
                def.Init(node, ballPrefab);
                return def;
        }
    }
}