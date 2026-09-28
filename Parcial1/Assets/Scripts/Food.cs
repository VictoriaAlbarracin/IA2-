using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Datos de la comida")]
    public string foodName;

    [Tooltip("Cantidad de hambre que recupera")]
    public float nutrition = 25f;
}
