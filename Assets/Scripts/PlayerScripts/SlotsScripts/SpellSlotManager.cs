using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSlotManager : MonoBehaviour
{
    public static SpellSlotManager Instance { get; private set; }

    [Header("Настройки")]
    [Range(1, 9)]
    [SerializeField] private int slotCount = 3;

    [Header("Ссылки")]
    [SerializeField] private NodeEditorManager nodeEditorManager;
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private BookInteraction bookInteraction;

    private List<SpellSlot> _slots = new();
    private int _activeIndex = 0;
    private InputAction[] _slotActions = System.Array.Empty<InputAction>();

    public event System.Action<int> OnSlotChanged;
    public event System.Action<int, SpellSlot> OnSlotSaved;

    public int SlotCount => _slots.Count;
    public int ActiveIndex => _activeIndex;
    public SpellSlot ActiveSlot => _slots[_activeIndex];
    public SpellSlot GetSlot(int i) => (i >= 0 && i < _slots.Count) ? _slots[i] : null;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildSlots();
        BuildInputActions(); // actions создаются и сразу включаются здесь
    }

    // OnEnable/OnDisable больше не трогают actions — Enable вызывается прямо в BuildInputActions
    // чтобы избежать гонки между Awake и OnEnable

    void OnDestroy()
    {
        DisableAndClearActions();
    }

    // ── Slot switching ─────────────────────────────────────────────────────────

    public void SwitchToSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= _slots.Count) return;
        if (newIndex == _activeIndex) return;

        Debug.Log($"[SpellSlotManager] Переключение слота: {_activeIndex + 1} → {newIndex + 1}");

        if (nodeEditorManager != null)
        {
            _slots[_activeIndex].SnapshotFromEditor(nodeEditorManager);
            _slots[_activeIndex].Compile();
        }

        _activeIndex = newIndex;

        if (nodeEditorManager != null)
            _slots[_activeIndex].RestoreToEditor(nodeEditorManager);

        ApplyToCaster();
        SyncGSM();
        OnSlotChanged?.Invoke(_activeIndex);
    }

    // ── Save ───────────────────────────────────────────────────────────────────

    public void SaveCurrentSlot()
    {
        var slot = _slots[_activeIndex];
        slot.SnapshotFromEditor(nodeEditorManager);
        slot.Compile();

        ApplyToCaster();
        SyncGSM();

        OnSlotSaved?.Invoke(_activeIndex, slot);
        Debug.Log($"[SpellSlotManager] Слот {_activeIndex + 1} сохранён");
    }

    public void SaveAllSlots()
    {
        SaveCurrentSlot();
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;
        foreach (var slot in gsm.SpellSlots)
            if (slot.compiledNode == null && !slot.IsEmpty)
                slot.Compile();
        Debug.Log($"[SpellSlotManager] Все {gsm.SpellSlots.Count} слотов сохранены.");
    }

    // ── Drag & drop API ────────────────────────────────────────────────────────

    public void AssignToSlot(int slotIndex, NodeBase node)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count || node == null) return;

        var slot = _slots[slotIndex];
        slot.graph = null;
        slot.compiledNode = node;

        if (slotIndex == _activeIndex)
            ApplyToCaster();

        SyncGSM();
        OnSlotSaved?.Invoke(slotIndex, slot);
        Debug.Log($"[SpellSlotManager] AssignToSlot: слот {slotIndex + 1} ← {node.GetDominantAttack()} dmg={node.Damage:F1}");
    }

    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= _slots.Count) return;
        if (indexB < 0 || indexB >= _slots.Count) return;
        if (indexA == indexB) return;

        var slotA = _slots[indexA];
        var slotB = _slots[indexB];

        (slotA.graph, slotB.graph) = (slotB.graph, slotA.graph);
        (slotA.compiledNode, slotB.compiledNode) = (slotB.compiledNode, slotA.compiledNode);

        if (_activeIndex == indexA || _activeIndex == indexB)
        {
            if (nodeEditorManager != null)
                _slots[_activeIndex].RestoreToEditor(nodeEditorManager);
            ApplyToCaster();
        }

        SyncGSM();
        OnSlotChanged?.Invoke(_activeIndex);
        Debug.Log($"[SpellSlotManager] SwapSlots: {indexA + 1} ↔ {indexB + 1}");
    }

    // ── Book ───────────────────────────────────────────────────────────────────

    public void OnBookOpened()
    {
        Debug.Log($"[SpellSlotManager] Книга открыта → слот {_activeIndex + 1}");
        _slots[_activeIndex].RestoreToEditor(nodeEditorManager);
    }

    // ── Late init ──────────────────────────────────────────────────────────────

    public void LateInit(NodeEditorManager mgr, SpellCaster caster, BookInteraction book)
    {
        if (mgr != null) nodeEditorManager = mgr;
        if (caster != null) spellCaster = caster;
        if (book != null) bookInteraction = book;

        // Отключаем старые actions перед пересозданием
        DisableAndClearActions();
        BuildSlots();
        BuildInputActions();
        ApplyToCaster();
        Debug.Log("[SpellSlotManager] LateInit завершён");
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void BuildSlots()
    {
        _slots.Clear();
        int count = Mathf.Clamp(slotCount, 1, 9);

        var gsm = GameStateManager.Instance;
        if (gsm != null && gsm.SpellSlots != null && gsm.SpellSlots.Count == count)
        {
            _slots.AddRange(gsm.SpellSlots);
            _activeIndex = Mathf.Clamp(gsm.ActiveSpellSlotIndex, 0, count - 1);
            foreach (var slot in _slots)
                if (!slot.IsEmpty) slot.Compile();
            Debug.Log($"[SpellSlotManager] Восстановлено {count} слотов из GSM, активный {_activeIndex + 1}");
        }
        else
        {
            for (int i = 0; i < count; i++) _slots.Add(new SpellSlot(i));
            Debug.Log($"[SpellSlotManager] Создано {count} новых слотов");
        }
    }

    private void BuildInputActions()
    {
        _slotActions = new InputAction[_slots.Count];
        for (int i = 0; i < _slots.Count; i++)
        {
            int idx = i;
            var a = new InputAction($"Slot{i + 1}", InputActionType.Button);
            a.AddBinding($"<Keyboard>/{i + 1}");
            a.performed += _ => SwitchToSlot(idx);
            a.Enable();
            _slotActions[i] = a;
        }
        Debug.Log($"[SpellSlotManager] Input actions созданы и включены ({_slots.Count} шт.)");
    }

    private void DisableAndClearActions()
    {
        foreach (var a in _slotActions)
        {
            a.performed -= _ => { };
            a.Disable();
            a.Dispose();
        }
        _slotActions = System.Array.Empty<InputAction>();
    }

    private void ApplyToCaster()
    {
        if (spellCaster == null) return;
        var node = _slots[_activeIndex].compiledNode;
        if (node != null)
        {
            spellCaster.PrepareSpellFromNode(node);
            Debug.Log($"[SpellSlotManager] SpellCaster ← слот {_activeIndex + 1}");
        }
        else
        {
            spellCaster.ClearSpell();
            Debug.Log($"[SpellSlotManager] SpellCaster сброшен — слот {_activeIndex + 1} пустой");
        }
    }

    private void SyncGSM()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) return;
        gsm.SpellSlots = _slots;
        gsm.ActiveSpellSlotIndex = _activeIndex;
    }
}