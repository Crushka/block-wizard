using UnityEngine;

public class BossAI : Enemy
{
    public string id;
    public EnemyBehaviour bossAI;

    public override void InitializeAI()
    {
        base.InitializeAI();
        EnemyBehaviour[] allBehaviours = GetComponents<EnemyBehaviour>();
        
        foreach(var b in allBehaviours)
        {
            if(b != bossAI)
            {
                bossAI = b;
                bossAI.Init(this);
                break;
            }
        }
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
