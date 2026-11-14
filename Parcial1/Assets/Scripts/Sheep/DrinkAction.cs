using UnityEngine;

public class DrinkAction : IUtilityAction
{
    public string Name => "Drink";

    public float GetScore(Animal animal) => animal.thirst * 1.2f;

    public void Execute(Animal animal)
    {
        // Buscamos el componente Water
        GameObject targetWater = animal.BuscarRecurso<Water>(false,50f);

        if (targetWater == null)
        {
            animal.ExecuteFallbackPatrol();
            return;
        }

        if (Vector3.Distance(animal.transform.position, targetWater.transform.position) < 1.5f)
        {
            animal.thirst = 0f;
            Debug.Log("tome aguita");
        }
        else
        {
            animal.Movement(targetWater.transform.position);
        }
    }
}