using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform floor;
    [SerializeField] private Transform gunHoldingPoint;
    [SerializeField] private WeaponsSO startingGun;
    
    private WeaponObject equipedGun;
    private WeaponObject droppedGun;

    private void Start() {
        if(startingGun != null){
            EquipStartingGun(startingGun);
        }
    }

    public void EquipGun(WeaponObject gunToEquip){
        Debug.Log(gunToEquip.name);
        if(equipedGun != null){
            //we have a gun in hand and will drop it in player position
            Vector3 rayCastPlayerPosition = new Vector3 (player.position.x , 0f, player.position.z);
            equipedGun.transform.position = rayCastPlayerPosition;
            equipedGun.SetParent(floor);
            droppedGun = equipedGun;
        }
       
        //Transform equipedGunTransform = Instantiate(gunToEquip.prefab, gunHoldingPoint);
        equipedGun = gunToEquip;
        equipedGun.SetParent(gunHoldingPoint);
    }

    public void EquipStartingGun(WeaponsSO gunToEquip){
        Transform equipedGunTransform = Instantiate(gunToEquip.prefab, gunHoldingPoint);
        equipedGun = equipedGunTransform.GetComponent<WeaponObject>();
        equipedGun.SetParent(gunHoldingPoint);
    }

    internal void Shoot()
    {
        if(equipedGun != null){
            equipedGun.Shoot();
        }
    }
}
