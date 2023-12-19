using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    private GridData sphereData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObject = new();

    private int currentPlayer = 1;

    private bool whiteWins = false;
    private bool blackWins = false;
    private bool isDraw = false;
    private bool firstTurn = true;

    private void Awake() 
    {
        StartPlacement(currentPlayer);
        sphereData = new GridData();
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

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (!placementValidity) return;

        GameObject newSphere = Instantiate(database.objects[selectedObjectIndex].Prefab);
        newSphere.transform.position = grid.CellToWorld(gridPosition);
        placedGameObject.Add(newSphere);

        sphereData.AddObjectAt(gridPosition,
            database.objects[selectedObjectIndex].Size,
            database.objects[selectedObjectIndex].ID,
            selectedObjectIndex);

        StopPlacement();
        if(!firstTurn)
            RotatePositions();
        StartCoroutine(EndTurn(1.0f));
        firstTurn = false;
    }

    private IEnumerator EndTurn(float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Then, based on the results, you can handle the game state accordingly
        if (whiteWins && blackWins)
        {
            Debug.Log("It's a draw!");
            // Handle the draw
        }
        else if (whiteWins)
        {
            Debug.Log("White wins!");
            // Handle white wins
        }
        else if (blackWins)
        {
            Debug.Log("Black wins!");
            // Handle black wins
        }
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        StartPlacement(currentPlayer);
        sphereData.ClearPositions();
    }

    private void RotatePositions()
    {
        foreach (var item in placedGameObject)
        {
            Vector3Int oldPosition = grid.WorldToCell(item.transform.position);

            // Call the UpdateGridData function to get the new position
            Vector3Int newPosition = sphereData.UpdateGridData(oldPosition);

            // Smoothly move the game object to the new position using Lerp
            StartCoroutine(MoveObjectLerp(item.transform, grid.CellToWorld(newPosition), 0.25f));

            if (sphereData.CheckFourSpheresInARow(1))
            {
                whiteWins = true;
            }

            if (sphereData.CheckFourSpheresInARow(2))
            {
                blackWins = true;
            }

            // Check for a draw
            if (!whiteWins && !blackWins && AllSpheresPlaced())
            {
                isDraw = true;
            }
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

    private bool CheckPlacementValidity(Vector3Int gridPosition) 
    {
        return sphereData.CanPlaceObjectAt(gridPosition);
    }
    private void Update() 
    {
        if (selectedObjectIndex < 0) return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }

    private bool AllSpheresPlaced()
    {
        // Check if the total number of placed spheres equals the expected total
        int totalExpectedSpheres = database.objects.Count; // Assuming each object has only one sphere
        return placedGameObject.Count == totalExpectedSpheres;
    }
}
