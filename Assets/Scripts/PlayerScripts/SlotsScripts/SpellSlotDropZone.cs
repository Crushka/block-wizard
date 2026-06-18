// SpellSlotDropZone.cs
// Attach to each HUD spell slot Image object (the same one that already has a Button).
// Receives drag-and-drop from SpellDragItem and tells SpellSlotManager what to do.
//
// Rules:
//   • Source = result slot  (-1) → assign spell to this slot (overwrite any existing)
//   • Source = another HUD slot  → swap the two slots
//   • Source == this slot        → no-op

using UnityEngine;
using UnityEngine.EventSystems;

public class SpellSlotDropZone : MonoBehaviour, IDropHandler
{
    // Set by SpellSlotUI when building buttons
    public int SlotIndex { get; set; }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag?.GetComponent<SpellDragItem>();
        if (drag == null || drag.SpellNode == null) return;

        int src = drag.SourceSlotIndex;

        if (src == SlotIndex) return; // dropped onto itself → ignore

        var mgr = SpellSlotManager.Instance;
        if (mgr == null) return;

        if (src == -1)
        {
            // From result slot → assign (overwrite)
            mgr.AssignToSlot(SlotIndex, drag.SpellNode);
        }
        else
        {
            // From another HUD slot → swap
            mgr.SwapSlots(src, SlotIndex);
        }

        drag.ConsumeByDrop();
    }
}