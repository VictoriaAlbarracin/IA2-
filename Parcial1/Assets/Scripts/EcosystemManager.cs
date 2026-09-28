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
            new Actions { Name = "Patrol", GetScore = () => 30f,           Execute = Patrol},
            new Actions { Name = "Comer",     GetScore = () => hunger,        Execute = Comer },
            new Actions { Name = "Beber",     GetScore = () => thirst * 1.2f, Execute = Beber },
            new Actions { Name = "Descansar", GetScore = () => 100f - energy, Execute = Descansar }
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

        if (energy <= 30f)
        {
            Debug.Log("OVEJA: DESCANSANDO");
            Descansar();
            return;
        }

        if (thirst >= 60f)
        {
            Debug.Log("OVEJA: BUSCANDO AGUA");
            Beber();
            return;
        }

        if (hunger >= 60f)
        {
            Debug.Log("OVEJA: BUSCANDO COMIDA");
            Comer();
            return;
        }

        Debug.Log("OVEJA: PATRULLANDO");
        Patrol();
    }

    public void Movement(Vector3 destino) 
    {
        destino.y = transform.position.y;

        Vector3 posicionAnterior = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            speed * Time.deltaTime
        );

    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Si llego al waypoint o no tiene uno asignado busca otro al azar
        if (_currentWaypoint == null || Vector3.Distance(transform.position, _currentWaypoint.position) < 1.5f)
        {
            int index = Random.Range(0, waypoints.Length);
            _currentWaypoint = waypoints[index];
        }

        Movement(_currentWaypoint.position);
    }

    void Comer()
    {
        var targetFood = SearchResource("Food");

        if (targetFood == null)
        {
            Patrol();
            return;
        }

        if (Vector3.Distance(transform.position, targetFood.transform.position) < 1.5f)
        {
            Destroy(targetFood);
            hunger = 0f;
        }
        else
        {
            Movement(targetFood.transform.position);
        }
    }

    void Beber()
    {
        GameObject targetWater = SearchResource("Water");

        if (targetWater == null)
        {
            Patrol();
            return;
        }

        float distancia = Vector3.Distance(
            transform.position,
            targetWater.transform.position
        );

        if (distancia < 1.5f)
        {
            thirst = 0f;
        }
        else
        {
            Movement(targetWater.transform.position);
        }
    }

    void Descansar()
    {
        var restZone = SearchResource("Rest");

        if (restZone == null)
        {
            Patrol();
            return;
        }

        if (Vector3.Distance(transform.position, restZone.transform.position) > 2f)
        {
            Movement(restZone.transform.position);
        }
        else
        {
            energy = 100;
            energy = Mathf.Clamp(energy, 0f, 100f);
        }
    }


    public IEnumerable<GameObject> ResourceManager(List<GameObject> poolObjetos)
    {
        foreach (var resource in poolObjetos)
        {
            if (resource != null && resource.activeInHierarchy)
            {
                yield return resource;
            }
        }
    }

    public GameObject SearchResource(string targetTag)
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(targetTag);

        GameObject recursoMasCercano = null;
        float distanciaMasCercana = Mathf.Infinity;

        foreach (GameObject objeto in objetos)
        {
            // No buscar la propia oveja como recurso
            if (objeto == gameObject)
                continue;

            float distancia = Vector3.Distance(
                transform.position,
                objeto.transform.position
            );

            if (distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                recursoMasCercano = objeto;
            }
        }

        return recursoMasCercano;
    }
}