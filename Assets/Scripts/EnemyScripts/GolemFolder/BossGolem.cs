using Unity.VisualScripting;
using UnityEngine;

public class BossGolem : BossAI
{
    public float MaxHP = 1500f;

    [Header("Ссылки на объекты Голема")]
    public GameObject stonePrefab;
    public Transform throwPoint;
    public GameObject slamMarkerPrefab;

    private void Start()
    {
        HP = MaxHP;
        InitializeAI();
    }
    
    public override void takeDamage(float amount)
    {
        if(HP <= 0)
        {
            return;
        }

        HP -= amount;
        if(HP <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        StopAllCoroutines();
        bossAI = null;
        baseAI = null;
        Destroy(gameObject, 3f);
    }

    protected void Update()
    {
        //bossAI?.execute(this);
    }
}
