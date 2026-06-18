using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a stack of items inside a single inventory slot.
/// Attach this to the InventoryItemPrefab root.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    // Renamed to ItemData to avoid compiler ambiguity with the Item class name
    public Item ItemData { get; private set; }
    public int Count { get; private set; }

    public void Initialise(Item item, int count, Sprite icon)
    {
        ItemData = item;
        Count = count;

        if (iconImage != null)
            iconImage.sprite = icon;

        RefreshCountLabel();
    }

    public void AddCount(int delta)
    {
        Count = Mathf.Max(0, Count + delta);
        RefreshCountLabel();
    }

    public void SetCount(int value)
    {
        Count = Mathf.Max(0, value);
        RefreshCountLabel();
    }

    private void RefreshCountLabel()
    {
        if (countText == null) return;
        countText.gameObject.SetActive(Count > 1);
        countText.text = $"x{Count}";
    }
}