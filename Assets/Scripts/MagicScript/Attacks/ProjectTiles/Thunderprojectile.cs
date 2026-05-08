using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThunderProjectile : MonoBehaviour
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
    private Vector3 _origin;
    private Vector3 _target;
    private float _jitterTimer;

    void Awake()
    {
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

    public void UpdateBolt(Vector3 origin, Vector3 target)
    {
        _origin = origin;
        _target = target;

        _jitterTimer -= Time.deltaTime;
        if (_jitterTimer > 0f) return;
        _jitterTimer = JitterSpeed;

        RebuildMainBolt();
        RebuildBranches();
    }

    private void RebuildMainBolt()
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            float t = (float)i / (SegmentCount - 1);
            Vector3 point = Vector3.Lerp(_origin, _target, t);

            if (i > 0 && i < SegmentCount - 1)
                point += RandomPerp(_origin, _target) * JitterRadius * (1f - Mathf.Abs(t - 0.5f) * 2f);

            _mainLine.SetPosition(i, point);
        }
    }

    private void RebuildBranches()
    {
        for (int b = 0; b < _branches.Count; b++)
        {
            float rootT = Random.Range(0.2f, 0.8f);
            Vector3 root = Vector3.Lerp(_origin, _target, rootT);

            Vector3 branchDir = (RandomPerp(_origin, _target) + Random.insideUnitSphere * 0.5f).normalized;
            float length = Vector3.Distance(_origin, _target) * BranchLengthFrac;
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