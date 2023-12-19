using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        List<Vector3Int> positionToOcupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOcupy, ID, placedObjectIndex);
        foreach (var pos in positionToOcupy)
        {
            if (placedObjects.ContainsKey(pos)) return;
            placedObjects[pos] = data;
        }
    }
    public void UpdateGridData(Vector3Int oldPosition, out Vector3Int newPositionInGrid)
    {
        // Check if the object is in the inner grid
        if (IsInInnerGrid(oldPosition))
        {
            // Call the function for inner grid movement
            Vector3Int newPositionInInnerGrid = MoveObjectsInInnerCell(oldPosition);
            newPositionInGrid = newPositionInInnerGrid;
            // Move the object in the inner grid
            MoveObject(oldPosition, newPositionInInnerGrid);
        }
        else
        {
            // Call the function for outer grid movement
            Vector3Int newPositionInOuterGrid = MoveObjectInOuterCell(oldPosition);
            newPositionInGrid = newPositionInOuterGrid;
            // Move the object in the outer grid
            MoveObject(oldPosition, newPositionInOuterGrid);
        }
    }

    private void MoveObject(Vector3Int oldPosition, Vector3Int newPosition)
    {
        if (placedObjects.ContainsKey(oldPosition))
        {
            PlacementData data = placedObjects[oldPosition];

            // Remove the object from the old position
            placedObjects.Remove(oldPosition);

            // Add the object to the new position
            placedObjects[newPosition] = data;
        }
    }
    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new List<Vector3Int>();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)  // Fix: Change x to y here
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize) 
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach(var pos in positionToOccupy) 
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }
    private bool IsInInnerGrid(Vector3Int position)
    {
        return position.x >= 1 && position.x <= 2 && position.z >= 1 && position.z <= 2;
    }
    private Vector3Int MoveObjectsInInnerCell(Vector3Int oldPosition)
    {
        Vector3Int newPosition = new Vector3Int();

        switch (oldPosition)
        {
            case var _ when oldPosition == new Vector3Int(1, 0, 2):
                newPosition = new Vector3Int(2, 0, 2);
                break;
            case var _ when oldPosition == new Vector3Int(2, 0, 2):
                newPosition = new Vector3Int(2, 0, 1);
                break;
            case var _ when oldPosition == new Vector3Int(2, 0, 1):
                newPosition = new Vector3Int(1, 0, 1);
                break;
            case var _ when oldPosition == new Vector3Int(1, 0, 1):
                newPosition = new Vector3Int(1, 0, 2);
                break;
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
}
   


public class PlacementData
{
    public List<Vector3Int> _occupiedPositions;
    public int _ID { get; private set; }
    public int _placedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int ID, int placedObjectIndex)
    {
        _occupiedPositions = occupiedPositions;
        _ID = ID;
        _placedObjectIndex = placedObjectIndex;
    }
}


