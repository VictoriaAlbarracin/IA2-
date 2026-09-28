using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EcosystemManager : MonoBehaviour
{
    [Header("Necesidades")]
    public float hunger = 0f;
    public float thirst = 0f;
    public float energy = 100f;

    [Header("Velocidad de Movimiento")]
    public float speed = 3.5f;

    [Header("Comportamiento libre (Waypoints)")]
    public Transform[] waypoints;
    private Transform _currentWaypoint;

    // Lista evaluada por LINQ
    private List<Actions> _actions;
    private Actions _currentAction;

    private void Start()
    {
        // Vinculamos cada cálculo de urgencia con su método correspondiente
        _actions = new List<Actions>
        {
            new Actions { Name = "Patrullar", GetScore = () => 30f,           Execute = Patrullar },
            new Actions { Name = "Comer",     GetScore = () => hunger,        Execute = Comer },
            new Actions { Name = "Beber",     GetScore = () => thirst * 1.2f, Execute = Beber },
            new Actions { Name = "Descansar", GetScore = () => 100f - energy, Execute = Descansar }
        };
    }

    private void Update()
    {
        // 1. Modificación constante de necesidades
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        // 2. LINQ: Evalúa la urgencia de cada acción y elige la mayor
        _currentAction = _actions
            .OrderByDescending(a => a.GetScore())
            .FirstOrDefault();

        // 3. Ejecuta la acción prioritaria
        _currentAction?.Execute();
    }

    // ==============================================================
    // TRASLADO DIRECTO (Sin físicas vectoriales)
    // ==============================================================
    public void MoverHacia(Vector3 destino)
    {
        destino.y = transform.position.y; // Mantiene fija la altura para que no flote ni se hunda
        transform.position = Vector3.MoveTowards(transform.position, destino, speed * Time.deltaTime);
    }

    // ==============================================================
    // COMPORTAMIENTOS
    // ==============================================================
    void Patrullar()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Si llegó al waypoint o no tiene uno asignado, busca otro al azar
        if (_currentWaypoint == null || Vector3.Distance(transform.position, _currentWaypoint.position) < 1.5f)
        {
            int index = Random.Range(0, waypoints.Length);
            _currentWaypoint = waypoints[index];
        }

        MoverHacia(_currentWaypoint.position);
    }

    void Comer()
    {
        var targetFood = BuscarRecursoOptimo("Food");
        if (targetFood == null) return;

        if (Vector3.Distance(transform.position, targetFood.transform.position) < 1.5f)
        {
            Destroy(targetFood);
            hunger = 0f;
        }
        else
        {
            MoverHacia(targetFood.transform.position);
        }
    }

    void Beber()
    {
        var targetWater = BuscarRecursoOptimo("Water");
        if (targetWater == null) return;

        if (Vector3.Distance(transform.position, targetWater.transform.position) < 1.5f)
        {
            // Al estar en el agua se detiene y bebe
            thirst -= Time.deltaTime * 40f;
            if (thirst < 0) thirst = 0;
        }
        else
        {
            MoverHacia(targetWater.transform.position);
        }
    }

    void Descansar()
    {
        var restZone = BuscarRecursoOptimo("Descanso");
        if (restZone == null) return;

        if (Vector3.Distance(transform.position, restZone.transform.position) > 2f)
        {
            MoverHacia(restZone.transform.position);
        }
        else
        {
            // Al llegar a la zona descansa en el lugar
            energy += Time.deltaTime * 20f;
            if (energy > 100f) energy = 100f;
        }
    }

    // ==============================================================
    // GENERATOR + LINQ (Búsqueda de recursos)
    // ==============================================================
    public IEnumerable<GameObject> GeneradorRecursosValidos(List<GameObject> poolObjetos)
    {
        foreach (var recurso in poolObjetos)
        {
            if (recurso != null && recurso.activeInHierarchy)
            {
                yield return recurso;
            }
        }
    }

    public GameObject BuscarRecursoOptimo(string targetTag)
    {
        List<GameObject> todosLosObjetos = GameObject.FindGameObjectsWithTag(targetTag).ToList();
        var recursosFiltrados = GeneradorRecursosValidos(todosLosObjetos);

        if (!recursosFiltrados.Any())
            return null;

        return recursosFiltrados
            .Where(r => r.CompareTag(targetTag))
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
            .FirstOrDefault();
    }
}