using UnityEngine;

public class Water : MonoBehaviour
{
    [Header("Fuente de Agua")]
    public float hydrationAmount = 50f;

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