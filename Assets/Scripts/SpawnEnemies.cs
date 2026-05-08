using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{

    public GameObject[] enemies;

    private void Start()
    {  
        for(int i = 0; i < enemies.Length; i++)
        {
            for(int j = 0; j < 1; j++)
            {
                Instantiate(enemies[i], new Vector3(RandomNumber(), 5, 10), Quaternion.Euler(0, 0, 0));
            }
        }
    }


    private int RandomNumber()
    {
        return Random.Range(0, 10);
    }
}
