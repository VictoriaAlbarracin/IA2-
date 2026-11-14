using UnityEngine;

public class EatAction : IUtilityAction
{
    public string Name => "Eat";

    public float GetScore(Animal animal) => animal.hunger;

    public void Execute(Animal animal)
    {
        // Buscamos directamente el componente Food
        GameObject targetFood = animal.BuscarRecurso<Food>(true,50f);

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
                Debug.Log("ya comi ñamñam");
            }
            Object.Destroy(targetFood);
        }
        else
        {
            animal.Movement(targetFood.transform.position);
        }
    }
}