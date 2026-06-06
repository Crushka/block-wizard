using UnityEngine;

public class Potion : RegularItem, IQuickSlotable
{
    public PotionType potionType = PotionType.Health;

    public void Activate() => Debug.Log("Зелье выпито!");

    public override void AddToInventory()
    {
        InventorySlotManager.Instance?.AddItem(this);
    }
}