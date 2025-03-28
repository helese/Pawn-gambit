using UnityEngine;

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

    private float tiempoUltimoDisparo; // Tiempo del último disparo
    private bool[] enemigosEnRango; // Indica si hay un enemigo dentro de cada área de detección

    public int puntosARecuperar;
    private GameManager gameManager;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Inicializar el array de detección de enemigos
        enemigosEnRango = new bool[puntosDeDisparo.Length];

        // Verificar el material al inicio
        VerificarMaterial();

        NotificarSpawn();
    }

    void Update()
    {
        // Verificar si el material de la torreta es el correcto
        if (TieneMaterialCorrecto())
        {
            // Verificar si hay enemigos dentro de las áreas de detección
            VerificarEnemigosEnRango();

            // Disparar solo si hay un enemigo en rango y es momento de disparar
            if (Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time; // Actualizar el tiempo del último disparo
            }
        }
    }

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

    private void VerificarMaterial()
    {
        bool materialCorrecto = TieneMaterialCorrecto();

        // Activar o desactivar los objetos según el material
        foreach (GameObject obj in objetosMaterialIncorrecto)
        {
            if (obj != null)
            {
                obj.SetActive(!materialCorrecto);
            }
        }
    }

    private void VerificarEnemigosEnRango()
    {
        // Buscar todos los objetos con el tag "Enemigo" en la escena
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");

        // Inicialmente asumir que no hay enemigos en rango en ninguna dirección
        for (int i = 0; i < enemigosEnRango.Length; i++)
        {
            enemigosEnRango[i] = false;
        }

        // Recorrer todos los enemigos
        foreach (GameObject enemigo in enemigos)
        {
            // Verificar si el enemigo está dentro de alguna de las cajas
            for (int i = 0; i < puntosDeDisparo.Length; i++)
            {
                Vector3 centroCaja = puntosDeDisparo[i].position + desplazamientosCajas[i];
                if (EstaEnCaja(enemigo.transform.position, centroCaja, tamanosCajas[i], rotacionesCajas[i]))
                {
                    enemigosEnRango[i] = true;
                }
            }
        }
    }

    private bool EstaEnCaja(Vector3 posicionEnemigo, Vector3 centroCaja, Vector3 tamanoCaja, Vector3 rotacionCaja)
    {
        // Crear una matriz de rotación para la caja
        Quaternion rotacion = Quaternion.Euler(rotacionCaja);

        // Convertir la posición del enemigo al espacio local de la caja
        Vector3 posicionLocal = Quaternion.Inverse(rotacion) * (posicionEnemigo - centroCaja);

        // Calcular los límites de la caja en su espacio local
        Vector3 min = -tamanoCaja / 2;
        Vector3 max = tamanoCaja / 2;

        // Verificar si la posición local del enemigo está dentro de los límites de la caja
        return posicionLocal.x >= min.x && posicionLocal.x <= max.x &&
               posicionLocal.y >= min.y && posicionLocal.y <= max.y &&
               posicionLocal.z >= min.z && posicionLocal.z <= max.z;
    }

    private void Disparar()
    {
        // Recorrer todos los puntos de disparo
        for (int i = 0; i < puntosDeDisparo.Length; i++)
        {
            // Disparar solo si hay un enemigo en la caja de colisión correspondiente
            if (enemigosEnRango[i])
            {
                // Instanciar el proyectil en la posición y rotación del punto de disparo
                GameObject proyectil = Instantiate(proyectilPrefab, puntosDeDisparo[i].position, puntosDeDisparo[i].rotation);

                // Obtener el componente Rigidbody del proyectil (si lo tiene)
                Rigidbody rb = proyectil.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Aplicar fuerza al proyectil en la dirección hacia adelante del punto de disparo
                    rb.linearVelocity = puntosDeDisparo[i].forward * 10f; // Ajusta la velocidad según sea necesario
                }
            }
        }
    }

    // Dibujar las áreas de detección en el Viewport
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Dibujar las cajas de colisión para cada punto de disparo
        for (int i = 0; i < puntosDeDisparo.Length; i++)
        {
            Vector3 centroCaja = puntosDeDisparo[i].position + desplazamientosCajas[i];
            DibujarCajaGizmo(centroCaja, tamanosCajas[i], rotacionesCajas[i]);
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
        // Verificar si la torreta tiene el material correcto antes de sumar puntos
        if (TieneMaterialCorrecto())
        {
            gameManager.SumarUnidades(puntosARecuperar); // Sumar unidades al slider
        }
    }
}