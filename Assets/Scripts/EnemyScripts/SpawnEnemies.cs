using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public static SpawnEnemies Instance;

    public GameObject[] enemies;
    public GameObject cell;
    public int cellQuantity = 5;
    public int enemiesPerType = 3;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnBossMinions()
    {
        Debug.Log("СПАВНЕР: Король Голем призвал миньонов!");

        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < enemiesPerType; j++)
            {
                Vector3 spawnPos = new Vector3(RandomNumber(), 1, 10);

                if (enemies[i].name == "Slime")
                {
                    Instantiate(enemies[i], spawnPos, Quaternion.identity);
                }
                else
                {
                    spawnPos.y = 5;
                    Instantiate(enemies[i], spawnPos, Quaternion.identity);
                }
            }
        }

        for (int i = 0; i < cellQuantity; i++)
        {
            Instantiate(cell, new Vector3(RandomNumber(), 1, 10), Quaternion.identity);
        }
    }

    private int RandomNumber()
    {
        return Random.Range(-20, 20);
    }
}