using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProyectilParabolico : MonoBehaviour
{
    [Header("Configuraci�n Trayectoria")]
    private Vector3 posicionInicial;
    private Vector3 posicionObjetivo;
    private float alturaMaxima;
    private float gravedad;
    private float tiempoVuelo;
    private float tiempoTotal;
    private float distanciaHorizontal;

    [Header("Configuraci�n Collider")]
    [SerializeField] private float tiempoDesactivarCollider = 0.5f; // Tiempo antes de activar el collider
    private Collider proyectilCollider; // Referencia al collider del proyectil
    private bool colliderActivado = false; // Control interno

    [Header("Sistema de Part�culas")]
    public GameObject prefabParticulas; // Prefab de las part�culas

    // Para Object Pooling
    private Queue<GameObject> poolOrigen;
    private bool devueltoAlPool = false;

    // Pool de part�culas (est�tico para compartir entre todos los proyectiles)
    private static Queue<GameObject> poolParticulas;
    private static Transform contenedorParticulas;
    private static List<GameObject> particulasActivas = new List<GameObject>();
    private static int maximoParticulasSimultaneas = 50; // L�mite de part�culas simult�neas

    // Para manejar las corrutinas en un objeto que no se desactive
    private static GestorCorrutinas gestorCorrutinas;

    // Inicializar el pool de part�culas si no existe
    private void Awake()
    {
        if (poolParticulas == null)
        {
            poolParticulas = new Queue<GameObject>();
            particulasActivas = new List<GameObject>();

            // Crear un contenedor para las part�culas en la escena
            GameObject container = new GameObject("Pool_Particulas_Parabolicas");
            contenedorParticulas = container.transform;

            // Crear un gestor de corrutinas que no se destruya
            if (gestorCorrutinas == null)
            {
                GameObject gestorObj = new GameObject("GestorCorrutinasParabolicas");
                gestorCorrutinas = gestorObj.AddComponent<GestorCorrutinas>();
                DontDestroyOnLoad(gestorObj);
            }
        }

        // Obtener el Collider
        proyectilCollider = GetComponent<Collider>();
    }

    public void SetPool(Queue<GameObject> pool)
    {
        poolOrigen = pool;
    }

    public void Configurar(Vector3 inicio, Vector3 objetivo, float altura, float gravedad)
    {
        // Configuraci�n de la trayectoria
        posicionInicial = inicio;
        posicionObjetivo = objetivo;
        alturaMaxima = altura;
        this.gravedad = gravedad;
        distanciaHorizontal = Vector3.Distance(new Vector3(inicio.x, 0, inicio.z), new Vector3(objetivo.x, 0, objetivo.z));
        tiempoTotal = Mathf.Sqrt((2 * alturaMaxima) / Mathf.Abs(gravedad)) * 2;

        // Reiniciar variables de control
        tiempoVuelo = 0f;
        devueltoAlPool = false;
        colliderActivado = false;

        // Desactivar el collider al inicio
        if (proyectilCollider != null)
        {
            proyectilCollider.enabled = false;
        }

        // Posicionar en el punto inicial
        transform.position = inicio;
    }

    private void Update()
    {
        if (devueltoAlPool)
            return;

        tiempoVuelo += Time.deltaTime;

        // Activar el collider despu�s del tiempo definido
        if (!colliderActivado && tiempoVuelo >= tiempoDesactivarCollider)
        {
            if (proyectilCollider != null)
            {
                proyectilCollider.enabled = true;
                colliderActivado = true;
            }
        }

        // Movimiento parab�lico
        float progreso = tiempoVuelo / tiempoTotal;
        Vector3 posicionHorizontal = Vector3.Lerp(posicionInicial, posicionObjetivo, progreso);
        float altura = alturaMaxima * Mathf.Sin(Mathf.PI * progreso);
        transform.position = new Vector3(posicionHorizontal.x, posicionInicial.y + altura, posicionHorizontal.z);

        // Al llegar al objetivo, crear efecto de part�culas y devolver al pool
        if (tiempoVuelo >= tiempoTotal && !devueltoAlPool)
        {
            // Crear efecto de impacto con part�culas
            CrearEfectoPart�culas();

            // Devolver al pool en lugar de destruir
            DevolverAlPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si ha impactado con algo v�lido (por ejemplo un enemigo)
        if (other.CompareTag("Enemigo") && !devueltoAlPool)
        {
            // Crear efecto de impacto con part�culas
            CrearEfectoPart�culas();

            // Devolver al pool
            DevolverAlPool();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Mismo comportamiento que en OnTriggerEnter
        if (collision.gameObject.CompareTag("Enemigo") && !devueltoAlPool)
        {
            CrearEfectoPart�culas();
            DevolverAlPool();
        }
    }

    private void CrearEfectoPart�culas()
    {
        // Crear efecto de part�culas usando el pool
        GameObject particula = ObtenerParticulaDelPool();
        if (particula != null)
        {
            particula.transform.position = transform.position;
            particula.transform.rotation = transform.rotation;
            particula.SetActive(true);

            // Configurar el efecto para que se devuelva al pool despu�s de terminar
            ParticleSystem ps = particula.GetComponent<ParticleSystem>();
            float tiempoEspera = (ps != null) ?
                ps.main.duration + ps.main.startLifetime.constantMax : 2f;

            // Usar el gestor para iniciar la corrutina
            gestorCorrutinas.IniciarCorrutina(DevolverParticulaAlPool(particula, tiempoEspera));
        }
    }

    private void DevolverAlPool()
    {
        // Prevenir m�ltiples devoluciones al pool
        if (devueltoAlPool)
            return;

        devueltoAlPool = true;

        // Desactivar el collider
        if (proyectilCollider != null)
        {
            proyectilCollider.enabled = false;
        }

        // Restaurar a estado inicial
        gameObject.SetActive(false);

        // Devolver al pool
        if (poolOrigen != null)
        {
            poolOrigen.Enqueue(gameObject);
        }
    }

    // M�todo para obtener una part�cula del pool o crear una nueva si es necesario
    private GameObject ObtenerParticulaDelPool()
    {
        GameObject particula = null;

        // Intentar obtener una part�cula existente del pool
        while (poolParticulas.Count > 0 && particula == null)
        {
            GameObject obj = poolParticulas.Dequeue();
            if (obj != null)
            {
                particula = obj;
                break;
            }
        }

        // Si no hay part�culas disponibles, verificar l�mite
        if (particula == null)
        {
            // Si alcanzamos el l�mite, reutilizar la part�cula m�s antigua
            if (particulasActivas.Count >= maximoParticulasSimultaneas && particulasActivas.Count > 0)
            {
                particula = particulasActivas[0];
                particulasActivas.RemoveAt(0);

                // Detener cualquier sistema de part�culas en ejecuci�n
                ParticleSystem ps = particula.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            else // Si no hemos alcanzado el l�mite, crear nueva
            {
                particula = Instantiate(prefabParticulas);
                particula.transform.parent = contenedorParticulas;
            }
        }

        // A�adir a la lista de part�culas activas
        if (particula != null)
        {
            particulasActivas.Add(particula);
        }

        return particula;
    }

    // Corrutina para devolver la part�cula al pool despu�s de que termine
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
