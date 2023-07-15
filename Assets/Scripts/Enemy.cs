using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity
{
    private enum State{
        Idle,
        Chasing,
        Attack
    }

    private State currentState;
    private NavMeshAgent navMeshAgent;
    private GameObject target;
    private LivingEntity targetLivingEntity;
    
    private Material skinMaterial;
    private Color originalColor;

    private float attackDistanceThreshhold = 0.5f;
    private float timeBetweenAttacks = 1f;
    private float nextAttackTime;
    private float enemyCollisionRadius;
    private float targetCollisonRadius;
    private bool hasTarget;
    private float damage = 5;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();

         // saving original color for changingg the color when attacking
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        currentState = State.Chasing;
    }

    protected override void Start() {
        base.Start();
        if(GameObject.FindGameObjectWithTag("Player") != null){
            target = GameObject.FindGameObjectWithTag("Player");
            hasTarget = true;

            //fetching LivingEntint of target for OnDeath event use
            targetLivingEntity = target.GetComponent<LivingEntity>();
            targetLivingEntity.OnDeath += OnTargetDeath;

            enemyCollisionRadius = transform.GetComponent<CapsuleCollider>().radius;
            targetCollisonRadius = target.GetComponent<CapsuleCollider>().radius;
            StartCoroutine (UpdatePath());
        }
    }

    private void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    private void Update() {
        if(hasTarget){
            if(Time.time > nextAttackTime){
            
                // not using Mathf.Distance because we don't need the actual distance, we are just comparing
                float sqrDistantToTarget = (target.transform.position - transform.position).sqrMagnitude;
                //we are near the target to attack
                if(sqrDistantToTarget < Mathf.Pow(attackDistanceThreshhold + enemyCollisionRadius + targetCollisonRadius, 2)){
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }   
    }

    IEnumerator Attack(){
        currentState = State.Attack;
        //navMeshAgent.enabled = false;
        
        Vector3 originalPostition = transform.position;
        Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
        Vector3 targetPosition = target.transform.position - dirToTarget * (enemyCollisionRadius);

        float percent = 0f;
        float attackSpeed = 3f;
        bool hasAppliedDamage = false;
        skinMaterial.color = Color.red;

        while(percent <= 1){
            // apply damage when enemy is at end of destination and is going back and damage has not been appiled
            if(percent >= .5f && !hasAppliedDamage){
                hasAppliedDamage = true;
                targetLivingEntity.PlayerTakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            //diagram based on y=(-x^2 + x) * 4
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPostition, targetPosition, interpolation);
            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        //navMeshAgent.enabled = true;
    }


    // using IEnumerator instead of update to reduce the cost of calculating
    IEnumerator UpdatePath(){
        float refreshRate = .25f;
        while(hasTarget){
            if(currentState == State.Chasing){
                //correcting that enemy destination won't be middle of player capsule
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                Vector3 targetPosition = target.transform.position - dirToTarget * (enemyCollisionRadius + targetCollisonRadius + attackDistanceThreshhold/2);
                if(!dead){
                    navMeshAgent.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);   
        }  
    }
}
