using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new Dictionary<Vector3Int, PlacementData>();
    private HashSet<Vector3Int> positionsToKeep = new HashSet<Vector3Int>();
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        Vector3Int positionToOcupy = CalculatePosition(gridPosition);
        PlacementData data = new PlacementData(positionToOcupy, ID, placedObjectIndex);
        
        if (placedObjects.ContainsKey(positionToOcupy))
            return;
        
        placedObjects[positionToOcupy] = data;
        
    }
    public Vector3Int UpdateGridData(Vector3Int oldPosition)
    {
        // Check if the object is in the inner grid
        if (IsInInnerGrid(oldPosition))
        {
            // Call the function for inner grid movement
            Vector3Int newPositionInInnerGrid = MoveObjectsInInnerCell(oldPosition);
            // Move the object in the inner grid
            return MoveObject(oldPosition, newPositionInInnerGrid);

        }
        else
        {
            // Call the function for outer grid movement
            Vector3Int newPositionInOuterGrid = MoveObjectInOuterCell(oldPosition);
            // Move the object in the outer grid
            return MoveObject(oldPosition, newPositionInOuterGrid);
        }
    }

    private Vector3Int MoveObject(Vector3Int oldPosition, Vector3Int newPosition)
    {
        if (placedObjects.ContainsKey(oldPosition))
        {
            PlacementData data = placedObjects[oldPosition];
            positionsToKeep.Add(newPosition);
            // Add the object to the new position
            placedObjects[newPosition] = data;

            if(!positionsToKeep.Contains(oldPosition))
                placedObjects.Remove(oldPosition);

            Debug.Log($"Moved object from {oldPosition} to {newPosition}");
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
    public bool CheckFourSpheresInARow(int targetID)
    {
        foreach (var position in placedObjects.Keys)
        {
            PlacementData data = placedObjects[position];

            // Check only for spheres of the target ID
            if (data._ID == targetID)
            {
                if (CheckAdjacentSpheres(position, targetID) || CheckDiagonalSpheres(position, targetID))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool CheckAdjacentSpheres(Vector3Int position, int targetID)
    {
        int count = 0;

        // Check horizontally
        count += CountAdjacentSpheres(position, new Vector3Int(1, 0, 0), targetID);
        count += CountAdjacentSpheres(position, new Vector3Int(-1, 0, 0), targetID);

        // Check vertically
        count += CountAdjacentSpheres(position, new Vector3Int(0, 0, 1), targetID);
        count += CountAdjacentSpheres(position, new Vector3Int(0, 0, -1), targetID);

        return count >= 3;
    }
    private bool CheckDiagonalSpheres(Vector3Int position, int targetID)
    {
        int count = 0;

        // Check diagonally (bottom-left to top-right)
        count += CountAdjacentSpheres(position, new Vector3Int(1, 0, 1), targetID);
        count += CountAdjacentSpheres(position, new Vector3Int(-1, 0, -1), targetID);

        // Check diagonally (top-left to bottom-right)
        count += CountAdjacentSpheres(position, new Vector3Int(1, 0, -1), targetID);
        count += CountAdjacentSpheres(position, new Vector3Int(-1, 0, 1), targetID);

        return count >= 3;
    }
    private int CountAdjacentSpheres(Vector3Int position, Vector3Int direction, int targetID)
    {
        int count = 0;

        for (int i = 1; i < 4; i++)
        {
            Vector3Int adjacentPosition = position + (direction * i);

            if (placedObjects.ContainsKey(adjacentPosition))
            {
                if (placedObjects[adjacentPosition]._ID == targetID)
                {
                    count++;
                }
                else
                {
                    break; // Stop counting if a different ID is encountered
                }
            }
            else
            {
                // Position not found in dictionary, break or handle accordingly
                break;
            }
        }

        return count;
    }
}
public class PlacementData
{
    public Vector3Int _occupiedPositions;
    public int _ID { get; private set; }
    public int _placedObjectIndex { get; private set; }

    public PlacementData(Vector3Int occupiedPositions, int ID, int placedObjectIndex)
    {
        _occupiedPositions = occupiedPositions;
        _ID = ID;
        _placedObjectIndex = placedObjectIndex;
    }
}


