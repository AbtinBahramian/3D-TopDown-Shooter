using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask collisonMask;
    WeaponObject weaponObject;

    float speed;
    float damage;
    float secondsToDestroy = 2f;
    float skinWidth = .1f;

    private void Start() {
        Destroy(gameObject, secondsToDestroy);
        //if projectile fire from within a enemy this array will return value
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .2f, collisonMask);
        if(initialCollisions.Length > 0){
            OnCollision(initialCollisions[0]);
        }
    }

    public void SetSpeed(float newSpeed){
        speed = newSpeed;
    }

    //settin the damage of bullet based on weapon damage
    public void SetDamage(float newDamage){
        damage = newDamage;
    }

    private void Update() {
        float moveDistance = Time.deltaTime * speed;

        //firing the projectile in the forward direction
        transform.Translate(Vector3.forward * moveDistance);
        CheckCollision(moveDistance);
    }

    private void CheckCollision(float moveDistance)
    {
        //castin a Ray from projectile position to the forward of it
        Ray ray = new Ray(transform.position, transform.forward);

        //raycasting to get the collison with collisionMask Layer
        if(Physics.Raycast(ray, out RaycastHit raycastHit, moveDistance + skinWidth, collisonMask, QueryTriggerInteraction.Collide)){
            OnCollission(raycastHit);
        }
    }

    private void OnCollission(RaycastHit raycastHit)
    {
        //Trying to get the Idamagebale component of object we hit with the raycast
        IDamagebale damagebaleObject = raycastHit.collider.GetComponent<IDamagebale>();
        if(damagebaleObject != null){
            damagebaleObject.TakeDamage(damage, raycastHit);
        }
        Destroy(gameObject);
    }

    private void OnCollision(Collider collider){
        IDamagebale damagebaleObject = collider.GetComponent<IDamagebale>();
        if(damagebaleObject != null){
            damagebaleObject.PlayerTakeDamage(damage);
        }
        Destroy(gameObject);
    }


}
