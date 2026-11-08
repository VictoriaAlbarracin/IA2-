using UnityEngine;

public class DrinkAction : IUtilityAction
{
    public string Name => "Drink";

    public float GetScore(EcosystemManager animal) => animal.thirst * 1.2f;

    public void Execute(EcosystemManager animal)
    {
        GameObject targetWater = animal.BuscarRecurso("Water", false);

        if (targetWater == null)
        {
            animal.ExecuteFallbackPatrol();
            return;
        }

        if (Vector3.Distance(animal.transform.position, targetWater.transform.position) < 1.5f)
        {
            animal.thirst = 0f;
            Debug.Log("ya tome aguita");
        }
        else
        {
            animal.Movement(targetWater.transform.position);
        }
    }
}
