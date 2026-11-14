using System;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    public bool onGrid;
    private Vector3 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
        OnMove(this); // Registro inicial en la grilla
    }

    private void Update()
    {
        // Solo avisa a la SpatialGrid cuando el animal realmente cambió de posición por su IA
        if (transform.position != _lastPosition)
        {
            _lastPosition = transform.position;
            OnMove(this);
        }
    }
}