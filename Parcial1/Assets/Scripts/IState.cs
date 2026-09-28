using UnityEngine;

// La interfaz base
public interface IState
{
    float GetScore(EcosystemManager animal);
    void Execute(EcosystemManager animal);
}

// 1. ESTADO: COMER
public class EatState : IState
{
    public float GetScore(EcosystemManager animal)
    {
        return animal.hunger;
    }

    public void Execute(EcosystemManager animal)
    {
        var targetFood = animal.BuscarRecursoOptimo("Food");
        if (targetFood == null) return;

        animal.AddForce(animal.Arrive(targetFood));

        if (Vector3.Distance(animal.transform.position, targetFood.transform.position) < 1.5f)
        {
            Object.Destroy(targetFood);
            animal.hunger = 0f;
        }
    }
}

// 2. ESTADO: BEBER
public class DrinkState : IState
{
    public float GetScore(EcosystemManager animal)
    {
        return animal.thirst;
    }

    public void Execute(EcosystemManager animal)
    {
        var targetWater = animal.BuscarRecursoOptimo("Water");
        if (targetWater == null) return;

        animal.AddForce(animal.Arrive(targetWater));

        if (Vector3.Distance(animal.transform.position, targetWater.transform.position) < 1.5f)
        {
            animal.StopMovement();
            animal.thirst -= Time.deltaTime * 50f;
            if (animal.thirst < 0) animal.thirst = 0;
        }
    }
}

// 3. ESTADO: DESCANSAR
public class RestState : IState
{
    public float GetScore(EcosystemManager animal)
    {
        return 100f - animal.energy;
    }

    public void Execute(EcosystemManager animal)
    {
        var restZone = animal.BuscarRecursoOptimo("Descanso");
        if (restZone != null && Vector3.Distance(animal.transform.position, restZone.transform.position) > 2f)
        {
            animal.AddForce(animal.Seek(restZone.transform.position));
        }
        else
        {
            animal.StopMovement();
            animal.energy += Time.deltaTime * 15f;
            if (animal.energy > 100f) animal.energy = 100f;
        }
    }
}

// 4. ESTADO: PATRULLA POR WAYPOINTS
public class FreeState : IState
{
    private Transform _currentWaypoint;

    public float GetScore(EcosystemManager animal)
    {
        return 40f; // Prioridad base cuando no hay urgencias
    }

    public void Execute(EcosystemManager animal)
    {
        if (animal.waypoints == null || animal.waypoints.Length == 0) return;

        if (_currentWaypoint == null || Vector3.Distance(animal.transform.position, _currentWaypoint.position) < 1.5f)
        {
            int index = Random.Range(0, animal.waypoints.Length);
            _currentWaypoint = animal.waypoints[index];
        }

        animal.AddForce(animal.Seek(_currentWaypoint.position));
    }
}