using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour 
{
    [Tooltip("Перетащите сюда компонент Item (Potion или NodeItem), находящийся на этом объекте")]
    [SerializeField] private Item itemData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && itemData != null)
        {
            itemData.AddToInventory();
            Destroy(gameObject);
        }
    }
}