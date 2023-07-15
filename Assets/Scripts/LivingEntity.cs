using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagebale
{
    public event Action OnDeath;

    [SerializeField] protected float startingHealth;
    protected float health;
    protected bool dead;

    // Start is called before the first frame update
    protected virtual void Start(){
        // each player or enemy can have different health
        health = startingHealth;
    }

    private void Die(){
        dead = true;
        //trigerring event to inform enemySpawner and reduce the number of enemies alive
        if(OnDeath != null){
            OnDeath();
        }
        Destroy(gameObject);
    }
    
    public void TakeDamage(float damage, RaycastHit raycastHit){
        //we will do stuff here later
        PlayerTakeDamage(damage);
    }

    public void PlayerTakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0 && !dead){
            Die();
        }
    }
}
