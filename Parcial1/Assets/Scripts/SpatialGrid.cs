using System.Collections.Generic;
using UnityEngine;

public class SpatialGrid : MonoBehaviour
{
    public float cellSize = 10f;

    private Dictionary<Vector2Int, List<GameObject>> _grid = new Dictionary<Vector2Int, List<GameObject>>();
    private Dictionary<GameObject, Vector2Int> _entityCellMap = new Dictionary<GameObject, Vector2Int>();

    private Vector2Int GetCellCoordinates(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    public void RegisterEntity(GameObject entity)
    {
        if (entity == null) return;
        Vector2Int cell = GetCellCoordinates(entity.transform.position);

        if (!_grid.ContainsKey(cell)) _grid[cell] = new List<GameObject>();

        _grid[cell].Add(entity);
        _entityCellMap[entity] = cell;
    }

    public void RemoveEntity(GameObject entity)
    {
        if (entity == null || !_entityCellMap.ContainsKey(entity)) return;

        Vector2Int cell = _entityCellMap[entity];
        if (_grid.ContainsKey(cell))
        {
            _grid[cell].Remove(entity);
            if (_grid[cell].Count == 0) _grid.Remove(cell);
        }
        _entityCellMap.Remove(entity);
    }

    public void UpdateEntityPosition(GameObject entity)
    {
        if (entity == null || !_entityCellMap.ContainsKey(entity)) return;

        Vector2Int currentCell = _entityCellMap[entity];
        Vector2Int newCell = GetCellCoordinates(entity.transform.position);

        if (currentCell != newCell)
        {
            _grid[currentCell].Remove(entity);
            if (_grid[currentCell].Count == 0) _grid.Remove(currentCell);

            if (!_grid.ContainsKey(newCell)) _grid[newCell] = new List<GameObject>();

            _grid[newCell].Add(entity);
            _entityCellMap[entity] = newCell;
        }
    }

    public IEnumerable<GameObject> GetEntitiesInRadius(Vector3 center, float radius)
    {
        Vector2Int minCell = GetCellCoordinates(new Vector3(center.x - radius, 0, center.z - radius));
        Vector2Int maxCell = GetCellCoordinates(new Vector3(center.x + radius, 0, center.z + radius));

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (_grid.ContainsKey(cell))
                {
                    foreach (var entity in _grid[cell])
                    {
                        if (entity != null && entity.activeInHierarchy)
                        {
                            yield return entity;
                        }
                    }
                }
            }
        }
    }
}