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

    private GridData gridData, sphereData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObject = new();

    private int currentPlayer = 1;

    private void Start() 
    {
        StartPlacement(currentPlayer);
        gridData = new();
        sphereData = new();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }
    public void StartPlacement(int ID) 
    {
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
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (!placementValidity) return;

        GameObject newSphere = Instantiate(database.objects[selectedObjectIndex].Prefab);
        newSphere.transform.position = grid.CellToWorld(gridPosition);
        placedGameObject.Add(newSphere);

        GridData selectedData = database.objects[selectedObjectIndex].ID == 0 ? gridData : sphereData;

        selectedData.AddObjectAt(gridPosition,
            database.objects[selectedObjectIndex].Size,
            database.objects[selectedObjectIndex].ID,
            placedGameObject.Count - 1);


        foreach(var item in placedGameObject)
        {
            Debug.Log(grid.WorldToCell(item.gameObject.transform.position));
        }
        EndTurn();
    }

    private void EndTurn()
    {
        // Switch the turn to the next player
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        StartPlacement(currentPlayer);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex) 
    {
        GridData selectedData = database.objects[selectedObjectIndex].ID == 0 ? gridData : sphereData;

        return selectedData.CanPlaceObjectAt(gridPosition, database.objects[selectedObjectIndex].Size);
    }
    private void Update() 
    {
        if (selectedObjectIndex < 0) return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }

}
