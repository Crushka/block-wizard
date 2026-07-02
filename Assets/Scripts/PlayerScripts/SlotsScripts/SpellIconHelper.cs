// SpellIconHelper.cs
// Generates a compact two-line label for a spell slot icon.
//
// Line 1: dominant attack type  (e.g. "Stream", "Thunder")
// Line 2: element tag + damage  (e.g. "🔥 47" / "Mix 83" / "Ice 120")
//
// Kept as a pure static helper so any UI component can call it without dependencies.

using UnityEngine;

public static class SpellIconHelper
{
    // Short localised names for AttackType
    private static string AttackLabel(AttackType t) => t switch
    {
        AttackType.Spray => "Spray",
        AttackType.Ball => "Ball",
        AttackType.Thunder => "Thunder",
        AttackType.Stream => "Stream",
        AttackType.CircularWave => "Wave",
        AttackType.Spike => "Spike",
        AttackType.Beam => "Beam",
        _ => "???"
    };

    // One-word element tag with an emoji prefix where it fits on a tiny icon
    private static string ElementTag(ElementType t) => t switch
    {
        ElementType.Fire => "🔥",
        ElementType.Water => "💧",
        ElementType.Earth => "🪨",
        ElementType.Air => "💨",
        ElementType.Cold => "❄",
        ElementType.Lightning => "⚡",
        ElementType.Steam => "♨",
        ElementType.Ice => "Ice",
        ElementType.Plasma => "Plsm",
        ElementType.Unknown => "Mix",   // CompositeNode – blended spell
        ElementType.None => "—",
        _ => "?"
    };

    /// <summary>
    /// Returns a compact label for a TMP text component on a spell slot icon.
    /// Returns an empty string when <paramref name="node"/> is null.
    /// </summary>
    public static string GetIconText(NodeBase node)
    {
        if (node == null) return string.Empty;

        string attackLine = AttackLabel(node.GetDominantAttack());
        string elementTag = ElementTag(node.NodeType);
        int damage = Mathf.RoundToInt(node.Damage);

        return $"{attackLine}\n{elementTag} {damage}";
    }

    /// <summary>
    /// Same as <see cref="GetIconText"/> but with a third line showing range.
    /// Useful if the slot icon is tall enough.
    /// </summary>
    public static string GetIconTextExtended(NodeBase node)
    {
        if (node == null) return string.Empty;

        string attackLine = AttackLabel(node.GetDominantAttack());
        string elementTag = ElementTag(node.NodeType);
        int damage = Mathf.RoundToInt(node.Damage);
        int range = Mathf.RoundToInt(node.Range);

        return $"{attackLine}\n{elementTag} ⚔{damage}\n↔{range}";
    }
}