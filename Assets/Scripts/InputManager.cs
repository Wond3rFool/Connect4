using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    [SerializeField]
    private LayerMask placementLayer;

    [SerializeField]
    private float raycastMaxDistance;

    private Vector3 lastPosition;


    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastMaxDistance, placementLayer))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}
