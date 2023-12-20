using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    #region Variables
    [SerializeField]
    GameObject mouseIndicator, cellIndicator;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private ObjectDatabaseSO database;
    [SerializeField]
    private TextMeshProUGUI text;

    private int selectedObjectIndex = -1;

    private GridData sphereData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObject = new();

    private int currentPlayer = 1;

    private float resetTimer = 5f;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Initialize and set up placement variables
        StartPlacement(currentPlayer);

        // Initialize GridData for sphere placements
        sphereData = new GridData();

        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        // Check if an object is selected for placement
        if (selectedObjectIndex < 0) return;

        // Get the mouse position and corresponding grid position
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        // Check placement validity and update the preview indicator color
        bool placementValidity = CheckPlacementValidity(gridPosition);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }
    #endregion

    #region Placement Methods
    // Start the object placement for a given player
    public void StartPlacement(int ID)
    {
        // Find the selected object index in the database
        selectedObjectIndex = database.objects.FindIndex(data => data.ID == ID);

        // Check if the selected object index is valid
        if (selectedObjectIndex < 0)
        {
            Debug.LogError($"no ID found {ID}");
            return;
        }

        // Set the text based on the currentPlayer;
        text.text = (currentPlayer == 1) ? "White to play" : "Black to play";

        // Activate the cell indicator for object placement
        cellIndicator.SetActive(true);

        // Subscribe to input events for object placement
        inputManager.OnClicked += PlaceObject;
    }

    // Stop the object placement process
    private void StopPlacement()
    {
        // Reset selected object index and deactivate cell indicator
        selectedObjectIndex = -1;
        cellIndicator.SetActive(false);

        // Unsubscribe from input events for object placement
        inputManager.OnClicked -= PlaceObject;
    }

    private void PlaceObject()
    {
        // Get the mouse position and corresponding grid position
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        // Check if the placement at the grid position is valid
        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (!placementValidity) return;

        // Instantiate and position the new sphere object
        GameObject newSphere = Instantiate(database.objects[selectedObjectIndex].Prefab);
        newSphere.transform.position = grid.CellToWorld(gridPosition);
        placedGameObject.Add(newSphere);

        // Update GridData with the placed object's data
        sphereData.AddObjectAt(gridPosition,
            database.objects[selectedObjectIndex].Size,
            currentPlayer,
            selectedObjectIndex);

        // Stop object placement, rotate positions, and end the turn with delay
        StopPlacement();
        RotatePositions();
        StartCoroutine(EndTurn(0.5f));
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        return sphereData.CanPlaceObjectAt(gridPosition);
    }
    #endregion

    #region Turn Management Methods
    // Coroutine to handle the end of the turn with a delay
    private IEnumerator EndTurn(float duration)
    {
        float elapsedTime = 0f;

        // Wait for the specified duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bool whiteWinsThisTurn = sphereData.CheckWinCondition("Player1", grid);
        bool blackWinsThisTurn = sphereData.CheckWinCondition("Player2", grid);

        // Check win conditions and update game state
        if (whiteWinsThisTurn && blackWinsThisTurn)
        {
            // Both players win
            text.text = "It's a draw!";
            // Reset the game after 10 seconds
            StartCoroutine(ResetGameAfterDelay(resetTimer));
        }
        else if (whiteWinsThisTurn)
        {
            // White wins
            text.text = "White wins!";

            // Reset the game after 10 seconds
            StartCoroutine(ResetGameAfterDelay(resetTimer));
        }
        else if (blackWinsThisTurn)
        {
            // Black wins
            text.text = "Black wins!";

            // Reset the game after 10 seconds
            StartCoroutine(ResetGameAfterDelay(resetTimer));
        }
        else if (AllSpheresPlaced())
        {
            // No one wins, and all spheres are placed
            text.text = "It's a draw!";

            // Reset the game after 10 seconds
            StartCoroutine(ResetGameAfterDelay(resetTimer));
        }
        else
        {
            // Switch player turn
            currentPlayer = (currentPlayer == 1) ? 2 : 1;

            // Update the text based on the new currentPlayer
            text.text = (currentPlayer == 1) ? "White to play" : "Black to play";

            // Start object placement for the next player
            StartPlacement(currentPlayer);

            // Clear sphere positions for the next turn
            sphereData.ClearPositions();
        }
    }

    // Rotate the positions of placed game objects based on the game rules
    private void RotatePositions()
    {
        foreach (var item in placedGameObject)
        {
            // Get the old position of the game object in grid coordinates
            Vector3Int oldPosition = grid.WorldToCell(item.transform.position);

            // Update the GridData to get the new position based on game rules
            Vector3Int newPosition = sphereData.UpdateGridData(oldPosition, currentPlayer);

            // Smoothly move the game object to the new position using Lerp
            StartCoroutine(MoveObjectLerp(item.transform, grid.CellToWorld(newPosition), 0.25f));
        }
    }

    // Coroutine for smoothly moving an object using Lerp
    private IEnumerator MoveObjectLerp(Transform transform, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        // Interpolate the position over time to create a smooth movement
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object reaches the exact target position
        transform.position = targetPosition;
    }
    #endregion

    #region Reset Methods
    private bool AllSpheresPlaced()
    {
        // Check if the total number of placed spheres equals the expected total
        int totalExpectedSpheres = 16;
        return placedGameObject.Count == totalExpectedSpheres;
    }
    private void DeleteAllSpheres()
    {
        foreach (var sphere in placedGameObject)
        {
            Destroy(sphere);
        }

        placedGameObject.Clear(); // Clear the list after destroying all spheres
    }
    private IEnumerator ResetGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset all necessary variables and game state here
        currentPlayer = 1;
        text.text = "White to play";

        // Delete all instantiated spheres
        DeleteAllSpheres();

        sphereData.ClearObjects();

        // Start object placement for the first player
        StartPlacement(currentPlayer);
    }
    #endregion
}
