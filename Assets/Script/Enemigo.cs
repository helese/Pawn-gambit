using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemigo : MonoBehaviour
{
    [Header("Configuración básica")]
    public float velocidad = 2f;
    public float tiempoDeEspera = 0.4f;
    public int vidaMaxima = 100;
    public int incrementoVidaPorOleada = 10;
    public int dañoATorreRey = 5;
    public Slider sliderVida;
    public string CasillaASeguir;

    [Header("Configuración del jefe")]
    public bool esJefe = false;
    public int vidaExtraJefe = 200;

    [Header("Puntuación")]
    public int puntuacionMinima = 10;
    public int puntuacionMaxima = 50;

    [Header("Regeneración")] // Nuevo header para regeneración
    public bool regeneraVida = false; // Activar/desactivar regeneración desde el Inspector
    public float vidaPorSegundo = 5f; // Cantidad de vida recuperada por segundo
    public float tiempoDeRegeneración = 2f;
    public int aumentoDeRegeneracionPorOleada = 2;

    [Header("Invencibilidad")] // Nuevo header
    public bool invencible = false; // Activar/desactivar desde Inspector
    public float tiempoInvencibilidad = 3f; // Duración de la invencibilidad
    public float cooldownInvencibilidad = 10f; // Tiempo entre activaciones
    private bool puedeSerInvencible = true; // Control interno del cooldown
    private bool esInvencible = false; // Estado actual

    [Header("Invencibilidad al Spawn")] // Nuevo header
    public bool invencibleAlSpawn = false; // Activar/desactivar desde Inspector
    public float tiempoInvencibilidadSpawn = 3f; // Tiempo de invencibilidad inicial

    [Header("Aura de Velocidad")] // Nuevo header
    public bool tieneAuraVelocidad = false;
    public float radioAura = 5f;
    public float porcentajeBoostVelocidad = 20f;
    public Color colorAura = Color.green;
    public float intervaloActualizacionAura = 0.5f; // Intervalo para optimizar

    private Dictionary<Enemigo, float> enemigosDentroDelArea = new Dictionary<Enemigo, float>();
    private Color colorOriginalSlider;
    private float tiempoParaActualizarAura;

    [Header("Curación al Morir")]
    public bool curarAliadosAlMorir = false; // Activar/desactivar desde el Inspector
    public float rangoCuracion = 5f; // Rango en unidades de Unity
    public int curacion = 100; // Cantidad de vida que cura


    [Header("Bestiario")]
    public string idEnemigo; // ID único (ej: "Goblin_01")

    private int vidaActual;
    private Transform[] waypoints;
    private int indiceWaypointActual = 0;

    // Evento para notificar la destrucción del enemigo
    public delegate void EnemigoDestruidoHandler(string idEnemigo);

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
        if (sliderVida != null)
        {
            colorOriginalSlider = sliderVida.fillRect.GetComponent<Image>().color;
        }

        AsignarCamaraAlCanvas();
        BuscarCasillasRojas();

        // Iniciar la corrutina de movimiento
        if (waypoints != null && waypoints.Length > 0)
        {
            StartCoroutine(MoverEnemigo());
        }
        if (regeneraVida)
        {
            StartCoroutine(RegenerarVida());
        }
        if (invencibleAlSpawn)
        {
            StartCoroutine(InvencibilidadAlSpawn());
        }
    }

    void Update()
    {
        if (tieneAuraVelocidad && Time.time >= tiempoParaActualizarAura)
        {
            ActualizarAuraVelocidad();
            tiempoParaActualizarAura = Time.time + intervaloActualizacionAura;
        }
    }

    // Método para asignar la cámara principal al Canvas
    private void AsignarCamaraAlCanvas()
    {
        // Buscar la cámara llamada "Main Camera"
        Camera mainCamera = GameObject.Find("Main Camera")?.GetComponent<Camera>();

        if (mainCamera != null)
        {
            // Obtener el componente Canvas del enemigo
            Canvas canvas = GetComponentInChildren<Canvas>();

            if (canvas != null)
            {
                // Asignar la cámara al Canvas
                canvas.worldCamera = mainCamera;
                canvas.planeDistance = 10; // Ajusta este valor según sea necesario
            }
            else
            {
                Debug.LogWarning("No se encontró un componente Canvas en el enemigo.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró una cámara llamada 'Main Camera'.");
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
            // Obtener la posición del waypoint actual
            Transform waypointActual = waypoints[indiceWaypointActual];

            // Calcular la dirección hacia el waypoint actual
            Vector3 direccion = (waypointActual.position - transform.position).normalized;

            // Mover al enemigo en incrementos de una unidad
            while (Vector3.Distance(transform.position, waypointActual.position) > 0.01f)
            {
                // Calcular la siguiente posición (una unidad más adelante)
                Vector3 siguientePosicion = transform.position + direccion * 1f; // 1 unidad de movimiento

                // Mover de manera fluida hacia la siguiente posición
                while (Vector3.Distance(transform.position, siguientePosicion) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, siguientePosicion, velocidad * Time.deltaTime);
                    yield return null; // Esperar al siguiente frame
                }

                // Asegurarse de que el enemigo esté exactamente en la siguiente posición
                transform.position = siguientePosicion;

                // Esperar el tiempo definido después de cada unidad de movimiento
                yield return new WaitForSeconds(tiempoDeEspera);
            }
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }

    IEnumerator RegenerarVida()
    {
        while (true && regeneraVida) // Solo funciona si está activado
        {
            yield return new WaitForSeconds(tiempoDeRegeneración); 

            // Regenerar vida sin exceder el máximo
            vidaActual = Mathf.Min(vidaActual + (int)vidaPorSegundo + incrementoVidaPorOleada, vidaMaxima);

            // Actualizar slider
            if (sliderVida != null)
            {
                sliderVida.value = vidaActual;
            }
        }
    }
    IEnumerator ActivarInvencibilidad()
    {
        puedeSerInvencible = false; // Bloquear nuevas activaciones
        esInvencible = true;

        // Cambiar color del slider (opcional)
        if (sliderVida != null)
            sliderVida.fillRect.GetComponent<Image>().color = Color.cyan;

        yield return new WaitForSeconds(tiempoInvencibilidad);

        esInvencible = false;

        // Restaurar color (opcional)
        if (sliderVida != null)
            sliderVida.fillRect.GetComponent<Image>().color = Color.red;

        // Cooldown antes de poder activarse de nuevo
        yield return new WaitForSeconds(cooldownInvencibilidad);
        puedeSerInvencible = true;
    }

    IEnumerator InvencibilidadAlSpawn()
    {
        esInvencible = true;

        // Cambiar color del slider (opcional)
        if (sliderVida != null)
        {
            sliderVida.fillRect.GetComponent<Image>().color = Color.cyan;
        }

        yield return new WaitForSeconds(tiempoInvencibilidadSpawn);

        esInvencible = false;

        // Restaurar color original
        if (sliderVida != null)
        {
            sliderVida.fillRect.GetComponent<Image>().color = Color.red;
        }
    }

    private void ActualizarAuraVelocidad()
    {
        // 1. Detectar enemigos dentro del área
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioAura);
        HashSet<Enemigo> enemigosActuales = new HashSet<Enemigo>();

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemigo") && col.gameObject != this.gameObject)
            {
                Enemigo enemigo = col.GetComponent<Enemigo>();
                if (enemigo != null)
                {
                    enemigosActuales.Add(enemigo);

                    // Si es nuevo, aplicar boost
                    if (!enemigosDentroDelArea.ContainsKey(enemigo))
                    {
                        enemigosDentroDelArea[enemigo] = enemigo.velocidad; // Guardar velocidad original
                        enemigo.velocidad *= (1 + porcentajeBoostVelocidad / 100f);

                        // Cambiar color
                        if (enemigo.sliderVida != null)
                        {
                            enemigo.sliderVida.fillRect.GetComponent<Image>().color = colorAura;
                        }
                    }
                }
            }
        }

        // 2. Buscar enemigos que salieron del área
        List<Enemigo> enemigosParaRemover = new List<Enemigo>();
        foreach (var kvp in enemigosDentroDelArea)
        {
            if (!enemigosActuales.Contains(kvp.Key))
            {
                // Restaurar velocidad
                kvp.Key.velocidad = kvp.Value;

                // Restaurar color
                if (kvp.Key.sliderVida != null)
                {
                    kvp.Key.sliderVida.fillRect.GetComponent<Image>().color = colorOriginalSlider;
                }

                enemigosParaRemover.Add(kvp.Key);
            }
        }

        // 3. Limpiar diccionario
        foreach (Enemigo enemigo in enemigosParaRemover)
        {
            enemigosDentroDelArea.Remove(enemigo);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (tieneAuraVelocidad)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radioAura);
        }
    }

    private void CurarAliados()
    {
        if (!curarAliadosAlMorir) return; // Si no está activado, no hacer nada

        // Buscar todos los enemigos en el rango
        Collider[] colliders = Physics.OverlapSphere(transform.position, rangoCuracion);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemigo") && col.gameObject != this.gameObject) // Evitar curarse a sí mismo
            {
                Enemigo aliado = col.GetComponent<Enemigo>();
                if (aliado != null)
                {
                    // Curar al aliado
                    aliado.RecibirCuracion(curacion);
                }
            }
        }
    }

    // Método para recibir curación (añadir al script del enemigo)
    public void RecibirCuracion(int cantidad)
    {
        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima); // No superar la vida máxima
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual; // Actualizar slider
        }
    }
    // Método para recibir daño
    public void RecibirDaño(int daño)
    {
        if (esInvencible) return; // Si es invencible, ignora el daño

        vidaActual -= daño; // Reducir la vida actual

        // Activar invencibilidad si está configurado
        if (invencible && puedeSerInvencible)
        {
            StartCoroutine(ActivarInvencibilidad());
        }

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

    // Método para manejar la muerte del enemigo
    private void Morir()
    {
        CurarAliados();
        // Notificar al GameManager que un enemigo ha sido destruido
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            if (esJefe)
            {
                // Si es un jefe, notificar su destrucción
                gameManager.NotificarJefeDestruido();
            }
            else
            {
                // Si es un enemigo normal, notificar su destrucción
                gameManager.NotificarEnemigoDestruido();
            }

            if (tieneAuraVelocidad)
            {
                foreach (var kvp in enemigosDentroDelArea)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.velocidad = kvp.Value;
                        if (kvp.Key.sliderVida != null)
                        {
                            kvp.Key.sliderVida.fillRect.GetComponent<Image>().color = colorOriginalSlider;
                        }
                    }
                }
                enemigosDentroDelArea.Clear();
            }

            // Otorgar puntuación aleatoria al ser destruido
            int score = Random.Range(puntuacionMinima, puntuacionMaxima + 1);
            ScoreManager.Instance.AddScore(score);
        }

        BestiarioManager.Instance.RegistrarEnemigoDerrotado(idEnemigo);

        // Destruir el objeto enemigo
        Destroy(gameObject);
    }

    // Método para obtener el daño que inflige el enemigo a la TorreRey
    public int ObtenerDañoATorreRey()
    {
        return dañoATorreRey;
    }
}