using UnityEngine;

public class DetectorDireccion : MonoBehaviour
{
    public string direccion; // Asigna "Frontal", "Trasero", etc. en el Editor
    public Jugador jugador;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            jugador.EnemigoEntro(direccion);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            jugador.EnemigoSalio(direccion);
        }
    }
}