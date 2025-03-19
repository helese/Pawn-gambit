using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float velocidad = 10f; // Velocidad del proyectil
    public float tiempoVida = 3f; // Tiempo antes de que el proyectil se destruya
    public bool puedeAtravesarObjetos = true; // Si el proyectil puede atravesar objetos

    private void Start()
    {
        // Destruir el proyectil después de un tiempo de vida
        Destroy(gameObject, tiempoVida);

        // Ignorar colisiones con el jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player"); // Buscar al jugador por tag
        if (jugador != null)
        {
            Collider colliderJugador = jugador.GetComponent<Collider>();
            Collider colliderProyectil = GetComponent<Collider>();

            if (colliderJugador != null && colliderProyectil != null)
            {
                Physics.IgnoreCollision(colliderJugador, colliderProyectil);
            }
        }
    }

    private void Update()
    {
        // Mover el proyectil hacia adelante
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el proyectil puede atravesar objetos
        if (!puedeAtravesarObjetos)
        {
            // Destruir el proyectil al colisionar con un objeto
            Destroy(gameObject);
        }

        // Aquí puedes añadir lógica adicional, como dañar al objeto con el que colisiona
        // Ejemplo: other.GetComponent<Salud>()?.RecibirDaño(damage);
    }
}