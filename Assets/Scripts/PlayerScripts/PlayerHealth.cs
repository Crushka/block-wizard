using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("UI")]
    [SerializeField] private GameObject deadScreen;

    [Header("Камера на время экрана смерти")]
    [SerializeField] private GameObject deadCamera;

    public void SetDeadScreen(GameObject screen)
    {
        deadScreen = screen;
    }
    public float GetHealth() {  return health; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("DathTrigger"))
        {
            Die();
        }
    }

    public void takeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }


    public void Die()
    {
        Debug.Log("Игрок погиб");

        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.PlayerHP = 0;

        if (deadCamera != null)
            deadCamera.SetActive(true);

        if (deadScreen != null)
            deadScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Destroy(transform.root.gameObject);
    }
}