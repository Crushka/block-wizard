using UnityEngine;

public class GolemFirstPhase : EnemyBehaviour
{
    public float rangedRange = 15f;
    public float stopDistance = 5f;

    public override void execute()
    {
        if(owner.HP / owner.maxHP > 0.5f)
        {
            float dist = Vector3.Distance(owner.transform.position, owner.target.position);
            
        }
    }

}
