using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Camera sceneCamera;

    [SerializeField]
    private LayerMask placementLayer;

    [SerializeField]
    private float raycastMaxDistance;

    private Vector3 lastPosition;

    public event Action OnClicked;
    #endregion

    #region Unity Methods
    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) 
            OnClicked?.Invoke();
    }
    #endregion

    #region Get Positions Methods
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
    #endregion
}
