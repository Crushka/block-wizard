using System.Collections.Generic;

[System.Serializable]
public class NodeComposition
{
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
        float minValue = 0.35f;
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
    }

    public ElementType GetDominantElement() => GetDominant(Elements, ElementType.None);
    public AttackType GetDominantAttack() => GetDominant(AttackTypes, AttackType.Ball);
    public StatusEffectType GetDominantEffect() => GetDominant(Effects, StatusEffectType.Burn);

    public static T GetDominant<T>(Dictionary<T, float> dict, T fallback) where T : struct
    {
        T best = fallback;
        float max = -1;
        foreach (var kvp in dict)
            if (kvp.Value > max) { max = kvp.Value; best = kvp.Key; }
        return best;
    }

    public static NodeComposition Merge(NodeComposition a, NodeComposition b)
    {
        var result = new NodeComposition();

        foreach (var kvp in a.Elements) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.Elements) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in a.AttackTypes) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.AttackTypes) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in a.Effects) result.Add(kvp.Key, kvp.Value);
        foreach (var kvp in b.Effects) result.Add(kvp.Key, kvp.Value);

        result.Normalize();
        return result;
    }
}