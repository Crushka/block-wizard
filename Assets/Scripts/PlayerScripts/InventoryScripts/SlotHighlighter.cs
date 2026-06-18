using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SlotHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Цвета")]
    [Tooltip("Цвет подсветки при наведении курсора")]
    public Color highlightColor = new Color(0.8f, 0.95f, 1f, 1f);

    [Tooltip("Цвет при нажатии на ячейку")]
    public Color pressedColor = new Color(0.6f, 0.85f, 1f, 1f);

    [Header("Анимация")]
    [Tooltip("Скорость перехода между цветами (больше = быстрее)")]
    [Range(1f, 20f)]
    public float transitionSpeed = 8f;

    [Tooltip("Включить плавный переход цвета")]
    public bool smoothTransition = true;

    private Image _image;
    private Color _defaultColor;
    private Color _targetColor;
    private bool _isHovered = false;

    public System.Action<SlotHighlight> OnSlotClicked;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _defaultColor = _image.color;
        _targetColor = _defaultColor;
    }

    private void Update()
    {
        if (!smoothTransition) return;

        if (_image.color != _targetColor)
        {
            _image.color = Color.Lerp(_image.color, _targetColor, Time.deltaTime * transitionSpeed);

            if (ColorDistance(_image.color, _targetColor) < 0.001f)
                _image.color = _targetColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        SetColor(highlightColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        SetColor(_defaultColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _image.color = pressedColor;
        _targetColor = _isHovered ? highlightColor : _defaultColor;

        OnSlotClicked?.Invoke(this);
    }

    public void ResetHighlight()
    {
        _isHovered = false;
        SetColor(_defaultColor);
    }

    public void SetDefaultColor(Color newDefault)
    {
        _defaultColor = newDefault;
        if (!_isHovered)
            SetColor(_defaultColor);
    }

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