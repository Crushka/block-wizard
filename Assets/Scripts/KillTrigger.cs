using System.Linq;

using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.takeDamage(9999f);
        }
        Debug.Log("[KillTrigger]");
    }
}