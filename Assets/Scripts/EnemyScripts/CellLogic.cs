using UnityEngine;

public class CellLogic : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [SerializeField] private float health = 50f;
    [SerializeField] private GameObject breakEffectPrefab;

    [Header("Настройки спавна пчел")]
    [SerializeField] private GameObject beePrefab;
    [SerializeField] private int beeCount = 3;
    [SerializeField] private float spawnRadius = 1.5f;

    private bool isDestroyed = false;

    public void takeDamage(float amount)
    {
        if (isDestroyed) return;

        health -= amount;
        Debug.Log($"Клетка получила урон: {amount}. Осталось: {health}");

        transform.position += Random.insideUnitSphere * 0.05f;

        if (health <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (QueenManage.Instance != null)
        {  
            QueenManage.Instance.CageDestroyed();
        }

        if (breakEffectPrefab != null) Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        SpawnBees();
        Destroy(gameObject);
    }

    private void SpawnBees()
    {
        if (beePrefab == null) return;

        for (int i = 0; i < beeCount; i++)
        {

            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y);

            Vector3 spawnPos = transform.position + randomOffset;


            GameObject bee = Instantiate(beePrefab, spawnPos, Quaternion.identity);

        }
    }
}