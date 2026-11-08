using UnityEngine;

public class EatAction : IUtilityAction
{
    public string Name => "Eat";

    public float GetScore(EcosystemManager animal) => animal.hunger;

    public void Execute(EcosystemManager animal)
    {
        GameObject targetFood = animal.BuscarRecurso("Food", true);

        if (targetFood == null)
        {
            animal.ExecuteFallbackPatrol();
            return;
        }

        if (Vector3.Distance(animal.transform.position, targetFood.transform.position) < 1.5f)
        {
            Food food = targetFood.GetComponent<Food>();
            if (food != null)
            {
                animal.hunger -= food.nutrition;
                animal.hunger = Mathf.Max(animal.hunger, 0f);
                Debug.Log("ya comi");
            }
            Object.Destroy(targetFood);
        }
        else
        {
            animal.Movement(targetFood.transform.position);
        }
    }
}