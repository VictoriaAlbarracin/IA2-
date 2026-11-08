using UnityEngine;

public class PatrolAction : IUtilityAction
{
    public string Name => "Patrol";
    private Transform _currentWaypoint;

    public float GetScore(EcosystemManager animal) => 30f; // Puntaje base

    public void Execute(EcosystemManager animal)
    {
        if (animal.waypoints == null || animal.waypoints.Length == 0) return;

        if (_currentWaypoint == null || Vector3.Distance(animal.transform.position, _currentWaypoint.position) < 1.5f)
        {
            int index = Random.Range(0, animal.waypoints.Length);
            _currentWaypoint = animal.waypoints[index];
        }

        animal.Movement(_currentWaypoint.position);
    }
}