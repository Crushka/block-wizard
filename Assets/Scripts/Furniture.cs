using UnityEngine;

public class Furniture : MonoBehaviour, IDamageable
{
    float HP = 100;

   public void takeDamage(float amount)
    {
        HP -= amount;
    }
}
