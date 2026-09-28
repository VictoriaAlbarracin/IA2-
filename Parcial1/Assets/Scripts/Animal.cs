using UnityEngine;

public abstract class Animal : MonoBehaviour
{
    // Esta es la inercia actual de la criatura
    public Vector3 _velocity;

    // Configuraciones de movimiento
    public float maxVelocity = 3f;
    [Range(0f, 1f)] public float maxForce = 0.1f;
    [SerializeField] float _radiusArrive = 5f;

    public Vector3 Velocity
    {
        get { return _velocity; }
    }

    protected virtual void Start()
    {
        // El Start queda vacío pero listo por si necesitas inicializar algo en el futuro
    }

    protected virtual void Update()
    {
        // Anulamos la Y para que no vuele, y aplicamos la velocidad a la posición real
        _velocity.y = 0;
        transform.position += _velocity * Time.deltaTime;

        // Rota el modelo 3D para que mire hacia donde está caminando
        if (_velocity != Vector3.zero)
            transform.forward = _velocity;
    }

    public virtual void AddForce(Vector3 force)
    {
        // Suma la fuerza de dirección a la inercia, respetando el límite de velocidad
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxVelocity);
    }

    // Comportamiento para ir directo a un punto
    public Vector3 Seek(Vector3 pos)
    {
        var desired = pos - transform.position;
        desired.Normalize();
        desired *= maxVelocity;

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }

    // Comportamiento para ir a un punto y frenar suavemente al llegar
    public Vector3 Arrive(GameObject target)
    {
        var dist = Vector3.Distance(transform.position, target.transform.position);

        // Si está lejos, usa un Seek normal
        if (dist > _radiusArrive)
            return Seek(target.transform.position);

        // Si está dentro del radio, desacelera
        var desired = target.transform.position - transform.position;
        desired.Normalize();
        desired *= maxVelocity * (dist / _radiusArrive);

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }

    // Lo usan los estados de "Descansar" y "Beber" para quedarse quietos
    public void StopMovement()
    {
        _velocity = Vector3.zero;
    }
}