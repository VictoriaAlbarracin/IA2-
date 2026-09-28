using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Type of food")]
    public string foodName;

    [Tooltip("Food nutrition")]
    public float nutrition = 25f; //cuanta hambre recupera
}
