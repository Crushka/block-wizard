using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamProjectile : ContinuousBeam
{
    [Header("Визуал луча")]
    public float StartWidth = 0.05f;
    public float EndWidth = 0.02f;
    public Color BeamColor = new Color(0.4f, 0.8f, 1f, 1f);

    [Header("Мерцание (пульсация ширины)")]
    public bool EnablePulse = true;
    public float PulseSpeed = 12f;
    public float PulseAmount = 0.02f;

    private LineRenderer _line;
    [SerializeField] private GameObject _impactInstance;

    protected override void Awake()
    {
        base.Awake();
        _line = GetComponent<LineRenderer>();
        SetupLine();
    }

    // Реализуем абстрактный метод родителя
    public override void SetupVisual(NodeBase node)
    {
        if (node == null) return;

        BeamColor = node.PrimaryColor;
        StartWidth = 0.03f + 0.03f * node.EmissionIntensity;
        EndWidth = 0.01f + 0.01f * node.EmissionIntensity;

        // Если материал уже задан в инспекторе, эту строку можно убрать
        _line.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        SetupLine();

        var light = GetComponent<Light>();
        if (light != null)
        {
            light.color = node.PrimaryColor;
            light.intensity = node.EmissionIntensity * 2f;
        }
    }

    // Реализуем абстрактный метод родителя для обновления визуала
    protected override void OnUpdateVisual(Vector3 start, Vector3 end)
    {
        _line.SetPosition(0, start);
        _line.SetPosition(1, end);

        if (EnablePulse)
        {
            float pulse = Mathf.Sin(Time.time * PulseSpeed) * PulseAmount;
            _line.startWidth = StartWidth + pulse;
            _line.endWidth = EndWidth + pulse * 0.5f;
        }

        if (_impactInstance != null)
        {
            _impactInstance.transform.position = end;
            Vector3 dir = (start - end).normalized;
            if (dir != Vector3.zero)
                _impactInstance.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void SetupLine()
    {
        _line.positionCount = 2;
        _line.useWorldSpace = true;
        _line.startWidth = StartWidth;
        _line.endWidth = EndWidth;
        _line.startColor = BeamColor;
        _line.endColor = new Color(BeamColor.r, BeamColor.g, BeamColor.b, 0f);
        _line.numCapVertices = 4;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
    }
}