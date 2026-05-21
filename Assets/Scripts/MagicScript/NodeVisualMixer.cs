using UnityEngine;

public static class NodeVisualMixer
{

    public static void MixVisuals(NodeBase target, NodeBase node1, NodeBase node2,
                                   float weightA = 1f, float weightB = 1f)
    {
        target.PrimaryColor = BlendColorsHSV(node1.PrimaryColor, node2.PrimaryColor, weightA, weightB);
        target.EmissionIntensity = Mathf.Max(node1.EmissionIntensity, node2.EmissionIntensity);
        float total = weightA + weightB;
        target.TrailLength = (node1.TrailLength * weightA + node2.TrailLength * weightB) / total;
        target.ParticleSize = (node1.ParticleSize * weightA + node2.ParticleSize * weightB) / total;
    }

    public static Color BlendColorsHSV(Color a, Color b, float weightA = 1f, float weightB = 1f)
    {
        Color.RGBToHSV(a, out float hA, out float sA, out float vA);
        Color.RGBToHSV(b, out float hB, out float sB, out float vB);

        float total = weightA + weightB;
        float t = weightB / total;

        float delta = hB - hA;
        if (delta > 0.5f) delta -= 1f;
        if (delta < -0.5f) delta += 1f;
        float hBlended = hA + delta * t;
        if (hBlended < 0f) hBlended += 1f;
        if (hBlended > 1f) hBlended -= 1f;

        float sBlended = Mathf.Max(sA, sB);
        float vBlended = Mathf.Max(vA, vB);

        return Color.HSVToRGB(hBlended, sBlended, vBlended);
    }
}