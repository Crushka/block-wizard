using System.Collections.Generic;
using System.Linq;



[System.Serializable]
public class NodeRecipe
{
    private static readonly List<AttackType> AttackPriority = new List<AttackType>
    {
        AttackType.Ball,
        AttackType.Spray,
        AttackType.Spike,
        AttackType.Beam,
        AttackType.Stream,
        AttackType.Thunder
    };

    public Dictionary<ElementType, float> Elements { get; } = new();
    public Dictionary<AttackType, float> AttackTypes { get; } = new();
    public Dictionary<StatusEffectType, float> Effects { get; } = new();

    public void Add(ElementType type, float weight) => AddToDict(Elements, type, weight);
    public void Add(AttackType type, float weight) => AddToDict(AttackTypes, type, weight);
    public void Add(StatusEffectType type, float weight) => AddToDict(Effects, type, weight);

    private void AddToDict<T>(Dictionary<T, float> dict, T key, float weight) where T : struct
    {
        dict.TryGetValue(key, out float current);
        dict[key] = current + weight;
    }
 
    public void Normalize()
    {
        NormalizeDict(Elements);
        NormalizeDict(AttackTypes);
        NormalizeDict(Effects);
    }

    public void Threshold()
    {
        float minValue = 0.32f;
        ThresholdDict(Elements, minValue);
        ThresholdDict(AttackTypes, minValue);
        ThresholdDict(Effects, minValue);
    }

    private void NormalizeDict<T>(Dictionary<T, float> dict)
    {
        float total = 0;
        foreach (var v in dict.Values) total += v;
        if (total <= 0) return;
        var keys = new List<T>(dict.Keys);
        foreach (var k in keys) dict[k] /= total;
    }

    private void ThresholdDict<T>(Dictionary<T, float> dict, float minValue)
    {
        var keys = new List<T>(dict.Keys);
        foreach (var k in keys)
            if (dict[k] < minValue)
                dict.Remove(k);
        Normalize();
    }

    public static T GetDominant<T>(Dictionary<T, float> dict, T fallback, List<T> priority) where T : struct
    {
        T best = fallback;
        float max = -1;
        int bestPriority = int.MaxValue;

        foreach (var kvp in dict)
        {
            int currentPriority = priority.IndexOf(kvp.Key);
            if (currentPriority == -1) currentPriority = int.MaxValue;

            if (kvp.Value > max || (kvp.Value == max && currentPriority < bestPriority))
            {
                max = kvp.Value;
                best = kvp.Key;
                bestPriority = currentPriority;
            }
        }
        return best;
    }

    public AttackType GetDominantAttack() => GetDominant(AttackTypes, AttackType.Ball, AttackPriority);
    public StatusEffectType GetFirstEffect()
    {
        if (Effects.Count == 0) return StatusEffectType.Slow;
        return Effects.Keys.First();
    }

    public static NodeRecipe Merge(NodeRecipe a, NodeRecipe b)
    {
        var result = new NodeRecipe();

        foreach (var kvp in a.Elements) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.Elements) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in a.AttackTypes) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.AttackTypes) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in a.Effects) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.Effects) result.Add(kvp.Key, kvp.Value);

        result.Normalize();
        return result;
    }

    public NodeRecipe Clone()
    {
        var copy = new NodeRecipe();
        foreach (var kvp in Elements) copy.Elements[kvp.Key] = kvp.Value;
        foreach (var kvp in AttackTypes) copy.AttackTypes[kvp.Key] = kvp.Value;
        foreach (var kvp in Effects) copy.Effects[kvp.Key] = kvp.Value;
        return copy;
    }
}