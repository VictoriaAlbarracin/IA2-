using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GridEntity))]
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

    public SpatialGrid spatialGrid;

    protected List<IUtilityAction> _actions;
    protected IUtilityAction _currentAction;

    protected virtual void Start()
    {
        InitBrain();
    }

    protected virtual void Update()
    {
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
    }

    protected abstract void InitBrain();

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
    // BÚSQUEDA ESPACIAL USANDO EL QUERY DE SPATIALGRID + LINQ
    // =========================================================
    public GameObject BuscarRecurso<T>(bool prioritizeNutrition, float radius = 15f) where T : Component
    {
        if (spatialGrid == null) return null;

        Vector3 min = transform.position + new Vector3(-radius, 0, -radius);
        Vector3 max = transform.position + new Vector3(radius, 0, radius);

        // Consulta circular en la SpatialGrid
        var nearbyEntities = spatialGrid.Query(min, max, pos =>
        {
            Vector3 diff = pos - transform.position;
            diff.y = 0;
            return diff.sqrMagnitude <= radius * radius;
        });

        // Filtramos por componente T usando LINQ Grupo 3 (.ToList(), .Any())
        var filteredResources = nearbyEntities
            .Where(e => e != null && e.gameObject.activeInHierarchy && e.gameObject != this.gameObject)
            .Select(e => e.GetComponent<T>())
            .Where(comp => comp != null)
            .ToList();

        if (!filteredResources.Any()) return null;

        if (prioritizeNutrition)
        {
            return filteredResources
                .OrderByDescending(r =>
                {
                    Food food = r as Food;
                    return food != null ? food.nutrition : 0f;
                })
                .FirstOrDefault()?.gameObject;
        }

        return filteredResources
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
            .FirstOrDefault()?.gameObject;
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
        // Al desactivar o destruir, si sigue en algún bucket lo sacamos moviéndolo fuera o desactivándolo
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}