using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemigo : MonoBehaviour
{
    [Header("Configuraci�n b�sica")]
    public float velocidad = 2f;
    public float tiempoDeEspera = 0.4f;
    public int vidaMaxima = 100;
    public int incrementoVidaPorOleada = 10;
    public int da�oATorreRey = 5;
    public Slider sliderVida;
    public string CasillaASeguir;

    [Header("Configuraci�n del jefe")]
    public bool esJefe = false;
    public int vidaExtraJefe = 200;

    [Header("Puntuaci�n")]
    public int puntuacionMinima = 10;
    public int puntuacionMaxima = 50;

    [Header("Bestiario")]
    public string idEnemigo; // ID �nico (ej: "Goblin_01")

    private int vidaActual;
    private Transform[] waypoints;
    private int indiceWaypointActual = 0;

    // Evento para notificar la destrucci�n del enemigo
    public delegate void EnemigoDestruidoHandler(string idEnemigo);
    public static event EnemigoDestruidoHandler OnEnemigoDestruido;

    void Start()
    {
        // Obtener la oleada actual del GameManager
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            vidaMaxima += incrementoVidaPorOleada * (gameManager.oleadaActual - 1);

            if (esJefe)
            {
                vidaMaxima += vidaExtraJefe;
            }
        }

        vidaActual = vidaMaxima;

        if (sliderVida != null)
        {
            sliderVida.maxValue = vidaMaxima;
            sliderVida.value = vidaActual;
        }

        AsignarCamaraAlCanvas();
        BuscarCasillasRojas();

        // Iniciar la corrutina de movimiento
        if (waypoints != null && waypoints.Length > 0)
        {
            StartCoroutine(MoverEnemigo());
        }
    }

    // M�todo para asignar la c�mara principal al Canvas
    private void AsignarCamaraAlCanvas()
    {
        // Buscar la c�mara llamada "Main Camera"
        Camera mainCamera = GameObject.Find("Main Camera")?.GetComponent<Camera>();

        if (mainCamera != null)
        {
            // Obtener el componente Canvas del enemigo
            Canvas canvas = GetComponentInChildren<Canvas>();

            if (canvas != null)
            {
                // Asignar la c�mara al Canvas
                canvas.worldCamera = mainCamera;
                canvas.planeDistance = 10; // Ajusta este valor seg�n sea necesario
            }
            else
            {
                Debug.LogWarning("No se encontr� un componente Canvas en el enemigo.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontr� una c�mara llamada 'Main Camera'.");
        }
    }

    void BuscarCasillasRojas()
    {
        // Buscar todos los objetos en la escena que tienen el tag especificado
        GameObject[] casillas = GameObject.FindGameObjectsWithTag(CasillaASeguir);

        // Si no se encontraron casillas, mostrar un mensaje de advertencia
        if (casillas == null || casillas.Length == 0)
        {
            Debug.LogWarning($"No se encontraron objetos con el tag '{CasillaASeguir}'.");
            return;
        }

        // Crear el array de waypoints y asignar las posiciones de las casillas
        waypoints = new Transform[casillas.Length];
        for (int i = 0; i < casillas.Length; i++)
        {
            waypoints[i] = casillas[i].transform;
        }
    }

    IEnumerator MoverEnemigo()
    {
        while (true) // Bucle infinito para moverse entre waypoints
        {
            // Obtener la posici�n del waypoint actual
            Transform waypointActual = waypoints[indiceWaypointActual];

            // Calcular la direcci�n hacia el waypoint actual
            Vector3 direccion = (waypointActual.position - transform.position).normalized;

            // Mover al enemigo en incrementos de una unidad
            while (Vector3.Distance(transform.position, waypointActual.position) > 0.01f)
            {
                // Calcular la siguiente posici�n (una unidad m�s adelante)
                Vector3 siguientePosicion = transform.position + direccion * 1f; // 1 unidad de movimiento

                // Mover de manera fluida hacia la siguiente posici�n
                while (Vector3.Distance(transform.position, siguientePosicion) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, siguientePosicion, velocidad * Time.deltaTime);
                    yield return null; // Esperar al siguiente frame
                }

                // Asegurarse de que el enemigo est� exactamente en la siguiente posici�n
                transform.position = siguientePosicion;

                // Esperar el tiempo definido despu�s de cada unidad de movimiento
                yield return new WaitForSeconds(tiempoDeEspera);
            }
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }

    // M�todo para recibir da�o
    public void RecibirDa�o(int da�o)
    {
        vidaActual -= da�o; // Reducir la vida actual

        // Actualizar el Slider de vida
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual;
        }

        // Verificar si el enemigo ha muerto
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    // M�todo para manejar la muerte del enemigo
    private void Morir()
    {
        // Notificar al GameManager que un enemigo ha sido destruido
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            if (esJefe)
            {
                // Si es un jefe, notificar su destrucci�n
                gameManager.NotificarJefeDestruido();
            }
            else
            {
                // Si es un enemigo normal, notificar su destrucci�n
                gameManager.NotificarEnemigoDestruido();
            }

            // Otorgar puntuaci�n aleatoria al ser destruido
            int score = Random.Range(puntuacionMinima, puntuacionMaxima + 1);
            ScoreManager.Instance.AddScore(score);
        }

        BestiarioManager.Instance.RegistrarEnemigoDerrotado(idEnemigo);

        // Destruir el objeto enemigo
        Destroy(gameObject);
    }

    // M�todo para obtener el da�o que inflige el enemigo a la TorreRey
    public int ObtenerDa�oATorreRey()
    {
        return da�oATorreRey;
    }
}