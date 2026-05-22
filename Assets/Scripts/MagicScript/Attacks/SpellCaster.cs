using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private AttackFactory attackFactory;
    [SerializeField] private Transform spawnPoint;

    private IAttack _currentAttack;

    public NodeBase CurrentSpellNode { get; private set; }

    public void PrepareSpell(SpellGraph graph)
    {
        NodeBase result = SpellGraphSolver.SlowGraph(graph);
        Debug.Log($"dmg : {result.Damage} | speed : {result.Speed} | range :  {result.Range}");
        CurrentSpellNode = result;
        _currentAttack = attackFactory.GetAttack(result);
    }

    public void PrepareSpellFromNode(NodeBase result)
    {
        Debug.Log($"dmg : {result.Damage} | speed : {result.Speed} | range : {result.Range}");
        CurrentSpellNode = result;
        _currentAttack = attackFactory.GetAttack(result);
    }

    public void PrepareFromEditor()
    {
        var launcher = FindAnyObjectByType<SpellCasterButton>();
        if (launcher == null)
        {
            Debug.LogError("[SpellCaster] SpellCasterButton не найден!");
            return;
        }

        NodeBase result = launcher.CastSpellFromEditor();
        if (result == null)
        {
            Debug.LogError("[SpellCaster] CastSpellFromEditor вернул null!");
            return;
        }

        Debug.Log($"dmg : {result.Damage} | speed : {result.Speed} | range : {result.Range}");
        Debug.Log($"color : {result.PrimaryColor} | emi : {result.EmissionIntensity} | size : {result.ParticleSize}");
        _currentAttack = attackFactory.GetAttack(result);
    }

    public void Cast() => _currentAttack?.Cast(spawnPoint);
    public void StopCast() => _currentAttack?.Stop();




}
