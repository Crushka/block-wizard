using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;

    public void takeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Игрок получил урон: " + amount + ", текущее здоровье: " + health);
        
        if (health < 0)
        {
            Debug.Log("Игрок погиб");
        }
    }
}
