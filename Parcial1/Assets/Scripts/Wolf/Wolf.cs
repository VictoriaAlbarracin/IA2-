using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wolf : Animal
{
    // Inputs que disparan las transiciones entre los 5 estados
    public enum WolfTrigger { StartPatrol, SpotPrey, SpotMeat, NeedWater, NeedRest }

    private WolfStateMachine<WolfTrigger> _fsm;

    [Header("Wolf Settings")]
    public float detectionRadius = 15f;
    public float resourceSearchRadius = 50f;
    public float attackRange = 1.5f;
    public float huntSpeedMultiplier = 1.35f;

    private Transform _currentTarget;
    private Transform _currentWaypoint;
    private float _baseSpeed;

    // Eventos cuya suscripción cambia dinámicamente según el estado actual
    public event Action<Sheep> OnPreySpotted;
    public event Action<Meat> OnMeatSpotted;

    protected override void InitBrain()
    {
        _baseSpeed = speed;
        _actions = new List<IUtilityAction>();

        // 1. CREACIÓN DE LOS 5 ESTADOS
        var patrolState = new WolfState<WolfTrigger>("Patrol");
        var huntState = new WolfState<WolfTrigger>("Hunt");
        var eatState = new WolfState<WolfTrigger>("Eat");
        var drinkState = new WolfState<WolfTrigger>("Drink");
        var restState = new WolfState<WolfTrigger>("Rest");

        // 2. CONFIGURACIÓN DE TRANSICIONES
        patrolState.AddTransition(WolfTrigger.SpotPrey, huntState);
        patrolState.AddTransition(WolfTrigger.SpotMeat, eatState);
        patrolState.AddTransition(WolfTrigger.NeedWater, drinkState);
        patrolState.AddTransition(WolfTrigger.NeedRest, restState);

        huntState.AddTransition(WolfTrigger.StartPatrol, patrolState);
        huntState.AddTransition(WolfTrigger.NeedWater, drinkState);
        huntState.AddTransition(WolfTrigger.NeedRest, restState);

        eatState.AddTransition(WolfTrigger.StartPatrol, patrolState);
        eatState.AddTransition(WolfTrigger.SpotPrey, huntState); // Si ve una oveja mientras va por carne, puede cazarla
        eatState.AddTransition(WolfTrigger.NeedWater, drinkState);
        eatState.AddTransition(WolfTrigger.NeedRest, restState);

        drinkState.AddTransition(WolfTrigger.StartPatrol, patrolState);
        drinkState.AddTransition(WolfTrigger.NeedRest, restState);

        restState.AddTransition(WolfTrigger.StartPatrol, patrolState);

        // 3. DEFINICIÓN DE COMPORTAMIENTOS POR ESTADO (OnEnter / OnUpdate / OnExit)

        // --- ESTADO 1: PATRULLAR ---
        patrolState.OnEnter += trigger =>
        {
            Debug.Log("[Wolf FSM] Entrando a estado: PATROL");
            // Suscripción dinámica: solo escucha detecciones de comida/presa al patrullar
            OnPreySpotted += ReactToSpottedPrey;
            OnMeatSpotted += ReactToSpottedMeat;
        };

        patrolState.OnUpdate += () =>
        {
            if (energy <= 25f) { _fsm.TriggerInput(WolfTrigger.NeedRest); return; }
            if (thirst >= 65f) { _fsm.TriggerInput(WolfTrigger.NeedWater); return; }

            if (hunger >= 30f)
            {
                // Prioridad 1: Si ve una oveja en rango de visión, la caza
                Sheep prey = DetectClosestSheep();
                if (prey != null)
                {
                    OnPreySpotted?.Invoke(prey);
                    return;
                }

                // Prioridad 2: Si no hay ovejas cerca, busca carne en el piso
                Meat meat = DetectClosestMeat();
                if (meat != null)
                {
                    OnMeatSpotted?.Invoke(meat);
                    return;
                }
            }

            MoveAlongWaypoints();
        };

        patrolState.OnExit += trigger =>
        {
            Debug.Log("[Wolf FSM] Saliendo de estado: PATROL");
            OnPreySpotted -= ReactToSpottedPrey;
            OnMeatSpotted -= ReactToSpottedMeat;
        };

        // --- ESTADO 2: CAZAR OVEJA ---
        huntState.OnEnter += trigger =>
        {
            Debug.Log("[Wolf FSM] Entrando a estado: HUNT (Persiguiendo oveja)");
            speed = _baseSpeed * huntSpeedMultiplier;
        };

        huntState.OnUpdate += () =>
        {
            if (energy <= 15f) { _fsm.TriggerInput(WolfTrigger.NeedRest); return; }

            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                Debug.Log("[Wolf] La presa desapareció. Volviendo a patrullar.");
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            float distance = Vector3.Distance(transform.position, _currentTarget.position);

            if (distance > detectionRadius * 1.4f)
            {
                Debug.Log("[Wolf] La oveja escapó de mi rango de visión.");
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
            }
            else if (distance <= attackRange)
            {
                Sheep sheep = _currentTarget.GetComponent<Sheep>();
                if (sheep != null) sheep.Die();

                hunger = Mathf.Clamp(hunger - 65f, 0f, 100f);
                Debug.Log("[Wolf] ¡Cacé y comí una oveja! Hambre actual: " + hunger);
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
            Debug.Log("[Wolf FSM] Saliendo de estado: HUNT");
            speed = _baseSpeed;
        };

        // --- ESTADO 3: COMER CARNE DEL PISO ---
        eatState.OnEnter += trigger =>
        {
            Debug.Log("[Wolf FSM] Entrando a estado: EAT (Yendo por carne del suelo)");
        };

        eatState.OnUpdate += () =>
        {
            if (energy <= 15f) { _fsm.TriggerInput(WolfTrigger.NeedRest); return; }

            // Si aparece una oveja en rango mientras camina hacia la carne, cambia a cazarla
            Sheep nearbyPrey = DetectClosestSheep();
            if (nearbyPrey != null)
            {
                _currentTarget = nearbyPrey.transform;
                _fsm.TriggerInput(WolfTrigger.SpotPrey);
                return;
            }

            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            float distance = Vector3.Distance(transform.position, _currentTarget.position);

            if (distance <= attackRange)
            {
                Meat meat = _currentTarget.GetComponent<Meat>();
                if (meat != null)
                {
                    hunger = Mathf.Clamp(hunger - meat.nutrition, 0f, 100f);
                    Debug.Log("[Wolf] ¡Ya comí carne del suelo! Hambre actual: " + hunger);
                }

                _currentTarget.gameObject.SetActive(false);
                Destroy(_currentTarget.gameObject);
                _currentTarget = null;
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
            }
            else
            {
                Movement(_currentTarget.position);
            }
        };

        eatState.OnExit += trigger =>
        {
            Debug.Log("[Wolf FSM] Saliendo de estado: EAT");
            _currentTarget = null;
        };

        // --- ESTADO 4: TOMAR AGUA ---
        drinkState.OnEnter += trigger =>
        {
            Debug.Log("[Wolf FSM] Entrando a estado: DRINK");
            _currentTarget = FindClosestWater();
        };

        drinkState.OnUpdate += () =>
        {
            if (_currentTarget == null)
            {
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            if (Vector3.Distance(transform.position, _currentTarget.position) <= 2.5f)
            {
                thirst = 0f;
                Debug.Log("[Wolf] ¡Ya tomé agua en " + _currentTarget.name + "!");
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
            Debug.Log("[Wolf FSM] Saliendo de estado: DRINK");
            _currentTarget = null;
        };

        // --- ESTADO 5: DESCANSAR ---
        restState.OnEnter += trigger =>
        {
            Debug.Log("[Wolf FSM] Entrando a estado: REST");
            _currentTarget = FindClosestRestZone();
        };

        restState.OnUpdate += () =>
        {
            if (_currentTarget == null)
            {
                _fsm.TriggerInput(WolfTrigger.StartPatrol);
                return;
            }

            if (Vector3.Distance(transform.position, _currentTarget.position) > 2.5f)
            {
                Movement(_currentTarget.position);
            }
            else
            {
                energy += Time.deltaTime * 25f;
                if (energy >= 100f)
                {
                    energy = 100f;
                    Debug.Log("[Wolf] ¡Ya descansé por completo en " + _currentTarget.name + "!");
                    _fsm.TriggerInput(WolfTrigger.StartPatrol);
                }
            }
        };

        restState.OnExit += trigger =>
        {
            Debug.Log("[Wolf FSM] Saliendo de estado: REST");
            _currentTarget = null;
        };

        // 4. INICIALIZACIÓN DE LA MÁQUINA DE ESTADOS
        _fsm = new WolfStateMachine<WolfTrigger>(patrolState);
    }

    protected override void Update()
    {
        base.Update();
        _fsm?.Tick();
    }

    protected override void EvaluateAction() { }

    // =========================================================
    // CALLBACKS DE EVENTOS DINÁMICOS
    // =========================================================
    private void ReactToSpottedPrey(Sheep prey)
    {
        Debug.Log("[Wolf Evento] ¡Oveja avistada: " + prey.name + "! Iniciando caza.");
        _currentTarget = prey.transform;
        _fsm.TriggerInput(WolfTrigger.SpotPrey);
    }

    private void ReactToSpottedMeat(Meat meat)
    {
        Debug.Log("[Wolf Evento] ¡Carne detectada en el piso: " + meat.name + "!");
        _currentTarget = meat.transform;
        _fsm.TriggerInput(WolfTrigger.SpotMeat);
    }

    // =========================================================
    // MÉTODOS DE NAVEGACIÓN Y DETECCIÓN (VÍA SPATIAL GRID)
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
        GameObject sheepObj = BuscarRecurso<Sheep>(false, detectionRadius);
        if (sheepObj != null) return sheepObj.GetComponent<Sheep>();

        return FindObjectsByType<Sheep>(FindObjectsSortMode.None)
            .Where(s => s != null && s.gameObject.activeInHierarchy && Vector3.Distance(transform.position, s.transform.position) <= detectionRadius)
            .OrderBy(s => Vector3.Distance(transform.position, s.transform.position))
            .FirstOrDefault();
    }

    private Meat DetectClosestMeat()
    {
        GameObject meatObj = BuscarRecurso<Meat>(false, detectionRadius);
        if (meatObj != null) return meatObj.GetComponent<Meat>();

        return FindObjectsByType<Meat>(FindObjectsSortMode.None)
            .Where(m => m != null && m.gameObject.activeInHierarchy && Vector3.Distance(transform.position, m.transform.position) <= detectionRadius)
            .OrderBy(m => Vector3.Distance(transform.position, m.transform.position))
            .FirstOrDefault();
    }

    private Transform FindClosestWater()
    {
        GameObject waterObj = BuscarRecurso<Water>(false, resourceSearchRadius);
        if (waterObj != null) return waterObj.transform;

        var fallback = FindObjectsByType<Water>(FindObjectsSortMode.None)
            .OrderBy(w => Vector3.Distance(transform.position, w.transform.position))
            .FirstOrDefault();
        return fallback != null ? fallback.transform : null;
    }

    private Transform FindClosestRestZone()
    {
        GameObject restObj = BuscarRecurso<RestZone>(false, resourceSearchRadius);
        if (restObj != null) return restObj.transform;

        var fallback = FindObjectsByType<RestZone>(FindObjectsSortMode.None)
            .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
            .FirstOrDefault();
        return fallback != null ? fallback.transform : null;
    }

    private void OnDrawGizmos() //gizmos para visualizar el rango de vision del wofl
    {
        //rango de detección de la oveja 8amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        //rango maximo antes de perder de vista a la oveja (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius * 1.4f);

        // rango del ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}