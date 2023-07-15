using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingEntity
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LayerMask weaponLayer;
    [SerializeField] private GunController gunController;

    private float speed = 10f;
    private Vector3 lastInteractDir;
    private Vector3 moveDir;

    protected override void Start() {
        base.Start(); // base class start function
        inputManager.OnWeaponPickUp += InputManager_OnWeaponPickUp;
    }

    private void InputManager_OnWeaponPickUp(object sender, EventArgs e)
    {
        
        HandleInteractions();
    }


    private void FixedUpdate() {
        Vector2 inputVector = inputManager.GetInputVectorNormalaized();
        moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        if(moveDir != Vector3.zero){
            lastInteractDir = moveDir;
        }

        transform.position += moveDir * speed * Time.fixedDeltaTime;
    }

    private void HandleInteractions(){

        float interactDistance = 2f;
        Vector3 New = new Vector3 (transform.position.x , 0f, transform.position.z);
        if(Physics.Raycast(New, lastInteractDir, out RaycastHit rayCastHit, interactDistance, weaponLayer)){
            if (rayCastHit.transform.TryGetComponent(out WeaponObject weaponObject)){
                gunController.EquipGun(weaponObject);
            }
        } 
    }
}
