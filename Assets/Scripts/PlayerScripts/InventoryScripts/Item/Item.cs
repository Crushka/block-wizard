using UnityEngine;

public interface IEquippable { void Equip(); }
public interface IQuickSlotable { void Activate(); }

[RequireComponent(typeof(Collider))]
public abstract class Item : MonoBehaviour
{
    [Header("Item Data")]
    public string id;
    public int amount = 1;

    public virtual void AddToInventory() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AddToInventory();
            Destroy(gameObject);
        }
    }
}