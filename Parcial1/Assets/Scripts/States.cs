using Unity.VisualScripting;
using UnityEngine;

// 1. COMPORTAMIENTO LIBRE
public class FreeState : IState
{
    EcosystemManager _animal;
    Vector3 _wanderTarget;

    public FreeState(EcosystemManager animal) { _animal = animal; }

    public void OnEnter() { GetNewRandomPoint(); }

    public void OnUpdate()
    {
        if (_animal.thirst >= _animal.limitThirst) { _animal._fsm.ChangeState("Drink"); return; }
        if (_animal.hunger >= _animal.limitHunger) { _animal._fsm.ChangeState("Eat"); return; }
        if (_animal.energy <= _animal.limitEnergy) { _animal._fsm.ChangeState("Rest"); return; }

        if (Vector3.Distance(_animal.transform.position, _wanderTarget) < 1.5f)
            GetNewRandomPoint();

        _animal.AddForce(_animal.Seek(_wanderTarget));
    }

    public void OnExit() { }

    void GetNewRandomPoint()
    {
        _animal.maxVelocity = 3f;
        _wanderTarget = _animal.transform.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }
}

// 2. ALIMENTARSE
public class EatState : IState
{
    EcosystemManager _animal;
    GameObject _targetFood;

    public EatState(EcosystemManager animal) { _animal = animal; }

    public void OnEnter()
    {
        _animal.maxVelocity = 6f;
        _targetFood = _animal.BuscarRecursoOptimo("Food");
    }

    public void OnUpdate()
    {
        if (_targetFood == null) { _animal._fsm.ChangeState("Free"); return; }

        _animal.AddForce(_animal.Arrive(_targetFood));

        if (Vector3.Distance(_animal.transform.position, _targetFood.transform.position) < 1.5f)
        {
            Object.Destroy(_targetFood);
            _animal.hunger = 0;
            _animal._fsm.ChangeState("Free");
        }
    }
    public void OnExit() { }
}

// 3. BEBER
public class DrinkState : IState
{
    EcosystemManager _animal;
    GameObject _targetWater;

    public DrinkState(EcosystemManager animal) { _animal = animal; }

    public void OnEnter()
    {
        _animal.maxVelocity = 6f;
        _targetWater = _animal.BuscarRecursoOptimo("Water");
    }

    public void OnUpdate()
    {
        if (_targetWater == null) { _animal._fsm.ChangeState("Free"); return; }

        _animal.AddForce(_animal.Arrive(_targetWater));

        if (Vector3.Distance(_animal.transform.position, _targetWater.transform.position) < 1.5f)
        {
            _animal.StopMovement();
            _animal.thirst -= Time.deltaTime * 50f;

            if (_animal.thirst <= 0)
            {
                _animal.thirst = 0;
                _animal._fsm.ChangeState("Free");
            }
        }
    }
    public void OnExit() { }
}

// 4. DESCANSAR
public class RestState : IState
{
    EcosystemManager _animal;
    GameObject _restZone;

    public RestState(EcosystemManager animal) { _animal = animal; }

    public void OnEnter()
    {
        _restZone = _animal.BuscarRecursoOptimo("Rest");
    }

    public void OnUpdate()
    {
        if (_restZone != null && Vector3.Distance(_animal.transform.position, _restZone.transform.position) > 2f)
        {
            _animal.AddForce(_animal.Seek(_restZone.transform.position));
        }
        else
        {
            _animal.StopMovement();
            _animal.energy += Time.deltaTime * 15f;
        }

        if (_animal.energy >= 100f)
        {
            _animal.energy = 100f;
            _animal._fsm.ChangeState("Free");
        }
    }
    public void OnExit() { }
}