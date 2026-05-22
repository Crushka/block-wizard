using UnityEngine;

public class BatProj : MonoBehaviour
{
    public float gravityScale = 3f;
    public float damage = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if(damageable != null)
        {
            if(!collision.gameObject.CompareTag("Bat"))
            {
                damageable.takeDamage(damage);
            }
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