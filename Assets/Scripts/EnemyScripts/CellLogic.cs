using UnityEngine;

public class CellLogic : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [SerializeField] private float health = 50f;
    [SerializeField] private GameObject breakEffectPrefab; // Эффект разрушения (частицы)

    [Header("Настройки спавна пчел")]
    [SerializeField] private GameObject beePrefab; // Префаб вашей пчелы с BeeAI
    [SerializeField] private int beeCount = 3; // Сколько пчел вылетит
    [SerializeField] private float spawnRadius = 1.5f;

    private bool isDestroyed = false;

    public void takeDamage(float amount)
    {
        if (isDestroyed) return;

        health -= amount;
        Debug.Log($"Клетка получила урон: {amount}. Осталось: {health}");

        // Можно добавить микро-тряску клетки при попадании
        transform.position += Random.insideUnitSphere * 0.05f;

        if (health <= 0)
        {
            Die();
        }
    }

    // Вставьте это в ваш существующий скрипт BeeCage.cs в метод Die()
    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // СООБЩАЕМ МЕНЕДЖЕРУ
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
            // Генерируем случайную позицию вокруг клетки
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y); // Чтобы пчелы не спавнились под землей

            Vector3 spawnPos = transform.position + randomOffset;

            // Создаем пчелу
            GameObject bee = Instantiate(beePrefab, spawnPos, Quaternion.identity);

            // Если нужно, чтобы пчелы сразу атаковали игрока, можно найти его
            // Но в вашем BeeAI.cs поиск игрока уже реализован в Start() через Tag "Player"
        }
    }
}