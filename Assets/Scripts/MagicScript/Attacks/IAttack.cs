using UnityEngine;

public interface IAttack
{
    void Init(NodeBase node, GameObject prefab);
    void Cast(Transform spawnPoint);
    void Stop();
}