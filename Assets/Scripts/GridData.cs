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
            positionsToKeep[newPosition] = data;

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
        placedObjects.Clear();
        foreach (var entry in positionsToKeep)
        {
            placedObjects[entry.Key] = entry.Value;
        }
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
    public bool CheckWinCondition(int targetID)
    {
        foreach (var position in placedObjects.Keys)
        {
            if (CheckDirection(position, targetID, new Vector3Int(1, 0, 0)) ||  // Check horizontally
                CheckDirection(position, targetID, new Vector3Int(0, 0, 1)) ||  // Check vertically
                CheckDirection(position, targetID, new Vector3Int(1, 0, 1)) ||  // Check diagonally (up-right)
                CheckDirection(position, targetID, new Vector3Int(1, 0, -1)))    // Check diagonally (up-left)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckDirection(Vector3Int start, int targetID, Vector3Int direction)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3Int currentPos = start + (direction * i);
            if (!placedObjects.TryGetValue(currentPos, out var data) || data._ID != targetID)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearObjects()
    {
        placedObjects.Clear();
        positionsToKeep.Clear();
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