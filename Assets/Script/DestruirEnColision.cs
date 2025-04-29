using UnityEngine;

public class DestruirEnColision : MonoBehaviour
{
    [Tooltip("La capa con la que debe colisionar para destruirse")]
    public LayerMask capaObjetivo;

    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto con el que colisionó está en la capa CasillaAjedrez
        if (((1 << collision.gameObject.layer) & capaObjetivo) != 0)
        {
            // Mostrar mensaje en consola para debugging
            Debug.Log(gameObject.name + " colisionó con " + collision.gameObject.name + " en la capa CasillaAjedrez y será destruido.");

            // Destruir este objeto
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // También verificar para triggers en caso de que el collider sea un trigger
        if (((1 << other.gameObject.layer) & capaObjetivo) != 0)
        {
            // Mostrar mensaje en consola para debugging
            Debug.Log(gameObject.name + " entró en trigger con " + other.gameObject.name + " en la capa CasillaAjedrez y será destruido.");

            // Destruir este objeto
            Destroy(gameObject);
        }
    }
}