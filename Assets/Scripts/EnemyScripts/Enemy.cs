using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Entity
{
    public Transform target;
    public List<IAttack> attacks;
    public Animator animator;
    private bool isDead = false;
    [Header("Ai Logic")]
    public EnemyBehaviour baseAI;

    public virtual void InitializeAI()
    {
        baseAI = GetComponent<EnemyBehaviour>();
        if(target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("PlayerBody");
            if(player != null)
            {
                target = player.transform;
            }
        }
           
        baseAI = GetComponent<EnemyBehaviour>();
        if(baseAI != null)
        {
            baseAI.Init(this);
        }
    }

    private void Update()
    {
        if(HP <= 0)
        {
            return;
        }
        baseAI?.execute();
    }

    private void Attack()
    {

    }

}
