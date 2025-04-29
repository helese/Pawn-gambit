using UnityEngine;
using System.Collections.Generic;

public class Torreta : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public float cadenciaDisparo = 1f; // Tiempo entre cada disparo
    public GameObject proyectilPrefab; // Prefab del proyectil

    [Header("Puntos de Disparo")]
    public Transform[] puntosDeDisparo; // Lista de puntos de disparo (direcciones y posiciones)
    public Vector3[] desplazamientosCajas; // Desplazamientos de las cajas de colisión respecto a los puntos de disparo
    public Vector3[] tamanosCajas; // Tamaños de las cajas de colisión para cada punto de disparo
    public Vector3[] rotacionesCajas; // Rotaciones de las cajas de colisión para cada punto de disparo

    [Header("Material Requerido")]
    public Material materialRequerido; // Material que debe tener la torreta para disparar

    [Header("Objetos a Encender/Apagar")]
    public GameObject[] objetosMaterialIncorrecto; // Objetos que se encienden si la torreta no tiene el material correcto

    [Header("Optimización")]
    public int tamanoPool = 10;  // Tamaño del pool para cada punto de disparo
    public float intervaloDeteccion = 0.2f; // Cada cuántos segundos verificar enemigos

    [Header("Configuración de Capas")]
    public int capaProyectil = 8; // Capa para los proyectiles (por defecto 8, ajustar según tu proyecto)
    public int capaEnemigo = 9;   // Capa para los enemigos (por defecto 9, ajustar según tu proyecto)

    private float tiempoUltimoDisparo; // Tiempo del último disparo
    private float tiempoUltimaDeteccion; // Tiempo de la última detección
    private bool[] enemigosEnRango; // Indica si hay un enemigo dentro de cada área de detección
    private bool materialCorrecto; // Cache para el estado del material

    public int puntosARecuperar;
    private GameManager gameManager;

    // Object pooling
    private List<Queue<GameObject>> poolsProyectiles;
    private Transform poolContainer;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Inicializar el array de detección de enemigos
        enemigosEnRango = new bool[puntosDeDisparo.Length];

        // Verificar el material al inicio
        materialCorrecto = TieneMaterialCorrecto();
        ActualizarVisualesSegunMaterial();

        NotificarSpawn();

        // Inicializar el object pooling
        InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        // Crear un contenedor para todos los proyectiles del pool
        poolContainer = new GameObject("ProyectilesPool_" + gameObject.name).transform;
        poolContainer.SetParent(null); // No quedarse como hijo de la torreta

        // Inicializar una lista de colas (una cola por punto de disparo)
        poolsProyectiles = new List<Queue<GameObject>>();

        // Crear un pool para cada punto de disparo
        for (int i = 0; i < puntosDeDisparo.Length; i++)
        {
            Queue<GameObject> poolPuntoDisparo = new Queue<GameObject>();

            // Prellenar el pool
            for (int j = 0; j < tamanoPool; j++)
            {
                GameObject proyectil = Instantiate(proyectilPrefab, Vector3.one * 1000, Quaternion.identity);
                proyectil.transform.SetParent(poolContainer);

                // Asignar la capa directamente (en lugar de usar NameToLayer)
                proyectil.layer = capaProyectil;

                proyectil.SetActive(false);

                // Configurar el proyectil para que vuelva al pool
                Proyectil scriptProyectil = proyectil.GetComponent<Proyectil>();
                if (scriptProyectil != null)
                {
                    scriptProyectil.SetPool(poolPuntoDisparo);
                }

                poolPuntoDisparo.Enqueue(proyectil);
            }

            poolsProyectiles.Add(poolPuntoDisparo);
        }
    }

    void Update()
    {
        // Verificar el material solo cada cierto tiempo
        if (Time.frameCount % 30 == 0)  // Cada 30 frames
        {
            materialCorrecto = TieneMaterialCorrecto();
            ActualizarVisualesSegunMaterial();
        }

        // Solo procesar el resto si el material es correcto
        if (materialCorrecto)
        {
            // Verificar enemigos en intervalos en lugar de cada frame
            if (Time.time >= tiempoUltimaDeteccion + intervaloDeteccion)
            {
                VerificarEnemigosEnRango();
                tiempoUltimaDeteccion = Time.time;
            }

            // Disparar solo si hay un enemigo en rango y es momento de disparar
            if (Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
            {
                bool disparo = Disparar();
                if (disparo)
                {
                    tiempoUltimoDisparo = Time.time; // Actualizar el tiempo del último disparo
                }
            }
        }
    }

    // Resto del código permanece igual...

    private void VerificarEnemigosEnRango()
    {
        // Usar OverlapBoxNonAlloc en lugar de FindGameObjectsWithTag para mejor rendimiento
        Collider[] hitColliders = new Collider[10]; // Ajustar según la cantidad máxima esperada

        // Inicialmente asumir que no hay enemigos en rango
        for (int i = 0; i < enemigosEnRango.Length; i++)
        {
            enemigosEnRango[i] = false;

            // Obtener parámetros de la caja
            Vector3 centroCaja = puntosDeDisparo[i].position + desplazamientosCajas[i];
            Quaternion rotacionCaja = Quaternion.Euler(rotacionesCajas[i]);

            // Detectar colisiones usando Physics.OverlapBoxNonAlloc
            // Podemos usar una máscara de capa para detectar solo enemigos
            int layerMask = 1 << capaEnemigo; // Máscara para detectar solo la capa de enemigos

            int numColliders = Physics.OverlapBoxNonAlloc(
                centroCaja,
                tamanosCajas[i] / 2, // Half extent
                hitColliders,
                rotacionCaja,
                layerMask
            );

            // Cualquier objeto detectado será un enemigo debido a la máscara de capa
            if (numColliders > 0)
            {
                enemigosEnRango[i] = true;
            }
        }
    }

    // El resto del código permanece igual...

    void NotificarSpawn()
    {
        ControladorEscenario[] controladores = FindObjectsByType<ControladorEscenario>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (ControladorEscenario controlador in controladores)
        {
            controlador.AplicarModificadorANuevoObjeto(this);
        }
    }

    private bool TieneMaterialCorrecto()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.material.name.Replace(" (Instance)", "") == materialRequerido.name;
        }
        return false;
    }

    private void ActualizarVisualesSegunMaterial()
    {
        // Activar o desactivar los objetos según el material
        foreach (GameObject obj in objetosMaterialIncorrecto)
        {
            if (obj != null)
            {
                obj.SetActive(!materialCorrecto);
            }
        }
    }

    private bool Disparar()
    {
        bool disparo = false;

        // Recorrer todos los puntos de disparo
        for (int i = 0; i < puntosDeDisparo.Length; i++)
        {
            // Disparar solo si hay un enemigo en la caja de colisión correspondiente
            if (enemigosEnRango[i])
            {
                // Obtener un proyectil del pool
                if (poolsProyectiles[i].Count > 0)
                {
                    GameObject proyectil = poolsProyectiles[i].Dequeue();

                    // Primero configuramos el proyectil mientras está inactivo
                    proyectil.transform.position = puntosDeDisparo[i].position;
                    proyectil.transform.rotation = puntosDeDisparo[i].rotation;

                    // Reiniciar el proyectil
                    Proyectil proyectilScript = proyectil.GetComponent<Proyectil>();
                    if (proyectilScript != null)
                    {
                        proyectilScript.Reiniciar();
                    }

                    // Configurar la velocidad si tiene Rigidbody
                    Rigidbody rb = proyectil.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero; // Resetear velocidad
                    }

                    // Ahora activamos el proyectil
                    proyectil.SetActive(true);

                    // Aplicar la velocidad después de activar
                    if (rb != null)
                    {
                        rb.linearVelocity = puntosDeDisparo[i].forward * 10f;
                    }

                    disparo = true;
                }
            }
        }

        return disparo;
    }

    // Dibujar las áreas de detección en el Viewport
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Dibujar las cajas de colisión para cada punto de disparo
        for (int i = 0; i < puntosDeDisparo.Length; i++)
        {
            if (puntosDeDisparo[i] != null) // Verificar que el punto de disparo exista
            {
                Vector3 centroCaja = puntosDeDisparo[i].position + desplazamientosCajas[i];
                DibujarCajaGizmo(centroCaja, tamanosCajas[i], rotacionesCajas[i]);
            }
        }
    }

    private void DibujarCajaGizmo(Vector3 centro, Vector3 tamano, Vector3 rotacion)
    {
        // Guardar la matriz de transformación actual
        Matrix4x4 matrizOriginal = Gizmos.matrix;

        // Aplicar la rotación a la matriz de transformación de Gizmos
        Gizmos.matrix = Matrix4x4.TRS(centro, Quaternion.Euler(rotacion), Vector3.one);

        // Dibujar la caja
        Gizmos.DrawWireCube(Vector3.zero, tamano);

        // Restaurar la matriz de transformación original
        Gizmos.matrix = matrizOriginal;
    }

    private void OnDestroy()
    {
        // Destruir el contenedor del pool
        if (poolContainer != null)
        {
            Destroy(poolContainer.gameObject);
        }

        // Verificar si la torreta tiene el material correcto antes de sumar puntos
        if (materialCorrecto && gameManager != null)
        {
            gameManager.SumarUnidades(puntosARecuperar); // Sumar unidades al slider
        }
    }
}