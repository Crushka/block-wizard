using UnityEngine;

public class BatProj : MonoBehaviour
{
    public float gravityScale = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bat"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("HIT!");
        }

        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        Vector3 extraGravity = Physics.gravity * (gravityScale - 1) * body.mass;
        body.AddForce(extraGravity);
    }
}