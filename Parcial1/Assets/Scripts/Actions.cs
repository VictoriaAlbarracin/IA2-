using System;
using UnityEngine;

public class Actions 
{
    public string Name; //nombre de la accion
    public Func<float> GetScore; // funcion que devuelve en un float el score de la urgencia de las necesidades
    public Action Execute;       // metodo que realiza la acción
}
