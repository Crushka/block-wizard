using UnityEngine;

public abstract class BossAI : Enemy
{
    public string id;
    public EnemyBehaviour bossAI;

    public override void InitializeAI()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("PlayerBody")?.transform;

        baseAI = GetComponent<QueenFlight>();
        bossAI = GetComponent<QueenCombatBehaviour>();

        if (baseAI != null) baseAI.Init(this);
        if (bossAI != null) bossAI.Init(this);
    }


    public override void Die()
    {
        Destroy(gameObject);
    }

    public override void Regeneration()
    {
    }

    public override void takeDamage(float amount)
    {
    }

}
