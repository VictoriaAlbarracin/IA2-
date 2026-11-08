using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EcosystemManager : MonoBehaviour
{
    [Header("Necessities")]
    public float hunger = 0f;
    public float thirst = 0f;
    public float energy = 100f;

    [Header("Movement Velocity")]
    public float speed = 3.5f;

    [Header("Free Behaviour")]
    public Transform[] waypoints;

    [Header("Resources")]
    public List<GameObject> resourcePool;

    private List<IUtilityAction> _actions;
    private IUtilityAction _currentAction;

    private void Start()
    {
        // Instanciamos las clases polimórficas de manera limpia
        _actions = new List<IUtilityAction>
        {
            new PatrolAction(),
            new EatAction(),
            new DrinkAction(),
            new RestAction()
        };
    }

    private void Update()
    {
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        hunger = Mathf.Clamp(hunger, 0f, 100f);
        thirst = Mathf.Clamp(thirst, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);

        if (hunger >= 100f || thirst >= 100f)
        {
            Destroy(gameObject);
            return;
        }

        EvaluateAction();
    }

    private void EvaluateAction()
    {
        // =========================================================
        // TOMA DE DECISIONES CON LINQ
        // Grupo 1: Where, FirstOrDefault | Grupo 2: OrderByDescending
        // =========================================================
        _currentAction = _actions
            .Where(action => action.GetScore(this) >= 10f) // Descarta acciones sin urgencia
            .OrderByDescending(action => action.GetScore(this))
            .FirstOrDefault();

        if (_currentAction != null)
        {
            _currentAction.Execute(this);
        }
    }

    // =========================================================
    // GENERATOR: Removido el CompareTag para que sea puro
    // =========================================================
    private IEnumerable<GameObject> GenerateResources()
    {
        foreach (GameObject resource in resourcePool)
        {
            if (resource != null && resource.activeInHierarchy && resource != gameObject)
            {
                yield return resource;
            }
        }
    }

    // =========================================================
    // CONSULTAS LINQ PARA BÚSQUEDA
    // =========================================================
    public GameObject BuscarRecurso(string targetTag, bool prioritizeNutrition)
    {
        var activeResources = GenerateResources();

        // Grupo 1: Aplicamos el filtro de Tag mediante LINQ en lugar del generador
        var filteredResources = activeResources.Where(r => r.CompareTag(targetTag));

        // Grupo 3: Any - Verificamos con evaluación temprana si el filtro dejó resultados
        if (!filteredResources.Any())
            return null;

        if (prioritizeNutrition)
        {
            return filteredResources
                .OrderByDescending(r => // Grupo 2: OrderByDescending
                {
                    Food food = r.GetComponent<Food>();
                    return food != null ? food.nutrition : 0f;
                })
                .FirstOrDefault(); // Grupo 1
        }

        return filteredResources
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position)) // Grupo 2: OrderBy
            .FirstOrDefault(); // Grupo 1
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

    // Usado por las acciones cuando el objetivo desaparece o no se encuentra
    public void ExecuteFallbackPatrol()
    {
        var patrol = _actions.OfType<PatrolAction>().FirstOrDefault(); // Grupo 3: OfType
        patrol?.Execute(this);
    }
}