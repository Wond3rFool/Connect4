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

        StopPlacement();
        RotatePositions();
        EndTurn();
    }

    private void EndTurn()
    {
        // Switch the turn to the next player
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        StartPlacement(currentPlayer);
    }

    private void RotatePositions()
    {
        foreach (var item in placedGameObject)
        {
            Vector3Int oldPosition = grid.WorldToCell(item.transform.position);
            Debug.Log(oldPosition);

            // Call the appropriate UpdateGridData based on the object's ID
            GridData selectedData = (item.CompareTag("Sphere")) ? sphereData : gridData;

            // Call the UpdateGridData function to get the new position
            Vector3Int newPosition;
            selectedData.UpdateGridData(oldPosition, out newPosition);

            // Smoothly move the game object to the new position using Lerp
            StartCoroutine(MoveObjectLerp(item.transform, grid.CellToWorld(newPosition), 1.0f));
        }
    }

    // Coroutine for smoothly moving an object using Lerp
    private IEnumerator MoveObjectLerp(Transform transform, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object reaches the exact target position
        transform.position = targetPosition;
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
