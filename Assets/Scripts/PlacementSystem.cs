using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    GameObject mouseIndicator, cellIndicator;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectDatabaseSO database;
    private int selectedObjectIndex = -1;



    private void Start() 
    {
        StopPlacement();
    }
    public void StartPlacement(int ID) 
    {
        StopPlacement();
        selectedObjectIndex = database.objects.FindIndex(data => data.ID == ID);
        if(selectedObjectIndex < 0) 
        {
            Debug.LogError($"no ID found {ID}");
            return;
        }
        cellIndicator.SetActive(true);
        inputManager.OnClicked += PlaceObject;
        inputManager.OnExit += StopPlacement;
    }
    private void StopPlacement() 
    {
        selectedObjectIndex = -1;
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= PlaceObject;
        inputManager.OnExit -= StopPlacement;
    }
    private void PlaceObject()
    {
        if (inputManager.IsPointerOverUI()) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        GameObject newSphere = Instantiate(database.objects[selectedObjectIndex].Prefab);
        newSphere.transform.position = grid.CellToWorld(gridPosition);

    }
    private void Update() 
    {
        if (selectedObjectIndex < 0) return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }

}
