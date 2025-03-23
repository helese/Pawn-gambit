using UnityEngine;

public class ProyectilParabolico : MonoBehaviour
{
    private Vector3 posicionInicial; // Posición inicial del proyectil
    private Vector3 posicionObjetivo; // Posición del objetivo
    private float alturaMaxima; // Altura máxima de la trayectoria
    private float gravedad; // Gravedad para el movimiento parabólico
    private float tiempoVuelo; // Tiempo transcurrido desde el lanzamiento
    private float distanciaHorizontal; // Distancia horizontal entre el mortero y el objetivo
    private float tiempoTotal; // Tiempo total de vuelo

    public void Configurar(Vector3 inicio, Vector3 objetivo, float altura, float gravedad)
    {
        posicionInicial = inicio;
        posicionObjetivo = objetivo;
        alturaMaxima = altura;
        this.gravedad = gravedad;

        // Calcular la distancia horizontal y el tiempo total de vuelo
        distanciaHorizontal = Vector3.Distance(new Vector3(inicio.x, 0, inicio.z), new Vector3(objetivo.x, 0, objetivo.z));
        tiempoTotal = Mathf.Sqrt((2 * alturaMaxima) / Mathf.Abs(gravedad)) * 2;
    }

    private void Update()
    {
        // Calcular el tiempo de vuelo
        tiempoVuelo += Time.deltaTime;

        // Calcular la posición en la parábola
        float progreso = tiempoVuelo / tiempoTotal;
        Vector3 posicionHorizontal = Vector3.Lerp(posicionInicial, posicionObjetivo, progreso);
        float altura = alturaMaxima * Mathf.Sin(Mathf.PI * progreso);

        // Aplicar la posición al proyectil
        transform.position = new Vector3(posicionHorizontal.x, posicionInicial.y + altura, posicionHorizontal.z);

        // Destruir el proyectil cuando llegue al objetivo
        if (tiempoVuelo >= tiempoTotal)
        {
            Destroy(gameObject);
        }
    }
}