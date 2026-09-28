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

    private Transform _currentWaypoint;

    [Header("Resources")]
    [SerializeField] private List<GameObject> resourcePool; //aca ponemos los objetos

    private List<Actions> _actions;
    private Actions _currentAction;


    private void Start()
    {
        _actions = new List<Actions>
        {
            new Actions
            {
                Name = "Patrol", 
                GetScore = () => 30f, //si no hay nada a mas de 30f que patruye
                Execute = Patrol
            },

            new Actions
            {
                Name = "Comer",
                GetScore = () => hunger,
                Execute = Comer
            },

            new Actions
            {
                Name = "Beber",
                GetScore = () => thirst * 1.2f,
                Execute = Beber
            },

            new Actions
            {
                Name = "Descansar",
                GetScore = () => 100f - energy,
                Execute = Descansar
            }
        };
    }

    private void Update()
    {
        //para aumentar las necesidades con el tiempo
        hunger += Time.deltaTime * 3f;
        thirst += Time.deltaTime * 4f;
        energy -= Time.deltaTime * 2f;

        //limitamos los valores entre 0 y 100
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        thirst = Mathf.Clamp(thirst, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);

        if (hunger >= 100f || thirst >= 100f) //Se muere si llega al limite de hambre o sed
        {
            Destroy(gameObject);
            return;
        }

        EvaluateAction();

    }

    private void EvaluateAction() //evalua las acciones tomando como prioridad que descanse, si no descansa sigue evaluando las demas
    {
        //descansar es la prioridad
        if (energy <= 30f)
        {
            _currentAction = _actions
                .FirstOrDefault(action => action.Name == "Descansar"); 

            if (_currentAction != null)
            {
                _currentAction.Execute();
            }
            return;
        }

        // si tiene la energia suficiente evalua el resto de las acciones
        _currentAction = _actions
            .Where(action => action.Name != "Descansar") //filtra las acciones descartando que descanse ya que no es una urgencia
            .Where(action => action.GetScore() > 10f) //descarta los score que no sean urgentes
            .OrderByDescending(action => action.GetScore()) //ordena los scores del mas alto al mas bajo
            .FirstOrDefault(); //toma el primer elemento

        if (_currentAction != null) //ejecuta la accion que sea necesaria
        {
            _currentAction.Execute();
        }
    }

    private IEnumerable<GameObject> GenerateResources(string targetTag)
    {
        foreach (GameObject resource in resourcePool)
        {
            if (resource != null &&
                resource.activeInHierarchy &&
                resource.CompareTag(targetTag) &&
                resource != gameObject)
            {
                yield return resource; // entrega un recurso cada vez que es solicitado.
            }
        }
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


    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (_currentWaypoint == null ||
            Vector3.Distance(
                transform.position,
                _currentWaypoint.position
            ) < 1.5f)
        {
            int index = Random.Range(0, waypoints.Length);

            _currentWaypoint = waypoints[index];
        }

        Movement(_currentWaypoint.position);
    }

    private void Comer()
    {
        //busca la comida con mayor nutrición.
        GameObject targetFood = SearchResource(
            "Food",
            true
        );

        if (targetFood == null)
        {
            Patrol();
            return;
        }

        if (Vector3.Distance(
                transform.position,
                targetFood.transform.position
            ) < 1.5f)
        {
            Food food = targetFood.GetComponent<Food>();

            if (food != null)
            {
                hunger -= food.nutrition;

                hunger = Mathf.Max(hunger, 0f); //despues de comer el hambre vuelve a 0
            }

            Destroy(targetFood);
        }
        else
        {
            Movement(targetFood.transform.position);
        }
    }

    private void Beber()
    {
        // con OrderBy busca el agua más cercana.

        GameObject targetWater = SearchResource(
            "Water",
            false
        );

        if (targetWater == null)
        {
            Patrol();
            return;
        }

        if (Vector3.Distance(
                transform.position,
                targetWater.transform.position
            ) < 1.5f)
        {
            thirst = 0f;
        }
        else
        {
            Movement(targetWater.transform.position);
        }
    }

    private void Descansar()
    {
        // con OrderBy busca la restzone mas cercana

        GameObject restZone = SearchResource(
            "Rest",
            false
        );

        if (restZone == null)
        {
            Patrol();
            return;
        }

        if (Vector3.Distance(
                transform.position,
                restZone.transform.position
            ) > 2f)
        {
            Movement(restZone.transform.position);
        }
        else
        {
            energy = 100f;
        }
    }

    private GameObject SearchResource(
        string targetTag,
        bool prioritizeNutrition // prioriza la comida con mas nutricion
    )
    {
        // el generator devuelve los recurso de forma lazy que se encuentran en la escena, con un yield return 
        IEnumerable<GameObject> availableResources =
            GenerateResources(targetTag);

        if (prioritizeNutrition)
        {
            return availableResources

                // mayor nutricion primero
                .OrderByDescending(resource =>
                {
                    Food food = resource.GetComponent<Food>();

                    if (food == null)
                        return 0f;

                    return food.nutrition;
                })
                // primer resultado
                .FirstOrDefault();
        }

        return availableResources

            // recurso mas cercano primero
            .OrderBy(resource =>
                Vector3.SqrMagnitude(
                    transform.position -
                    resource.transform.position
                )
            )
            .FirstOrDefault();
    }
}