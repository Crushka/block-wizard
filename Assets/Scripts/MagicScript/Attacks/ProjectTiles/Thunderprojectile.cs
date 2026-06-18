
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThunderProjectile : ContinuousBeam // Наследуемся от нашего нового класса
{
    [Header("главная молния")]
    public int SegmentCount = 12;
    public float JitterRadius = 0.3f;
    public float JitterSpeed = 0.05f;

    [Header("доп молнии")]
    public int BranchCount = 4;
    public int BranchSegments = 5;
    public float BranchJitter = 0.2f;
    public float BranchLengthFrac = 0.35f;

    [Header("визуал")]
    public float StartWidth = 0.08f;
    public float EndWidth = 0.02f;
    public Color BoltColor = new Color(0.6f, 0.8f, 1f, 1f);

    private LineRenderer _mainLine;
    private List<LineRenderer> _branches = new();
    private float _jitterTimer;

    protected override void Awake()
    {
        base.Awake(); // ОБЯЗАТЕЛЬНО вызываем Awake родителя, чтобы определились маски слоев!

        _mainLine = GetComponent<LineRenderer>();
        SetupLineRenderer(_mainLine, StartWidth, EndWidth, BoltColor);
        _mainLine.positionCount = SegmentCount;

        for (int i = 0; i < BranchCount; i++)
        {
            var go = new GameObject($"Branch_{i}");
            go.transform.SetParent(transform);
            var lr = go.AddComponent<LineRenderer>();
            SetupLineRenderer(lr, StartWidth * 0.4f, 0f, BoltColor * 0.7f);
            lr.positionCount = BranchSegments;
            _branches.Add(lr);
        }
    }

    // Этот метод вызывается автоматически из родительского класса
    protected override void OnUpdateVisual(Vector3 start, Vector3 end)
    {
        _jitterTimer -= Time.deltaTime;
        if (_jitterTimer > 0f) return;
        _jitterTimer = JitterSpeed;

        RebuildMainBolt(start, end);
        RebuildBranches(start, end);
    }

    public override void SetupVisual(NodeBase node)
    {
        BoltColor = node.PrimaryColor;
        StartWidth = 0.05f + 0.04f * node.EmissionIntensity;
        EndWidth = 0.01f + 0.01f * node.EmissionIntensity;
        JitterRadius = 0.2f + 0.15f * node.ParticleSize;

        if (_mainLine != null)
            SetupLineRenderer(_mainLine, StartWidth, EndWidth, BoltColor);

        foreach (var branch in _branches)
            SetupLineRenderer(branch, StartWidth * 0.4f, 0f, BoltColor * 0.7f);

        var light = GetComponent<Light>();
        if (light != null)
        {
            light.color = node.PrimaryColor;
            light.intensity = node.EmissionIntensity * 2f;
        }
    }

    private void RebuildMainBolt(Vector3 start, Vector3 end)
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            float t = (float)i / (SegmentCount - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            if (i > 0 && i < SegmentCount - 1)
                point += RandomPerp(start, end) * JitterRadius * (1f - Mathf.Abs(t - 0.5f) * 2f);

            _mainLine.SetPosition(i, point);
        }
    }

    private void RebuildBranches(Vector3 start, Vector3 end)
    {
        for (int b = 0; b < _branches.Count; b++)
        {
            float rootT = Random.Range(0.2f, 0.8f);
            Vector3 root = Vector3.Lerp(start, end, rootT);

            Vector3 branchDir = (RandomPerp(start, end) + Random.insideUnitSphere * 0.5f).normalized;
            float length = Vector3.Distance(start, end) * BranchLengthFrac;
            Vector3 branchEnd = root + branchDir * length;

            var lr = _branches[b];
            for (int i = 0; i < BranchSegments; i++)
            {
                float t = (float)i / (BranchSegments - 1);
                Vector3 pt = Vector3.Lerp(root, branchEnd, t);
                if (i > 0 && i < BranchSegments - 1)
                    pt += RandomPerp(root, branchEnd) * BranchJitter;
                lr.SetPosition(i, pt);
            }
        }
    }

    private static Vector3 RandomPerp(Vector3 a, Vector3 b)
    {
        Vector3 dir = (b - a).normalized;
        Vector3 arb = Mathf.Abs(dir.x) < 0.9f ? Vector3.right : Vector3.up;
        Vector3 perp = Vector3.Cross(dir, arb).normalized;
        float angle = Random.Range(0f, 360f);
        return Quaternion.AngleAxis(angle, dir) * perp;
    }

    private static void SetupLineRenderer(LineRenderer lr, float startW, float endW, Color color)
    {
        if (lr.material == null || lr.material.shader.name != "Sprites/Default")
            lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, 0f);
        lr.startWidth = startW;
        lr.endWidth = endW;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }
}

