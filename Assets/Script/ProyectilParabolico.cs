using UnityEngine;

public class ProyectilParabolico : MonoBehaviour
{
    private Vector3 posicionInicial;
    private Vector3 posicionObjetivo;
    private float alturaMaxima;
    private float gravedad;
    private float tiempoVuelo;
    private float distanciaHorizontal;
    private float tiempoTotal;

    // Nueva variable: tiempo de desactivación del collider
    [SerializeField] private float tiempoDesactivarCollider = 0.5f; // Tiempo en segundos
    private Collider proyectilCollider; // Referencia al collider del proyectil
    private bool colliderActivado = false; // Control interno

    public void Configurar(Vector3 inicio, Vector3 objetivo, float altura, float gravedad)
    {
        posicionInicial = inicio;
        posicionObjetivo = objetivo;
        alturaMaxima = altura;
        this.gravedad = gravedad;

        distanciaHorizontal = Vector3.Distance(new Vector3(inicio.x, 0, inicio.z), new Vector3(objetivo.x, 0, objetivo.z));
        tiempoTotal = Mathf.Sqrt((2 * alturaMaxima) / Mathf.Abs(gravedad)) * 2;

        // Obtener el Collider y desactivarlo al inicio
        proyectilCollider = GetComponent<Collider>();
        if (proyectilCollider != null)
        {
            proyectilCollider.enabled = false;
        }
    }

    private void Update()
    {
        tiempoVuelo += Time.deltaTime;

        // Activar el collider después del tiempo definido
        if (!colliderActivado && tiempoVuelo >= tiempoDesactivarCollider)
        {
            if (proyectilCollider != null)
            {
                proyectilCollider.enabled = true;
                colliderActivado = true;
            }
        }

        // Movimiento parabólico
        float progreso = tiempoVuelo / tiempoTotal;
        Vector3 posicionHorizontal = Vector3.Lerp(posicionInicial, posicionObjetivo, progreso);
        float altura = alturaMaxima * Mathf.Sin(Mathf.PI * progreso);
        transform.position = new Vector3(posicionHorizontal.x, posicionInicial.y + altura, posicionHorizontal.z);

        // Destruir al llegar al objetivo
        if (tiempoVuelo >= tiempoTotal)
        {
            Destroy(gameObject);
        }
    }
}