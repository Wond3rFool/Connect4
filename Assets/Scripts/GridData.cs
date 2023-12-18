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
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary arleady contains this cell position {pos}");
            placedObjects[pos] = data;
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