//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
//public class ThunderProjectile : ContinuousBeam
//{
//    [Header("главная молния")]
//    public int SegmentCount = 12;
//    public float JitterRadius = 0.3f;
//    public float JitterSpeed = 0.05f;

//    [Header("доп молнии")]
//    public int BranchCount = 4;
//    public int BranchSegments = 5;
//    public float BranchJitter = 0.2f;
//    public float BranchLengthFrac = 0.35f;

//    [Header("визуал")]
//    public float StartWidth = 0.08f;
//    public float EndWidth = 0.02f;
//    public Color BoltColor = new Color(0.6f, 0.8f, 1f, 1f);

//    private LineRenderer _mainLine;
//    private List<LineRenderer> _branches = new();
//    private float _jitterTimer;

//    // Кэш для двумерного смещения (шума)
//    private Vector2[] _mainJitterOffsets;
//    private List<Vector2[]> _branchJitterOffsets = new();
//    private float[] _branchRootT;
//    private Vector2[] _branchDirs;
//    private float[] _branchLengths;

//    protected override void Awake()
//    {
//        base.Awake();

//        _mainLine = GetComponent<LineRenderer>();
//        SetupLineRenderer(_mainLine, StartWidth, EndWidth, BoltColor);
//        _mainLine.positionCount = SegmentCount;

//        for (int i = 0; i < BranchCount; i++)
//        {
//            var go = new GameObject($"Branch_{i}");
//            go.transform.SetParent(transform);
//            var lr = go.AddComponent<LineRenderer>();
//            SetupLineRenderer(lr, StartWidth * 0.4f, 0f, BoltColor * 0.7f);
//            lr.positionCount = BranchSegments;
//            _branches.Add(lr);
//        }

//        // Инициализируем массивы для кэширования шума
//        _mainJitterOffsets = new Vector2[SegmentCount];
//        _branchRootT = new float[BranchCount];
//        _branchDirs = new Vector2[BranchCount];
//        _branchLengths = new float[BranchCount];

//        for (int i = 0; i < BranchCount; i++)
//        {
//            _branchJitterOffsets.Add(new Vector2[BranchSegments]);
//        }

//        GenerateNewJitter();
//    }

//    protected override void OnUpdateVisual(Vector3 start, Vector3 end)
//    {
//        _jitterTimer -= Time.deltaTime;
//        if (_jitterTimer <= 0f)
//        {
//            _jitterTimer = JitterSpeed;
//            GenerateNewJitter(); // Генерируем новый шум только по таймеру
//        }

//        // Перестраиваем молнию под новые start/end каждый кадр
//        RebuildMainBolt(start, end);
//        RebuildBranches(start, end);
//    }

//    private void GenerateNewJitter()
//    {
//        // Шум для основного луча
//        for (int i = 0; i < SegmentCount; i++)
//        {
//            _mainJitterOffsets[i] = Random.insideUnitCircle;
//        }

//        // Настройки и шум для ответвлений
//        for (int b = 0; b < BranchCount; b++)
//        {
//            _branchRootT[b] = Random.Range(0.2f, 0.8f);
//            _branchDirs[b] = Random.insideUnitCircle.normalized;
//            _branchLengths[b] = Random.Range(0.8f, 1.2f);

//            for (int i = 0; i < BranchSegments; i++)
//            {
//                _branchJitterOffsets[b][i] = Random.insideUnitCircle * BranchJitter;
//            }
//        }
//    }

//    private void RebuildMainBolt(Vector3 start, Vector3 end)
//    {
//        Vector3 dir = (end - start).normalized;
//        if (dir == Vector3.zero) return;

//        // Строим перпендикулярный базис на основе направления луча
//        Vector3 right = Mathf.Abs(dir.x) < 0.9f ? Vector3.right : Vector3.up;
//        Vector3 perp1 = Vector3.Cross(dir, right).normalized;
//        Vector3 perp2 = Vector3.Cross(dir, perp1).normalized;

