using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EcosystemManager : Animal
{
    [Header("Estadísticas (Necesidades)")]
    public float hunger = 0f;
    public float thirst = 0f;
    public float energy = 100f;

    [Header("Patrullaje")]
    public Transform[] waypoints;

    // Lista de comportamientos evaluables
    private List<IState> _states;
    private IState _currentState;

    protected override void Start()
    {
        base.Start();

        // Instanciamos los estados
        _states = new List<IState>
        {
            new EatState(),
            new DrinkState(),
            new RestState(),
            new FreeState()
        };
    }

    protected override void Update()
    {
        // 1. Modificación de necesidades en el tiempo
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        // 2. Selección del estado prioritario usando LINQ
        _currentState = _states
            .OrderByDescending(s => s.GetScore(this))
            .FirstOrDefault();

        // 3. Ejecución del comportamiento
        _currentState?.Execute(this);

        // 4. Movimiento físico base
        base.Update();
    }

    // ==============================================================
    // GENERATOR + LINQ PARA RECURSOS
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