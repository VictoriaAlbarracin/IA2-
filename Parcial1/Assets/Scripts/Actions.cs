using System;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public string Name;
    public Func<float> GetScore; // funcion que devuelve un float (la urgencia)
    public Action Execute;       // metodo realiza la acción
}
