using UnityEngine;
using System.Collections.Generic;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float velocidad = 10f; // Velocidad del proyectil
    public float tiempoVida = 3f; // Tiempo antes de que el proyectil se devuelva al pool
    public bool puedeAtravesarObjetos = true; // Si el proyectil puede atravesar objetos

    // Para Object Pooling
    private Queue<GameObject> poolOrigen;
    private float tiempoCreacion;
    private bool devueltoAlPool = false;

    private void Awake()
    {
        // La configuración de capas ahora se hace desde el editor o al crear el proyectil
        // No intentamos cambiar la capa aquí
    }

    public void SetPool(Queue<GameObject> pool)
    {
        poolOrigen = pool;
    }

    public void Reiniciar()
    {
        // Marcar como activo (no devuelto al pool)
        devueltoAlPool = false;
        // Registrar el tiempo de creación para control manual de vida
        tiempoCreacion = Time.time;
    }

    private void Update()
    {
        // Verificar si ya ha pasado el tiempo de vida
        if (!devueltoAlPool && Time.time > tiempoCreacion + tiempoVida)
        {
            DevolverAlPool();
            return;
        }

        // Mover el proyectil hacia adelante
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo nos interesa colisionar con enemigos
        if (other.CompareTag("Enemigo"))
        {
            // Devolver el proyectil al pool al colisionar con un enemigo (a menos que pueda atravesar)
            if (!puedeAtravesarObjetos && !devueltoAlPool)
            {
                DevolverAlPool();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solo nos interesa colisionar con enemigos
        if (collision.gameObject.CompareTag("Enemigo"))
        {
            if (!puedeAtravesarObjetos && !devueltoAlPool)
            {
                DevolverAlPool();
            }
        }
    }

    private void DevolverAlPool()
    {
        // Prevenir múltiples devoluciones al pool
        if (devueltoAlPool)
            return;

        devueltoAlPool = true;

        // Restaurar a estado inicial
        gameObject.SetActive(false);

        // Devolver al pool
        if (poolOrigen != null)
        {
            poolOrigen.Enqueue(gameObject);
        }
    }
}