using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Type of food")]
    public string foodName;
    public float nutrition = 25f;

    private SpatialGrid _spatialGrid;

    private void Start()
    {
        _spatialGrid = FindFirstObjectByType<SpatialGrid>();
        if (_spatialGrid != null)
        {
            _spatialGrid.RegisterEntity(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_spatialGrid != null)
        {
            _spatialGrid.RemoveEntity(gameObject);
        }
    }
}