using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    [SerializeField] private WeaponsSO weaponsSO;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform projectile;

    private float nextShotTime;

    public WeaponsSO GetWeaponSO(){
        return weaponsSO;
    }

    public void SetParent(Transform parent){
        this.transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    public void Shoot(){
        //check if we can shoot based on the rateoffire
        if(Time.time > nextShotTime){
            nextShotTime = Time.time + this.GetWeaponSO().msBetweenShots / 1000;   // converting to seconds and add to next shot time

            Transform newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation);
            newProjectile.GetComponent<Projectile>().SetSpeed(this.GetWeaponSO().bulletSpeed); // sending bullet speed
            newProjectile.GetComponent<Projectile>().SetDamage(this.weaponsSO.damage); // sending bullet damage
        }
    }

}
