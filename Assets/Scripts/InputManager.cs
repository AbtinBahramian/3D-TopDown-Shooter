using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event EventHandler OnWeaponPickUp;

    private InputActionManager inputActions;
    [SerializeField] private GunController gunController;

    private void Awake() {
        inputActions = new InputActionManager();
        inputActions.Player.Move.Enable();
        inputActions.Player.I.performed += Interaction_Performed;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F)){
            OnWeaponPickUp?.Invoke(this, EventArgs.Empty);
        }

        if(Input.GetMouseButton(0)){
            gunController.Shoot();
        }
    }

    private void Interaction_Performed(InputAction.CallbackContext context)
    {
        
        
    }

    public Vector2 GetInputVectorNormalaized(){
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }
}
