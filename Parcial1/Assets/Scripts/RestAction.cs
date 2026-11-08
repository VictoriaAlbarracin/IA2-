using UnityEngine;

public class RestAction : IUtilityAction
{
    public string Name => "Rest";

    public float GetScore(Animal animal)
    {
        if (animal.energy <= 30f) return 1000f;

        return 100f - animal.energy;
    }

    public void Execute(Animal animal)
    {
        GameObject restZone = animal.BuscarRecurso("Rest", false);

        if (restZone == null)
        {
            animal.ExecuteFallbackPatrol();
            return;
        }

        if (Vector3.Distance(animal.transform.position, restZone.transform.position) > 2f)
        {
            animal.Movement(restZone.transform.position);
        }
        else
        {
            animal.energy = 100f;
            Debug.Log("ya descanse");
        }
    }
}
