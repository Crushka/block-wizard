using System.Linq;
using UnityEngine;

public class KillAll : MonoBehaviour
{
    public GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = player.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.takeDamage(9999f);
        }
    }
}
