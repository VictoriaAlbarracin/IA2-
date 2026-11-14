using UnityEngine;

[RequireComponent(typeof(GridEntity))]
public class Meat : MonoBehaviour
{
    [Header("Propiedades de la Carne")]
    public string meatName = "Carne";
    public float nutrition = 45f;
}