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

        // Ignorar colisiones con todos los objetos excepto los con tag "Enemigo"
        Collider colliderProyectil = GetComponent<Collider>();
        if (colliderProyectil != null)
        {
            // Ignorar colisiones con todos los colliders excepto los enemigos
            Collider[] todosColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (Collider collider in todosColliders)
            {
                if (collider.gameObject.tag != "Enemigo" && collider != colliderProyectil)
                {
                    Physics.IgnoreCollision(colliderProyectil, collider);
                }
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
        // Solo nos interesa colisionar con enemigos
        if (other.gameObject.tag == "Enemigo")
        {

            // Destruir el proyectil al colisionar con un enemigo (a menos que pueda atravesar)
            if (!puedeAtravesarObjetos)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Por si decides usar colisiones físicas en lugar de triggers
        if (collision.gameObject.tag != "Enemigo")
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
        else
        {
            if (!puedeAtravesarObjetos)
            {
                Destroy(gameObject);
            }
        }
    }
}