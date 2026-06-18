// SpellSlotUI.cs
// Работает со статическими объектами иерархии — ничего не спавнит.
//
// Иерархия в сцене (3 слота):
//   AbilityBack1  ← Image (фон слота, красится под состояние)
//     NumGroup1
//       NumText   ← TextMeshProUGUI (текст иконки заклинания)
//   AbilityBack2 / NumGroup2 / NumText
//   AbilityBack3 / NumGroup3 / NumText
//
// Drag & drop:
//   • SpellDragItem вешается на AbilityBackN в рантайме (один раз в Start).
//   • SpellSlotDropZone вешается на AbilityBackN в рантайме (один раз в Start).
//   • Слот результата — отдельный Image в книге, тоже получает SpellDragItem когда
//     вызывается ShowResultSpell().

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSlotUI : MonoBehaviour
{
    // ── Inspector: HUD-слоты (статические объекты) ────────────────────────────

    [Header("HUD – фоны слотов (AbilityBack)")]
    [SerializeField] private Image slotBack0;   // AbilityBack1
    [SerializeField] private Image slotBack1;   // AbilityBack2
    [SerializeField] private Image slotBack2;   // AbilityBack3

    [Header("HUD – тексты слотов (NumText внутри NumGroup)")]
    [SerializeField] private TextMeshProUGUI slotLabel0;  // NumText слота 1
    [SerializeField] private TextMeshProUGUI slotLabel1;  // NumText слота 2
    [SerializeField] private TextMeshProUGUI slotLabel2;  // NumText слота 3

    [Header("Слот результата (в книге)")]
    [SerializeField] private Image resultSlotImage;  // Image ячейки результата
    [SerializeField] private TextMeshProUGUI resultLabel;      // TMP внутри ячейки результата

    [Header("Менеджер и кнопка сохранения")]
    [SerializeField] private SpellSlotManager slotManager;
    [SerializeField] private Button saveButton;

    [Header("Цвета слотов")]
    [SerializeField] private Color colorEmpty = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    [SerializeField] private Color colorFilled = new Color(0.20f, 0.45f, 0.85f, 0.90f);
    [SerializeField] private Color colorActive = new Color(1.00f, 0.80f, 0.10f, 1.00f);

    // ── Private ───────────────────────────────────────────────────────────────

    // Массивы для удобного итерирования — заполняются в Start из SerializeField выше
    private Image[] _backs;
    private TextMeshProUGUI[] _labels;

    // SpellDragItem на каждом слоте — обновляем при каждом Refresh
    private SpellDragItem[] _dragItems;

    // Drag-item на слоте результата
    private SpellDragItem _resultDragItem;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (slotManager == null) slotManager = SpellSlotManager.Instance;
        if (slotManager == null) { Debug.LogError("[SpellSlotUI] SpellSlotManager не найден"); return; }

        // Собираем массивы из полей Inspector
        _backs = new Image[] { slotBack0, slotBack1, slotBack2 };
        _labels = new TextMeshProUGUI[] { slotLabel0, slotLabel1, slotLabel2 };
        _dragItems = new SpellDragItem[_backs.Length];

        // Вешаем SpellSlotDropZone и Button на каждый AbilityBack один раз
        for (int i = 0; i < _backs.Length; i++)
        {
            if (_backs[i] == null) continue;

            int idx = i;

            // Drop zone
            var dz = _backs[i].GetComponent<SpellSlotDropZone>();
            if (dz == null) dz = _backs[i].gameObject.AddComponent<SpellSlotDropZone>();
            dz.SlotIndex = i;

            // Клик по слоту — переключить активный
            var btn = _backs[i].GetComponent<Button>();
            if (btn == null) btn = _backs[i].gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => slotManager.SwitchToSlot(idx));

            // Placeholder для drag-item (заполнится в RefreshHudSlots)
            _dragItems[i] = null;
        }

        slotManager.OnSlotChanged += _ => RefreshAll();
        slotManager.OnSlotSaved += (_, __) => RefreshAll();

        if (saveButton != null)
            saveButton.onClick.AddListener(slotManager.SaveCurrentSlot);

        RefreshAll();
    }

    void OnDestroy()
    {
        if (slotManager == null) return;
        slotManager.OnSlotChanged -= _ => RefreshAll();
        slotManager.OnSlotSaved -= (_, __) => RefreshAll();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_backs == null) return; // Start ещё не вызван
        RefreshHudSlots();
    }

    private void RefreshHudSlots()
    {
        for (int i = 0; i < _backs.Length; i++)
        {
            if (_backs[i] == null) continue;

            bool isActive = i == slotManager.ActiveIndex;
            var slot = slotManager.GetSlot(i);
            bool filled = slot?.compiledNode != null;
            NodeBase node = slot?.compiledNode;

            // Цвет фона
            _backs[i].color = isActive ? colorActive : filled ? colorFilled : colorEmpty;

            // Текст иконки
            if (_labels[i] != null)
                _labels[i].text = node != null ? SpellIconHelper.GetIconText(node) : "Пусто";

            // Обновляем SpellDragItem на AbilityBack
            UpdateDragItem(i, node);
        }
    }

    // Добавляет/обновляет SpellDragItem прямо на объекте AbilityBack
    private void UpdateDragItem(int i, NodeBase node)
    {
        var backGO = _backs[i].gameObject;

        if (node == null)
        {
            // Нет заклинания — убираем drag
            var existing = backGO.GetComponent<SpellDragItem>();
            if (existing != null) Destroy(existing);
            _dragItems[i] = null;
            return;
        }

        // Есть заклинание — обновляем или создаём SpellDragItem
        var drag = backGO.GetComponent<SpellDragItem>();
        if (drag == null) drag = backGO.AddComponent<SpellDragItem>();
        drag.SourceSlotIndex = i;
        drag.SpellNode = node;
        _dragItems[i] = drag;
    }

    // ── Слот результата ───────────────────────────────────────────────────────

    /// <summary>
    /// Вызвать из кода книги/редактора когда заклинание скомпилировано.
    /// Показывает иконку в ячейке результата и делает её draggable.
    /// </summary>
    public void ShowResultSpell(NodeBase node)
    {
        // Убираем старый drag с ячейки результата
        if (_resultDragItem != null)
        {
            Destroy(_resultDragItem);
            _resultDragItem = null;
        }

        if (node == null)
        {
            if (resultLabel != null) resultLabel.text = string.Empty;
            if (resultSlotImage != null) resultSlotImage.color = colorEmpty;
            return;
        }

        // Текст
        if (resultLabel != null)
            resultLabel.text = SpellIconHelper.GetIconTextExtended(node);

        // Цвет фона ячейки — тинт цвета заклинания
        if (resultSlotImage != null)
            resultSlotImage.color = new Color(
                node.PrimaryColor.r,
                node.PrimaryColor.g,
                node.PrimaryColor.b, 0.6f);

        // Вешаем SpellDragItem на Image ячейки результата
        if (resultSlotImage != null)
        {
            var drag = resultSlotImage.GetComponent<SpellDragItem>();
            if (drag == null) drag = resultSlotImage.gameObject.AddComponent<SpellDragItem>();
            drag.SourceSlotIndex = -1;   // -1 = слот результата
            drag.SpellNode = node;
            _resultDragItem = drag;
        }
    }

    /// <summary>Очищает ячейку результата.</summary>
    public void ClearResultSlot()
    {
        if (_resultDragItem != null) { Destroy(_resultDragItem); _resultDragItem = null; }
        if (resultLabel != null) resultLabel.text = string.Empty;
        if (resultSlotImage != null) resultSlotImage.color = colorEmpty;
    }
}