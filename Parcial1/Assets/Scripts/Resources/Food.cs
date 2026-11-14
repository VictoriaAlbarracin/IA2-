using UnityEngine;

[RequireComponent(typeof(GridEntity))]
public class Food : MonoBehaviour
{
    [Header("Type of food")]
    public string foodName;
    public float nutrition = 25f;
}