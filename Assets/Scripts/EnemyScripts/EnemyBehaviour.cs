using UnityEngine;

public abstract class EnemyBehaviour : MonoBehaviour
{
    protected Enemy owner;

    public virtual void Init(Enemy enemy)
    {
        owner = enemy;
    }

    public abstract void execute();

}
