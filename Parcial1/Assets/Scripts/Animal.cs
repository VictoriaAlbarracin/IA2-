using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Animal : MonoBehaviour
{
    [Header("Necesidades Básicas")]
    public float hunger = 0f;
    public float thirst = 0f;
    public float energy = 100f;

    [Header("Velocidad de Movimiento")]
    public float speed = 3.5f;

    [Header("Comportamiento Autónomo")]
    public Transform[] waypoints;

    // Referencia inyectada para optimizar las consultas espaciales en el TP2
    public SpatialGrid spatialGrid;

    protected List<IUtilityAction> _actions;
    protected IUtilityAction _currentAction;

    protected virtual void Start()
    {
        // 1. Obliga a que cada animal defina su cerebro en sus scripts hijos
        InitBrain();
    }

    protected virtual void Update()
    {
        // Desgaste biológico en el tiempo
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        hunger = Mathf.Clamp(hunger, 0f, 100f);
        thirst = Mathf.Clamp(thirst, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);

        if (hunger >= 100f || thirst >= 100f)
        {
            Die();
            return;
        }

        EvaluateAction();

        // TP 2: Actualizamos la posición del animal en la grilla para que otros lo detecten
        if (spatialGrid != null)
        {
            spatialGrid.UpdateEntityPosition(this.gameObject);
        }
    }

    // Método que deberán sobreescribir Oveja y Lobo
    protected abstract void InitBrain();

    // =========================================================
    // TOMA DE DECISIONES
    // =========================================================
    protected virtual void EvaluateAction()
    {
        if (_actions == null || _actions.Count == 0) return;

        _currentAction = _actions
            .Where(action => action.GetScore(this) >= 10f)
            .OrderByDescending(action => action.GetScore(this))
            .FirstOrDefault();

        _currentAction?.Execute(this);
    }

    // =========================================================
    // BÚSQUEDA ESPACIAL A TRAVÉS DE LA GRILLA (TP2)
    // =========================================================
    public GameObject BuscarRecurso(string targetTag, bool prioritizeNutrition)
    {
        if (spatialGrid == null) return null;

        // En lugar de buscar en todo el mapa, solo le pedimos a la grilla nuestra zona
        var nearbyEntities = spatialGrid.GetEntitiesInRadius(transform.position, 15f);

        // Aplicamos LINQ sobre los objetos cercanos
        var filteredResources = nearbyEntities
            .Where(e => e != null && e.activeInHierarchy && e.CompareTag(targetTag) && e != this.gameObject);

        if (!filteredResources.Any()) return null;

        if (prioritizeNutrition)
        {
            return filteredResources
                .OrderByDescending(r =>
                {
                    Food food = r.GetComponent<Food>();
                    return food != null ? food.nutrition : 0f;
                })
                .FirstOrDefault();
        }

        return filteredResources
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
            .FirstOrDefault();
    }

    public void Movement(Vector3 objective)
    {
        objective.y = transform.position.y;
        transform.position = Vector3.MoveTowards(
            transform.position,
            objective,
            speed * Time.deltaTime
        );
    }

    public void ExecuteFallbackPatrol()
    {
        var patrol = _actions.OfType<PatrolAction>().FirstOrDefault();
        patrol?.Execute(this);
    }

    public virtual void Die()
    {
        if (spatialGrid != null)
        {
            spatialGrid.RemoveEntity(this.gameObject);
        }
        Destroy(gameObject);
    }
}