using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{

    public GameObject[] enemies;
    public GameObject cell;
    public int cellQuantity = 0;

    private void Start()
    {  
        for(int i = 0; i < enemies.Length; i++)
        {
            for(int j = 0; j < 100; j++)
            {
                if(enemies[i].name == "Slime")
                {
                    Instantiate(enemies[i], new Vector3(RandomNumber(), 1, 10), Quaternion.Euler(0, 0, 0));
                    continue;
                }
                Instantiate(enemies[i], new Vector3(RandomNumber(), 5, 10), Quaternion.Euler(0, 0, 0));
            }
        }

        for(int i = 0; i < cellQuantity; i++)
        {
            Instantiate(cell, new Vector3(RandomNumber(), 1, 10), Quaternion.Euler(0, 0, 0));
        }
    }


    private int RandomNumber()
    {
        return Random.Range(0, 20);
    }
}
