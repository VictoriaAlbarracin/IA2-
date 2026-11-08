using UnityEngine;

public class RestZone : MonoBehaviour
{
    [Header("Zona de Descanso")]
    public float restBonus = 1.5f;

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