using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EcosystemManager : Animal
{
    public FSM _fsm;

    [Header("Estadísticas (Necesidades)")]
    public float hunger = 0f;
    public float thirst = 0f;
    public float energy = 100f;

    [Header("Umbrales Críticos")]
    public float limitHunger = 60f;
    public float limitThirst = 70f;
    public float limitEnergy = 30f;

    protected override void Start()
    {
        base.Start();

        // Inicializamos la FSM y sus 4 comportamientos obligatorios
        _fsm = new FSM();
        _fsm.AddState("Free", new FreeState(this));
        _fsm.AddState("Eat", new EatState(this));
        _fsm.AddState("Drink", new DrinkState(this));
        _fsm.AddState("Rest", new RestState(this));

        _fsm.ChangeState("Free");
    }

    protected override void Update()
    {
        // 1. Modificación de necesidades con el paso del tiempo
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        // 2. Ejecutar la inteligencia actual
        _fsm.Execute();

        // 3. Aplicar el movimiento físico heredado de Animal
        base.Update();
    }

    // ==============================================================
    // GENERATOR + LAZY EVALUATION (20 Puntos)
    // ==============================================================
    public IEnumerable<GameObject> GeneradorRecursosValidos(List<GameObject> poolObjetos)
    {
        foreach (var recurso in poolObjetos)
        {
            // Se evalúa uno a uno de forma perezosa. Si se destruyó, se ignora.
            if (recurso != null && recurso.activeInHierarchy)
            {
                yield return recurso;
            }
        }
    }

    // ==============================================================
    // CONSULTAS LINQ (30 Puntos - 1 Función de cada grupo)
    // ==============================================================
    public GameObject BuscarRecursoOptimo(string targetTag)
    {
        // Para automatizar y no arrastrar prefabs a mano, buscamos los tags dinámicamente
        List<GameObject> todosLosObjetos = GameObject.FindGameObjectsWithTag(targetTag).ToList(); // Grupo 3: ToList

        var recursosFiltrados = GeneradorRecursosValidos(todosLosObjetos);

        // Grupo 3: Any (Validación de seguridad antes de procesar cálculos pesados)
        if (!recursosFiltrados.Any())
            return null;

        return recursosFiltrados
            .Where(r => r.CompareTag(targetTag)) // Grupo 1: Where
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position)) // Grupo 2: OrderBy (Elige el más cercano)
            .FirstOrDefault(); // Grupo 1: FirstOrDefault
    }
}