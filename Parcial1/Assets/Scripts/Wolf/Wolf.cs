using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wolf : Animal
{
    // Inputs que disparan las transiciones entre estados
    public enum WolfTrigger { StartPatrol, SpotPrey, NeedWater, NeedRest }

    private WolfStateMachine<WolfTrigger> _fsm;

    [Header("Parámetros del Lobo")]
    public float detectionRadius = 15f;
    public float attackRange = 1.5f;
    public float huntSpeedMultiplier = 1.35f;

    private Transform _currentTarget;
    private Transform _currentWaypoint;
    private float _baseSpeed;

    // Evento cuya suscripción cambia dinámicamente según el estado actual
    public event Action<Sheep> OnPreySpotted;

    protected override void InitBrain()
    {
        _baseSpeed = speed;
        _actions = new List<IUtilityAction>(); // Evita referencias nulas en la clase padre

        // 1. CREACIÓN DE LOS 4 ESTADOS
        var patrolState = new WolfState<WolfTrigger>("Patrol");
        var huntState = new WolfState<WolfTrigger>("Hunt");
        var drinkState = new WolfState<WolfTrigger>("Drink");
        var restState = new WolfState<WolfTrigger>("Rest");

        // 2. CONFIGURACIÓN DE TRANSICIONES
        patrolState.AddTransition(WolfTrigger.SpotPrey, huntState);
        patrolState.AddTransition(WolfTrigger.NeedWater, drinkState);
        patrolState.AddTransition(WolfTrigger.NeedRest, restState);

        huntState.AddTransition(WolfTrigger.StartPatrol, patrolState);
        huntState.AddTransition(WolfTrigger.NeedWater, drinkState);
        huntState.AddTransition(WolfTrigger.NeedRest, restState);

        drinkState.AddTransition(WolfTrigger.StartPatrol, patrolState);
        drinkState.AddTransition(WolfTrigger.NeedRest, restState);

        restState.AddTransition(WolfTrigger.StartPatrol, patrolState);

        // 3. DEFINICIÓN DE COMPORTAMIENTOS POR ESTADO (Enter / Update / Exit)

        // --- ESTADO 1: PATRULLAR ---
        patrolState.OnEnter += trigger =>
        {
            // Nos suscribimos al evento de avistamiento ÚNICAMENTE al patrullar
            OnPreySpotted += ReactToSpottedPrey;
        };

        patrolState.OnUpdate += () =>
        {
            // Prioridades de supervivencia
            if (energy <= 25f) { _fsm.TriggerInput(WolfTrigger.NeedRest); return; }
            if (thirst >= 65f) { _fsm.TriggerInput(WolfTrigger.NeedWater); return; }

            // Detección de ovejas si tiene hambre
            if (hunger >= 30f)
            {
                Sheep prey = DetectClosestSheep();
                if (prey != null)
                {
                    OnPreySpotted?.Invoke(prey); // Dispara el evento suscrito
                    return;
                }
            }

            MoveAlongWaypoints();
        };

        patrolState.OnExit += trigger =>
        {
            // Desuscribimos el evento al salir de Patrol para que no reaccione en otros estados
            OnPreySpotted -= ReactToSpottedPrey;
        };

        // --- ESTADO 2: CAZAR (Perseguir oveja) ---
        huntState.OnEnter += trigger =>
        {
            speed = _baseSpeed * huntSpeedMultiplier;
        };

        huntState.OnUpdate += () =>
        {
            if (energy <= 15f) { _fsm.TriggerInput(WolfTrigger.NeedRest); return; }

            if (_currentTarget == null)
            {
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            float distance = Vector3.Distance(transform.position, _currentTarget.position);

            if (distance > detectionRadius * 1.4f)
            {
                // Perdió de vista a la oveja
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
            }
            else if (distance <= attackRange)
            {
                // Alcanzó a la oveja: la caza y recupera hambre
                Sheep sheep = _currentTarget.GetComponent<Sheep>();
                if (sheep != null) sheep.Die();

                hunger = Mathf.Clamp(hunger - 65f, 0f, 100f);
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
            }
            else
            {
                Movement(_currentTarget.position);
            }
        };

        huntState.OnExit += trigger =>
        {
            speed = _baseSpeed; // Restaura la velocidad normal al dejar de cazar
        };

        // --- ESTADO 3: TOMAR AGUA ---
        drinkState.OnEnter += trigger =>
        {
            _currentTarget = FindClosestWater();
        };

        drinkState.OnUpdate += () =>
        {
            if (_currentTarget == null)
            {
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            if (Vector3.Distance(transform.position, _currentTarget.position) <= 1.5f)
            {
                thirst = 0f;
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
            }
            else
            {
                Movement(_currentTarget.position);
            }
        };

        drinkState.OnExit += trigger =>
        {
            _currentTarget = null;
        };

        // --- ESTADO 4: DESCANSAR ---
        restState.OnEnter += trigger =>
        {
            _currentTarget = FindClosestRestZone();
        };

        restState.OnUpdate += () =>
        {
            if (_currentTarget != null && Vector3.Distance(transform.position, _currentTarget.position) > 2f)
            {
                Movement(_currentTarget.position);
            }
            else
            {
                // Recupera energía progresivamente en el lugar
                energy += Time.deltaTime * 20f;
                if (energy >= 100f)
                {
                    energy = 100f;
                    _fsm.TriggerInput(WolfTrigger.StartPatrol);
                }
            }
        };

        restState.OnExit += trigger =>
        {
            _currentTarget = null;
        };

        // 4. INICIALIZACIÓN DE LA MÁQUINA DE ESTADOS
        _fsm = new WolfStateMachine<WolfTrigger>(patrolState);
    }

    protected override void Update()
    {
        // Ejecuta desgaste biológico (hunger, thirst, energy) y actualización de grilla de Animal
        base.Update();

        // Actualiza el estado activo de la FSM
        _fsm?.Tick();
    }

    // Anulamos la lógica de Utility AI de Animal para que no interfiera con la FSM
    protected override void EvaluateAction() { }

    // =========================================================
    // CALLBACK DEL EVENTO DINÁMICO
    // =========================================================
    private void ReactToSpottedPrey(Sheep prey)
    {
        _currentTarget = prey.transform;
        _fsm.TriggerInput(WolfTrigger.SpotPrey);
    }

    // =========================================================
    // MÉTODOS DE NAVEGACIÓN Y DETECCIÓN
    // =========================================================
    private void MoveAlongWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (_currentWaypoint == null || Vector3.Distance(transform.position, _currentWaypoint.position) < 1.5f)
        {
            int randomIndex = UnityEngine.Random.Range(0, waypoints.Length);
            _currentWaypoint = waypoints[randomIndex];
        }

        Movement(_currentWaypoint.position);
    }

    private Sheep DetectClosestSheep()
    {
        // Usamos el método BuscarRecurso<T> de Animal que ya consulta el Query de SpatialGrid
        GameObject sheepObj = BuscarRecurso<Sheep>(false, detectionRadius);
        if (sheepObj != null)
        {
            return sheepObj.GetComponent<Sheep>();
        }

        // Respaldo por si aún no asignaste la SpatialGrid en el Inspector
        return FindObjectsByType<Sheep>(FindObjectsSortMode.None)
            .Where(s => s != null && Vector3.Distance(transform.position, s.transform.position) <= detectionRadius)
            .OrderBy(s => Vector3.Distance(transform.position, s.transform.position))
            .FirstOrDefault();
    }

    private Transform FindClosestWater()
    {
        GameObject waterObj = BuscarRecurso<Water>(false);
        if (waterObj != null) return waterObj.transform;

        var fallback = FindObjectsByType<Water>(FindObjectsSortMode.None)
            .OrderBy(w => Vector3.Distance(transform.position, w.transform.position))
            .FirstOrDefault();
        return fallback != null ? fallback.transform : null;
    }

    private Transform FindClosestRestZone()
    {
        GameObject restObj = BuscarRecurso<RestZone>(false);
        if (restObj != null) return restObj.transform;

        var fallback = FindObjectsByType<RestZone>(FindObjectsSortMode.None)
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
            .FirstOrDefault();
        return fallback != null ? fallback.transform : null;
    }
}