using UnityEngine;

public class Reservation : RegularItem, IEquippable
{
    public string reservationAttribute;

    public void Equip()
    {
        // TODO: equip logic
    }

    public override void AddToInventory()
    {
        InventorySlotManager.Instance?.AddItem(this);
    }
}