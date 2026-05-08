using UnityEngine;

public class GolemProj : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // Чтобы камень не пролетал сквозь объекты на большой скорости
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ПРЯМОЕ ПОПАДАНИЕ! Игрок получил урон.");
            // target.TakeDamage(damage);
        }

        // Эффект удара (можно добавить частицы пыли)
        Destroy(gameObject);
    }
}