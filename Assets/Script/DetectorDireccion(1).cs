using UnityEngine;
using System.Collections.Generic;
public class DetectorDireccion : MonoBehaviour
{
    public string direccion;
    public Jugador jugador;
    private List<GameObject> enemigosEnZona = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo") || other.CompareTag("Torreta") || other.CompareTag("TorreDelRey") || other.CompareTag("ObjetosVarios"))
        {
            if (!enemigosEnZona.Contains(other.gameObject)) // Evitar duplicados
            {
                enemigosEnZona.Add(other.gameObject);
                jugador.EnemigoEntro(direccion);
                Debug.Log($"Enemigo detectado en {direccion}: {other.gameObject.name}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemigo") || other.CompareTag("Torreta") || other.CompareTag("TorreDelRey") || other.CompareTag("ObjetosVarios"))
        {
            if (enemigosEnZona.Contains(other.gameObject))
            {
                enemigosEnZona.Remove(other.gameObject);
                // Solo notificar salida si no quedan m�s enemigos en la zona
                if (enemigosEnZona.Count == 0)
                {
                    jugador.EnemigoSalio(direccion);
                    Debug.Log($"No hay m�s enemigos en {direccion}");
                }
            }
        }
    }

    void Update()
    {
        bool cambioDetectado = false;
        // Verifica si alg�n enemigo en la zona fue destruido
        for (int i = enemigosEnZona.Count - 1; i >= 0; i--)
        {
            if (enemigosEnZona[i] == null)
            {
                enemigosEnZona.RemoveAt(i);
                cambioDetectado = true;
            }
        }

        // Solo notificar si hubo cambios y no quedan enemigos
        if (cambioDetectado && enemigosEnZona.Count == 0)
        {
            jugador.EnemigoSalio(direccion);
            Debug.Log($"Enemigo destruido, no hay m�s en {direccion}");
        }
    }

    // M�todo para forzar verificaci�n de enemigos
    public void ForzarVerificacion()
    {
        // Eliminar referencias nulas
        for (int i = enemigosEnZona.Count - 1; i >= 0; i--)
        {
            if (enemigosEnZona[i] == null)
            {
                enemigosEnZona.RemoveAt(i);
            }
        }

        // Actualizar estado basado en la lista actual
        if (enemigosEnZona.Count == 0)
        {
            jugador.EnemigoSalio(direccion);
        }
        else
        {
            jugador.EnemigoEntro(direccion);
        }
    }
}