//        for (int i = 0; i < SegmentCount; i++)
//        {
//            float t = (float)i / (SegmentCount - 1);
//            Vector3 point = Vector3.Lerp(start, end, t);

//            if (i > 0 && i < SegmentCount - 1)
//            {
//                Vector2 offset2D = _mainJitterOffsets[i];
//                // Проецируем 2D шум на перпендикулярную плоскость луча в 3D
//                Vector3 jitter = (perp1 * offset2D.x + perp2 * offset2D.y) * JitterRadius * (1f - Mathf.Abs(t - 0.5f) * 2f);
//                point += jitter;
//            }

//            _mainLine.SetPosition(i, point);
//        }
//    }

//    private void RebuildBranches(Vector3 start, Vector3 end)
//    {
//        Vector3 dir = (end - start).normalized;
//        if (dir == Vector3.zero) return;

//        Vector3 right = Mathf.Abs(dir.x) < 0.9f ? Vector3.right : Vector3.up;
//        Vector3 perp1 = Vector3.Cross(dir, right).normalized;
//        Vector3 perp2 = Vector3.Cross(dir, perp1).normalized;

//        for (int b = 0; b < _branches.Count; b++)
//        {
//            float rootT = _branchRootT[b];
//            Vector3 root = Vector3.Lerp(start, end, rootT);

//            Vector2 branchDir2D = _branchDirs[b];
//            // Направляем ветку в сторону и немного вперед по направлению основного луча
//            Vector3 branchDir3D = (perp1 * branchDir2D.x + perp2 * branchDir2D.y + dir * 0.5f).normalized;

//            float length = Vector3.Distance(start, end) * BranchLengthFrac * _branchLengths[b];
//            Vector3 branchEnd = root + branchDir3D * length;

//            // Локальный базис для самой ветки
//            Vector3 bDir = (branchEnd - root).normalized;
//            Vector3 bRight = Mathf.Abs(bDir.x) < 0.9f ? Vector3.right : Vector3.up;
//            Vector3 bPerp1 = Vector3.Cross(bDir, bRight).normalized;
//            Vector3 bPerp2 = Vector3.Cross(bDir, bPerp1).normalized;

//            var lr = _branches[b];
//            for (int i = 0; i < BranchSegments; i++)
//            {
//                float t = (float)i / (BranchSegments - 1);
//                Vector3 pt = Vector3.Lerp(root, branchEnd, t);

//                if (i > 0 && i < BranchSegments - 1)
//                {
//                    Vector2 bOffset2D = _branchJitterOffsets[b][i];
//                    Vector3 bJitter = (bPerp1 * bOffset2D.x + bPerp2 * bOffset2D.y);
//                    pt += bJitter;
//                }
//                lr.SetPosition(i, pt);
//            }
//        }
//    }

//    public override void SetupVisual(NodeBase node)
//    {
//        BoltColor = node.PrimaryColor;
//        StartWidth = 0.05f + 0.04f * node.EmissionIntensity;
//        EndWidth = 0.01f + 0.01f * node.EmissionIntensity;
//        JitterRadius = 0.2f + 0.15f * node.ParticleSize;

//        if (_mainLine != null)
//            SetupLineRenderer(_mainLine, StartWidth, EndWidth, BoltColor);

//        foreach (var branch in _branches)
//            SetupLineRenderer(branch, StartWidth * 0.4f, 0f, BoltColor * 0.7f);

//        var light = GetComponent<Light>();
//        if (light != null)
//        {
//            light.color = node.PrimaryColor;
//            light.intensity = node.EmissionIntensity * 2f;
//        }
//    }

//    private static void SetupLineRenderer(LineRenderer lr, float startW, float endW, Color color)
//    {
//        if (lr.material == null || lr.material.shader.name != "Sprites/Default")
//            lr.material = new Material(Shader.Find("Sprites/Default"));

//        lr.startColor = color;
//        lr.endColor = new Color(color.r, color.g, color.b, 0f);
//        lr.startWidth = startW;
//        lr.endWidth = endW;
//        lr.useWorldSpace = true;
//        lr.numCapVertices = 4;
//        lr.numCornerVertices = 4;
//        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//        lr.receiveShadows = false;
//    }
//}