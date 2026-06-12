using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to every HUD mirror slot GameObject alongside InventorySlot.
///
/// HUDQuickSlotSelector calls Activate() / Deactivate() to show which slot
/// is currently selected. Supports optional smooth colour lerp identical to
/// the SlotHighlight approach.
/// </summary>
[RequireComponent(typeof(Image))]
public class HUDSlotActiveIndicator : MonoBehaviour
{
    [Header("Цвета")]
    [Tooltip("Цвет активного (выбранного) слота")]
    public Color activeColor = new Color(1f, 0.85f, 0.2f, 1f);   // золотистый

    [Tooltip("Цвет неактивного слота (обычный фон)")]
    public Color inactiveColor = new Color(1f, 1f, 1f, 1f);

    [Header("Анимация")]
    [Tooltip("Скорость перехода между цветами (больше = быстрее)")]
    [Range(1f, 20f)]
    public float transitionSpeed = 8f;

    [Tooltip("Включить плавный переход цвета")]
    public bool smoothTransition = true;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private Image _image;
    private Color _targetColor;
    private bool _isActive = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _targetColor = inactiveColor;
        _image.color = inactiveColor;
    }

    private void Update()
    {
        if (!smoothTransition) return;
        if (_image.color == _targetColor) return;

        _image.color = Color.Lerp(_image.color, _targetColor, Time.deltaTime * transitionSpeed);

        if (ColorDistance(_image.color, _targetColor) < 0.001f)
            _image.color = _targetColor;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()
    {
        _isActive = true;
        SetColor(activeColor);
    }

    public void Deactivate()
    {
        _isActive = false;
        SetColor(inactiveColor);
    }

    public bool IsActive => _isActive;

    // ── Helper ────────────────────────────────────────────────────────────────

    private void SetColor(Color color)
    {
        _targetColor = color;
        if (!smoothTransition)
            _image.color = color;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) +
               Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a);
    }
}