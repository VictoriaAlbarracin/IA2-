using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Patrón Singleton para acceder desde cualquier script
    public static GameManager instance;

    [Header("Configuración de Entorno")]
    // Capa que la criatura reconocerá como obstáculos para esquivarlos
    public LayerMask obstacleMask;

    [Header("Ecosistema (Para Flocking futuro)")]
    // Lista global de criaturas en la escena
    public List<Animal> followers = new List<Animal>();

    [Header("Parámetros Globales de Manada")]
    public float radiusSeparation = 2.5f;
    public float weightSeparation = 1.5f;

    public float radiusDetected = 5f;
    public float weightCohesion = 1.0f;
    public float weightAlignment = 1.0f;

    private void Awake()
    {
        // Inicializamos el Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}