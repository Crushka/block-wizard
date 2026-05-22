
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
    [SerializeField] private GameObject streamPrefab;
    [SerializeField] private GameObject circleWave;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject beamPrefab;
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
        Debug.Log($"[AttackFactory] dominant attack: {node.GetDominantAttack()}, node type: {node.GetElementType()}");
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
            
            case AttackType.Stream:
                var stream = new Stream();
                stream.Init(node, streamPrefab);
                return stream;

            case AttackType.CircularWave:
                var waveAttack = new CircleWaveAttack();
                waveAttack.Init(node, circleWave);
                return waveAttack;

            case AttackType.Spike:
                var spike = new SpikeAttack();
                spike.Init(node, spikePrefab);
                return spike;

            case AttackType.Beam:
                var beam = new BeamAttack();
                beam.Init(node, beamPrefab);
                return beam;

            default:
                Debug.Log("[AttacFactory] не определённый type атаки");
                var def = new BallAttack();
                def.Init(node, ballPrefab);
                return def;
        }
    }
}