using UnityEngine;
using System.Collections.Generic;

public class DetectorDireccion : MonoBehaviour
{
    public string direccion;
    public Jugador jugador;
    private List<GameObject> enemigosEnZona = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo") || other.CompareTag("Torreta"))
        {
            enemigosEnZona.Add(other.gameObject);
            jugador.EnemigoEntro(direccion);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemigo") || other.CompareTag("Torreta"))
        {
            enemigosEnZona.Remove(other.gameObject);
            jugador.EnemigoSalio(direccion);
        }
    }

    void Update()
    {
        // Verifica si algún enemigo en la zona fue destruido
        for (int i = enemigosEnZona.Count - 1; i >= 0; i--)
        {
            if (enemigosEnZona[i] == null)
            {
                enemigosEnZona.RemoveAt(i);
                jugador.EnemigoSalio(direccion);
            }
        }
    }
}