using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMouse3D : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;
    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // cast a ray from camera to where the mouse is
        Plane floorGround = new Plane (Vector3.up, Vector3.zero);

        

        if(floorGround.Raycast(ray, out float rayDistance)){ 
            Vector3 point = ray.GetPoint(rayDistance);

            //to prevent the player to look down and set bounderies
            Vector3 heightCorrectedPoint = new Vector3(point.x, transform.position.y, point.z);

            transform.LookAt(heightCorrectedPoint);
        }
    }
}
