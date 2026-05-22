using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SpellSlotUI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private SpellSlotManager slotManager;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private GameObject slotButtonPrefab;
    [SerializeField] private Button saveButton;

    [Header("Цвета")]
    [SerializeField] private Color colorEmpty = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    [SerializeField] private Color colorFilled = new Color(0.20f, 0.45f, 0.85f, 0.90f);
    [SerializeField] private Color colorActive = new Color(1.00f, 0.80f, 0.10f, 1.00f);

    private readonly List<Image> _imgs = new();
    private readonly List<TextMeshProUGUI> _labels = new();

    void Start()
    {
        if (slotManager == null) slotManager = SpellSlotManager.Instance;
        if (slotManager == null) { Debug.LogError("[SpellSlotUI] SpellSlotManager не найден"); return; }

        BuildButtons();

        slotManager.OnSlotChanged += _ => Refresh();
        slotManager.OnSlotSaved += (_, __) => Refresh();

        if (saveButton != null)
            saveButton.onClick.AddListener(slotManager.SaveCurrentSlot);

        Refresh();
    }

    void OnDestroy()
    {
        if (slotManager == null) return;
        slotManager.OnSlotChanged -= _ => Refresh();
        slotManager.OnSlotSaved -= (_, __) => Refresh();
    }

    private void BuildButtons()
    {
        foreach (Transform t in slotContainer) Destroy(t.gameObject);
        _imgs.Clear(); _labels.Clear();

        for (int i = 0; i < slotManager.SlotCount; i++)
        {
            int idx = i;
            var obj = Instantiate(slotButtonPrefab, slotContainer);

            var btn = obj.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => slotManager.SwitchToSlot(idx));

            _imgs.Add(obj.GetComponent<Image>());

            var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) lbl.text = (i + 1).ToString();
            _labels.Add(lbl);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < _imgs.Count; i++)
        {
            if (_imgs[i] == null) continue;
            bool active = i == slotManager.ActiveIndex;
            bool filled = slotManager.GetSlot(i) is { IsEmpty: false };
            _imgs[i].color = active ? colorActive : filled ? colorFilled : colorEmpty;
        }
    }
}