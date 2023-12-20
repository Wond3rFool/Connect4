using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridData
{
    #region Variable Storage
    private Dictionary<Vector3Int, PlacementData> placedObjects = new Dictionary<Vector3Int, PlacementData>();
    private Dictionary<Vector3Int, PlacementData> positionsToKeep = new Dictionary<Vector3Int, PlacementData>();
    #endregion

    #region Object Placement
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        Vector3Int positionToOcupy = CalculatePosition(gridPosition);
        PlacementData data = new PlacementData(ID, placedObjectIndex);

        if (placedObjects.ContainsKey(positionToOcupy))
            return;

        placedObjects[positionToOcupy] = data;

    }
    private Vector3Int CalculatePosition(Vector3Int gridPosition)
    {
        return gridPosition;
    }
    public bool CanPlaceObjectAt(Vector3Int gridPosition)
    {
        Vector3Int positionToOccupy = CalculatePosition(gridPosition);

        if (placedObjects.ContainsKey(positionToOccupy))
            return false;

        return true;
    }
    #endregion

    #region Grid and Object Manipulation
    public Vector3Int UpdateGridData(Vector3Int oldPosition, int ID)
    {
        // Check if the object is in the inner grid
        if (IsInInnerGrid(oldPosition))
        {
            // Call the function for inner grid movement
            Vector3Int newPositionInInnerGrid = MoveObjectsInInnerCell(oldPosition);
            // Move the object in the inner grid
            return MoveObject(oldPosition, newPositionInInnerGrid, ID);

        }
        else
        {
            // Call the function for outer grid movement
            Vector3Int newPositionInOuterGrid = MoveObjectInOuterCell(oldPosition);
            // Move the object in the outer grid
            return MoveObject(oldPosition, newPositionInOuterGrid, ID);
        }
    }
    private Vector3Int MoveObject(Vector3Int oldPosition, Vector3Int newPosition, int ID)
    {
        if (placedObjects.ContainsKey(oldPosition))
        {
            PlacementData data = placedObjects[oldPosition];
            // Add the object to the new position
            placedObjects[newPosition] = data;
            positionsToKeep[newPosition] = data;

            // Check if the old position is in positionsToKeep
            if (!positionsToKeep.ContainsKey(oldPosition))
            {
                // If it's not in positionsToKeep, remove it
                placedObjects.Remove(oldPosition);
                
            }
            return newPosition;
        }
        else
        {
            Debug.LogError($"Object not found at {oldPosition}");
            return newPosition;
        }
    }
    public void ClearPositions()
    {
        positionsToKeep.Clear();
    }
    private bool IsInInnerGrid(Vector3Int position)
    {
        return position.x >= 1 && position.x <= 2 && position.z >= 1 && position.z <= 2;
    }
    private Vector3Int MoveObjectsInInnerCell(Vector3Int oldPosition)
    {
        Vector3Int newPosition = new Vector3Int();

        if (oldPosition == new Vector3Int(1, 0, 2))
        {
            newPosition = new Vector3Int(2, 0, 2);
        }
        else if (oldPosition == new Vector3Int(2, 0, 2))
        {
            newPosition = new Vector3Int(2, 0, 1);
        }
        else if (oldPosition == new Vector3Int(2, 0, 1))
        {
            newPosition = new Vector3Int(1, 0, 1);
        }
        else if (oldPosition == new Vector3Int(1, 0, 1))
        {
            newPosition = new Vector3Int(1, 0, 2);
        }

        return newPosition;
    }
    private Vector3Int MoveObjectInOuterCell(Vector3Int oldPosition)
    {
        Vector3Int newPosition = new Vector3Int(oldPosition.x, oldPosition.y, oldPosition.z);

        if (oldPosition.x == 0 && oldPosition.z < 3)  // Check if moving right stays within the outer grid
        {
            newPosition.z += 1;
            return newPosition;
        }
        if (oldPosition.x < 3 && oldPosition.z == 3)  // Check if moving right stays within the outer grid
        {
            newPosition.x += 1;
            return newPosition;
        }

        if (oldPosition.x == 3 && oldPosition.z > 0)  // Check if moving down stays within the outer grid
        {
            newPosition.z -= 1;
            return newPosition;
        }

        if (oldPosition.x > 0 && oldPosition.z == 0)  // Check if moving left stays within the outer grid
        {
            newPosition.x -= 1;
            return newPosition;
        }
        return newPosition;
    }
    #endregion

    #region Grid Searching and clearing
    public bool CheckWinCondition(string playerTag, Grid grid)
    {
        foreach (var position in placedObjects.Keys)
        {
            if (CheckConsecutive(position, playerTag, new Vector3Int(1, 0, 0), grid) ||  // Check horizontally (right)
                CheckConsecutive(position, playerTag, new Vector3Int(0, 0, 1), grid) ||  // Check vertically (down)
                CheckConsecutive(position, playerTag, new Vector3Int(1, 0, 1), grid) ||  // Check diagonally (up-right)
                CheckConsecutive(position, playerTag, new Vector3Int(1, 0, -1), grid))    // Check diagonally (up-left)
            {
                Debug.Log($"Win condition met for player with tag {playerTag} at position {position}");
                return true;
            }
        }

        return false;
    }
    private bool CheckConsecutive(Vector3Int start, string playerTag, Vector3Int direction, Grid grid)
    {
        int consecutiveCount = 0;

        for (int i = -2; i <= 2; i++) // Iterate over a larger range to check three consecutive placements
        {
            Vector3Int currentPos = start + (direction * i);

            // Use GameObject.FindGameObjectsWithTag to find game objects with the specified tag
            GameObject[] spheresWithTag = GameObject.FindGameObjectsWithTag(playerTag);

            // Convert the positions of the spheres to grid positions
            Vector3Int[] sphereGridPositions = Array.ConvertAll(spheresWithTag, sphere => grid.WorldToCell(sphere.transform.position));

            // Check if the current position is in the array of grid positions
            if (Array.Exists(sphereGridPositions, pos => pos == currentPos))
            {
                consecutiveCount++;

                if (consecutiveCount == 4)
                {
                    Debug.Log($"Consecutive count met for player with tag {playerTag}");
                    return true;
                }
            }
            else
            {
                consecutiveCount = 0; // Reset count if there's a gap in the sequence
            }
        }

        return false;
    }
    public void ClearObjects()
    {
        placedObjects.Clear();
    }
    #endregion
}
public class PlacementData
{
    public int _ID { get; private set; }
    public int _placedObjectIndex { get; private set; }

    public PlacementData(int ID, int placedObjectIndex)
    {
        _ID = ID;
        _placedObjectIndex = placedObjectIndex;
    }
    public void SetID(int ID)
    {
        _ID = ID;
    }
}