using System;
using System.Collections.Generic;
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
    public void UpdateGridData(Vector3Int oldPosition, Vector3Int newPosition)
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

    private Vector3Int MoveInInnerCells(Vector3Int oldPosition)
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


