using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Proyectil : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float velocidad = 10f; // Velocidad del proyectil
    public float tiempoVida = 3f; // Tiempo antes de que el proyectil se devuelva al pool
    public bool puedeAtravesarObjetos = true; // Si el proyectil puede atravesar objetos

    [Header("Sistema de Partículas")]
    public GameObject prefabParticulas; // Prefab de las partículas

    // Para Object Pooling
    private Queue<GameObject> poolOrigen;
    private float tiempoCreacion;
    private bool devueltoAlPool = false;

<<<<<<< HEAD
    // Pool de partículas (estático para compartir entre todos los proyectiles)
    private static Queue<GameObject> poolParticulas;
    private static Transform contenedorParticulas;
    private static List<GameObject> particulasActivas = new List<GameObject>();
    private static int maximoParticulasSimultaneas = 50; // Límite de partículas simultáneas

    // Para manejar las corrutinas en un objeto que no se desactive
    private static GestorCorrutinas gestorCorrutinas;

    // Inicializar el pool de partículas si no existe
    private void Awake()
    {
        if (poolParticulas == null)
        {
            poolParticulas = new Queue<GameObject>();
            particulasActivas = new List<GameObject>();

            // Crear un contenedor para las partículas en la escena
            GameObject container = new GameObject("Pool_Particulas");
            contenedorParticulas = container.transform;

            // Crear un gestor de corrutinas que no se destruya
            if (gestorCorrutinas == null)
            {
                GameObject gestorObj = new GameObject("GestorCorrutinas");
                gestorCorrutinas = gestorObj.AddComponent<GestorCorrutinas>();
                DontDestroyOnLoad(gestorObj);
            }
        }
=======
    private void Awake()
    {
        // La configuración de capas ahora se hace desde el editor o al crear el proyectil
        // No intentamos cambiar la capa aquí
>>>>>>> parent of ee12fa2 ([ART]Assets and particles)
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
<<<<<<< HEAD
            // Crear efecto de partículas usando el pool
            GameObject particula = ObtenerParticulaDelPool();
            if (particula != null)
            {
                particula.transform.position = transform.position;
                particula.transform.rotation = transform.rotation;
                particula.SetActive(true);

                // Configurar el efecto para que se devuelva al pool después de terminar
                ParticleSystem ps = particula.GetComponent<ParticleSystem>();
                float tiempoEspera = (ps != null) ?
                    ps.main.duration + ps.main.startLifetime.constantMax : 2f;

                // Usar el gestor para iniciar la corrutina en lugar de este objeto
                gestorCorrutinas.IniciarCorrutina(DevolverParticulaAlPool(particula, tiempoEspera));
            }

=======
>>>>>>> parent of ee12fa2 ([ART]Assets and particles)
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

    // Método para obtener una partícula del pool o crear una nueva si es necesario
    private GameObject ObtenerParticulaDelPool()
    {
        GameObject particula = null;

        // Intentar obtener una partícula existente del pool
        while (poolParticulas.Count > 0 && particula == null)
        {
            GameObject obj = poolParticulas.Dequeue();
            if (obj != null)
            {
                particula = obj;
                break;
            }
        }

        // Si no hay partículas disponibles, verificar límite
        if (particula == null)
        {
            // Si alcanzamos el límite, reutilizar la partícula más antigua
            if (particulasActivas.Count >= maximoParticulasSimultaneas && particulasActivas.Count > 0)
            {
                particula = particulasActivas[0];
                particulasActivas.RemoveAt(0);

                // Detener cualquier sistema de partículas en ejecución
                ParticleSystem ps = particula.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            else // Si no hemos alcanzado el límite, crear nueva
            {
                particula = Instantiate(prefabParticulas);
                particula.transform.parent = contenedorParticulas;
            }
        }

        // Añadir a la lista de partículas activas
        if (particula != null)
        {
            particulasActivas.Add(particula);
        }

        return particula;
    }

    // Corrutina para devolver la partícula al pool después de que termine
    private IEnumerator DevolverParticulaAlPool(GameObject particula, float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);

        if (particula != null)
        {
            // Eliminar de la lista de activas
            particulasActivas.Remove(particula);

            // Desactivar y devolver al pool
            particula.SetActive(false);
            poolParticulas.Enqueue(particula);
        }
    }
}

// Clase para gestionar las corrutinas en un objeto permanente
public class GestorCorrutinas : MonoBehaviour
{
    public void IniciarCorrutina(IEnumerator corrutina)
    {
        StartCoroutine(corrutina);
    }
}