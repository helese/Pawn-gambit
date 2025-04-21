using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class Jugador : MonoBehaviour
{
    [Header("Movimiento Ajedrez")]
    public float velocidadMovimiento = 10f;
    public float tiempoEntreMovimientos = 0.2f;
    private bool seEstaMoviendo = false;
    private bool turboActivo = false;
    private Vector3 posicionObjetivo;
    private Transform camara;

    [Header("Teleportaci�n")]
    public Vector3 coordenadaTeleport1;
    public Vector3 coordenadaTeleport2;
    public Vector3 coordenadaTeleport3;
    public Vector3 coordenadaTeleport4;
    public float tiempoEsperaPostTeleport = 0.5f;

    [Header("Interacci�n")]
    public float radioInteraccion = 3f;
    private bool puedeMoverse = true;
    private GameObject canvasConstruccion;
    public bool canvasActivo = false;
    public Button[] botonesConstruccion;
    public GameObject[] prefabsConstruccion;
    private Vector3 posicionCasillaSeleccionada;
    private Rigidbody rb; // Referencia al Rigidbody

    [Header("Destrucci�n de Torres")]
    public GameObject canvasDestruccion;
    private GameObject torreSeleccionada;
    public TextMeshProUGUI textoPuntosRecuperar;

    [Header("Previsualizaci�n")]
    public Material materialPreview;
    private GameObject previewActual;
    private GameObject objetoInteractuableActual;

    [Header("Detecci�n de Enemigos")]
    public GameObject colliderFrontal;
    public GameObject colliderTrasero;
    public GameObject colliderIzquierdo;
    public GameObject colliderDerecho;

    public MoverCamara moverCamara;
    private bool camavaraSeMovio = false;

    public GameObject panelPausa;

    private bool enemigoEnFrente;
    private bool enemigoAtras;
    private bool enemigoIzquierda;
    private bool enemigoDerecha;

    // Referencia al componente de f�sica para raycasts
    private PhysicsRaycaster physicsRaycaster;
    private bool raycastEnEspera = false;
    private float tiempoEsperaRaycast = 0.1f;

    // Capa para ignorar raycast
    private LayerMask capaOriginalUI;

    void Start()
    {
        // Configura los colliders como triggers (si no lo est�n)
        colliderFrontal.GetComponent<Collider>().isTrigger = true;
        colliderTrasero.GetComponent<Collider>().isTrigger = true;
        colliderIzquierdo.GetComponent<Collider>().isTrigger = true;
        colliderDerecho.GetComponent<Collider>().isTrigger = true;

        posicionObjetivo = transform.position;
        camara = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        // Obtener referencia al PhysicsRaycaster de la c�mara principal
        physicsRaycaster = Camera.main.GetComponent<PhysicsRaycaster>();
        if (physicsRaycaster == null)
        {
            physicsRaycaster = Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
        }

        // Guardar la configuraci�n original del PhysicsRaycaster
        if (physicsRaycaster != null)
        {
            capaOriginalUI = physicsRaycaster.eventMask;
        }

        // Inicializaci�n de UI (mantener tu c�digo existente)
        canvasConstruccion = GameObject.FindGameObjectWithTag("CanvasConstruccion");
        if (canvasConstruccion != null) canvasConstruccion.SetActive(false);

        canvasDestruccion = GameObject.FindGameObjectWithTag("CanvasDestruccion");
        if (canvasDestruccion != null) canvasDestruccion.SetActive(false);

        Button botonDestruir = canvasDestruccion.GetComponentInChildren<Button>();
        if (botonDestruir != null) botonDestruir.onClick.AddListener(DestruirTorre);

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null) GameManager.OnTorreReyDestruida += DesactivarMovimiento;

        for (int i = 0; i < botonesConstruccion.Length; i++)
        {
            int index = i;
            botonesConstruccion[i].onClick.AddListener(() => OnBotonConstruccionClic(index));
        }

        foreach (Button boton in botonesConstruccion)
        {
            AddHoverEvents(boton);
        }
    }

    void Update()
    {
        // Gestionar el estado de los raycasts seg�n si hay canvas activo
        GestionarEstadoRaycasts();

        // Controles de teleportaci�n
        if (puedeMoverse && !seEstaMoviendo && !canvasActivo && !canvasDestruccion.activeSelf)
        {
            // Teleportar con teclas num�ricas
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                TeleportarJugador(coordenadaTeleport1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                TeleportarJugador(coordenadaTeleport2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                TeleportarJugador(coordenadaTeleport3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                TeleportarJugador(coordenadaTeleport4);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!camavaraSeMovio)
            {
                moverCamara.IniciarMovimiento1();
                camavaraSeMovio = true;
            }
            else
            {
                moverCamara.ReiniciarPosicion();
                camavaraSeMovio = false;
            }
        }
        if (puedeMoverse && !seEstaMoviendo && !canvasActivo && !canvasDestruccion.activeSelf)
        {
            ProcesarMovimiento();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            turboActivo = !turboActivo;
        }

        if (turboActivo)
        {
            velocidadMovimiento = 17f;
            tiempoEntreMovimientos = 0.1f;
        }
        else
        {
            velocidadMovimiento = 10f;
            tiempoEntreMovimientos = 0.2f;
        }
        if (Input.GetMouseButtonUp(0) && puedeMoverse && panelPausa != null && !panelPausa.activeSelf)
        {
            if (!canvasActivo && !canvasDestruccion.activeSelf && !raycastEnEspera)
            {
                VerificarInteraccion();
                VerificarInteraccionTorre();
            }
        }

        if (Input.GetMouseButtonUp(1) && panelPausa != null && !panelPausa.activeSelf)
        {
            if (canvasActivo && objetoInteractuableActual != null) DesactivarCanvas();
            if (canvasDestruccion != null) canvasDestruccion.SetActive(false);
        }
    }

    // M�todo para teleportar al jugador a una posici�n espec�fica
    private void TeleportarJugador(Vector3 destino)
    {
        if (seEstaMoviendo) return;

        seEstaMoviendo = true;

        // Redondear a coordenadas enteras para mantener la consistencia
        Vector3 posicionFinal = new Vector3(
            Mathf.Round(destino.x),
            Mathf.Round(destino.y),
            Mathf.Round(destino.z)
        );

        // Actualizar la posici�n del jugador
        transform.position = posicionFinal;

        // Sincronizar el Rigidbody
        if (rb != null)
        {
            rb.position = posicionFinal;
            rb.linearVelocity = Vector3.zero;
        }

        // Actualizar la posici�n objetivo
        posicionObjetivo = posicionFinal;

        // Desactivar movimiento temporalmente
        StartCoroutine(DesactivarMovimientoTemporal());

        Debug.Log($"Teleportado a: {posicionFinal}");
    }

    // Corrutina para desactivar movimiento brevemente tras teleportaci�n
    private IEnumerator DesactivarMovimientoTemporal()
    {
        yield return new WaitForSeconds(tiempoEsperaPostTeleport);
        seEstaMoviendo = false;
    }

    // M�todo mejorado para gestionar el estado de los raycasts
    private void GestionarEstadoRaycasts()
    {
        bool hayCanvasActivo = canvasActivo || (canvasDestruccion != null && canvasDestruccion.activeSelf);

        if (physicsRaycaster != null)
        {
            // Si hay un canvas activo, desactivar los raycasts
            physicsRaycaster.enabled = !hayCanvasActivo && !raycastEnEspera;

            // Alternativa: en lugar de desactivar completamente, configurar para ignorar la capa UI
            // if (hayCanvasActivo || raycastEnEspera)
            // {
            //     // Ignorar la capa UI
            //     physicsRaycaster.eventMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("UI");
            // }
            // else
            // {
            //     // Restaurar capas originales
            //     physicsRaycaster.eventMask = capaOriginalUI;
            // }
        }
    }

    void LateUpdate()
    {
        if (!seEstaMoviendo && puedeMoverse)
        {
            // Mantiene al jugador alineado incluso cuando no se mueve
            Vector3 posicionAlineada = new Vector3(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y),
                Mathf.Round(transform.position.z)
            );

            if (transform.position != posicionAlineada)
            {
                transform.position = posicionAlineada;
                if (rb != null) rb.position = posicionAlineada;
            }
        }
    }

    private void ProcesarMovimiento()
    {
        float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        float movimientoVertical = Input.GetAxisRaw("Vertical");

        if (movimientoHorizontal != 0 || movimientoVertical != 0)
        {
            Vector3 direccion = (camara.forward * movimientoVertical + camara.right * movimientoHorizontal).normalized;
            direccion.y = 0;

            // Determinar direcci�n primaria
            Vector3 direccionLateral = Vector3.zero;
            Vector3 direccionFrontal = Vector3.zero;

            if (Mathf.Abs(direccion.x) > Mathf.Abs(direccion.z))
            {
                direccionLateral = new Vector3(Mathf.Sign(direccion.x), 0, 0);
            }
            else
            {
                direccionFrontal = new Vector3(0, 0, Mathf.Sign(direccion.z));
            }

            // Verificar colisiones
            bool bloqueoLateral = (direccionLateral.x > 0 && enemigoDerecha) || (direccionLateral.x < 0 && enemigoIzquierda);
            bool bloqueoFrontal = (direccionFrontal.z > 0 && enemigoEnFrente) || (direccionFrontal.z < 0 && enemigoAtras);

            if (!bloqueoLateral && !bloqueoFrontal)
            {
                posicionObjetivo = transform.position + direccionLateral + direccionFrontal;
                StartCoroutine(MoverACasilla(posicionObjetivo));
            }
        }
    }

    // M�todos para detectar enemigos (llamados desde los colliders hijos)
    public void EnemigoEntro(string direccion)
    {
        switch (direccion)
        {
            case "Frontal": enemigoEnFrente = true; break;
            case "Trasero": enemigoAtras = true; break;
            case "Izquierdo": enemigoIzquierda = true; break;
            case "Derecho": enemigoDerecha = true; break;
        }
    }

    public void EnemigoSalio(string direccion)
    {
        switch (direccion)
        {
            case "Frontal": enemigoEnFrente = false; break;
            case "Trasero": enemigoAtras = false; break;
            case "Izquierdo": enemigoIzquierda = false; break;
            case "Derecho": enemigoDerecha = false; break;
        }
    }


    IEnumerator MoverACasilla(Vector3 objetivo)
    {
        seEstaMoviendo = true;

        Vector3 posicionInicial = transform.position;
        float tiempoTranscurrido = 0f;

        // Mueve suavemente al jugador
        while (Vector3.Distance(transform.position, objetivo) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                objetivo,
                velocidadMovimiento * Time.deltaTime
            );
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Fuerza la posici�n final a coordenadas enteras
        Vector3 posicionFinal = new Vector3(
            Mathf.Round(objetivo.x),
            Mathf.Round(objetivo.y),
            Mathf.Round(objetivo.z)
        );

        transform.position = posicionFinal;

        // Sincroniza el Rigidbody si existe
        if (rb != null)
        {
            rb.position = posicionFinal;
            rb.linearVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(tiempoEntreMovimientos);
        seEstaMoviendo = false;
    }


    // M�todo para desactivar el movimiento del jugador
    public void DesactivarMovimiento()
    {
        puedeMoverse = false;

    }

    // M�todo para activar el movimiento del jugador
    public void ActivarMovimiento()
    {

        puedeMoverse = true;
    }

    // M�todo para verificar la interacci�n con objetos interactuables
    private void VerificarInteraccion()
    {
        // No realizar raycast si hay un canvas activo o estamos en espera
        if (canvasActivo || canvasDestruccion.activeSelf || raycastEnEspera) return;

        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);

        // --- Visualizaci�n del Raycast (solo en el Editor) ---
        Debug.DrawRay(rayo.origin, rayo.direction * 100f, Color.green, 1f); // L�nea verde durante 1 segundo

        RaycastHit hit = Physics.RaycastAll(rayo)
            .Where(h => !h.collider.CompareTag("TagAIgnorar") && h.collider.gameObject.layer != LayerMask.NameToLayer("UI"))
            .OrderBy(h => h.distance)
            .FirstOrDefault();

        if (hit.collider != null)
        {
            Debug.Log($"Objeto pulsado: {hit.collider.gameObject.name} | Posici�n: {hit.point}");


            if (hit.collider.CompareTag("ObjetoInteractuable"))
            {
                float distancia = Vector3.Distance(transform.position, hit.transform.position);
                if (distancia <= radioInteraccion)
                {
                    objetoInteractuableActual = hit.collider.gameObject;

                    Transform puntoDeInstancia = hit.collider.transform.Find("PuntoDeInstancia");
                    if (puntoDeInstancia != null)
                    {
                        posicionCasillaSeleccionada = puntoDeInstancia.position;

                        // --- Visualizaci�n del punto de instancia (opcional) ---
                        Debug.DrawLine(hit.point, puntoDeInstancia.position, Color.blue, 1f);
                    }
                    else
                    {
                        Debug.LogWarning("No se encontr� el objeto hijo 'PuntoDeInstancia' en la casilla.");
                        posicionCasillaSeleccionada = hit.point;
                    }

                    if (canvasConstruccion != null)
                    {
                        canvasConstruccion.SetActive(true);
                        canvasActivo = true;

                        // Asegurarse de que el canvas de destrucci�n est� desactivado
                        if (canvasDestruccion != null)
                        {
                            canvasDestruccion.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void OnBotonConstruccionClic(int index)
    {
        if (previewActual != null && index >= 0 && index < prefabsConstruccion.Length)
        {
            // Instanciar versi�n final
            GameObject construccion = Instantiate(
                prefabsConstruccion[index],
                previewActual.transform.position,
                previewActual.transform.rotation,
                objetoInteractuableActual.transform
            );

            // Aplicar materiales originales
            ResetMaterials(construccion, prefabsConstruccion[index]);

            DestruirPreview();

            // Asegurar que los canvas se desactiven
            DesactivarCanvas();

            // Log para depuraci�n
            Debug.Log("Bot�n de construcci�n pulsado. Desactivando canvas.");
        }
        else
        {
            // Desactivar canvas incluso si no hay un preview
            DesactivarCanvas();
            Debug.LogWarning("Se intent� construir sin preview o con �ndice inv�lido.");
        }
    }

    public void DesactivarCanvas()
    {
        // Desactiva el canvas de construcci�n
        if (canvasConstruccion != null)
        {
            canvasConstruccion.SetActive(false);
        }

        // Desactiva el canvas de destrucci�n
        if (canvasDestruccion != null)
        {
            canvasDestruccion.SetActive(false);
        }

        // Actualiza la variable de control
        canvasActivo = false;

        // Elimina cualquier preview que pudiera estar activo
        DestruirPreview();

        // Limpia la referencia al objeto interactuable
        objetoInteractuableActual = null;

        // Activar el per�odo de espera para los raycasts
        StartCoroutine(ReactivarRaycast());

        // Log para depuraci�n
        Debug.Log("Canvas desactivados. canvasActivo = " + canvasActivo);
    }

    // Corrutina para reactivar los raycasts despu�s de un tiempo de espera
    private IEnumerator ReactivarRaycast()
    {
        raycastEnEspera = true;
        yield return new WaitForSeconds(tiempoEsperaRaycast);
        raycastEnEspera = false;
    }

    private void VerificarInteraccionTorre()
    {
        // No realizar raycast si hay un canvas activo o estamos en espera
        if (canvasActivo || canvasDestruccion.activeSelf || raycastEnEspera) return;

        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Usar LayerMask para ignorar la capa UI
        int layerMask = ~LayerMask.GetMask("UI");

        if (Physics.Raycast(rayo, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.CompareTag("Torreta"))
            {
                torreSeleccionada = hit.collider.gameObject;

                // Obtener el script Torreta y mostrar los puntos
                Torreta torreta = torreSeleccionada.GetComponent<Torreta>();
                if (torreta != null && textoPuntosRecuperar != null)
                {
                    textoPuntosRecuperar.text = torreta.puntosARecuperar.ToString();

                    // Opcional: Formatear el texto con estilo
                    // textoPuntosRecuperar.text = $"Recuperar: <color=green>{torreta.puntosARecuperar}</color> puntos";
                }

                if (canvasDestruccion != null)
                {
                    // Desactivar el canvas de construcci�n si est� activo
                    if (canvasConstruccion != null && canvasConstruccion.activeSelf)
                    {
                        canvasConstruccion.SetActive(false);
                    }

                    // Activar el canvas de destrucci�n
                    canvasDestruccion.SetActive(true);
                }
            }
        }
    }

    // M�todo para destruir la torre seleccionada
    private void DestruirTorre()
    {
        if (torreSeleccionada != null)
        {
            Debug.Log("Destruyendo torre: " + torreSeleccionada.name);
            Destroy(torreSeleccionada);
            torreSeleccionada = null;
        }

        // Desactivar ambos canvas
        DesactivarCanvas();
    }

    // Dibujar gizmo para visualizar el radio de interacci�n
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);

        // Dibujar puntos de teleportaci�n
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(coordenadaTeleport1, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(coordenadaTeleport2, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(coordenadaTeleport3, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(coordenadaTeleport4, 0.5f);
    }

    private void AddHoverEvents(Button boton)
    {
        EventTrigger trigger = boton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = boton.gameObject.AddComponent<EventTrigger>();

        // Evento Pointer Enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnBotonHover(boton); });
        trigger.triggers.Add(entryEnter);

        // Evento Pointer Exit
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnBotonHoverExit(); });
        trigger.triggers.Add(entryExit);
    }

    private void DestruirPreview()
    {
        if (previewActual != null)
        {
            Destroy(previewActual);
            previewActual = null;
        }
    }

    // Cuando el puntero entra en el bot�n
    private void OnBotonHover(Button botonHover)
    {
        int index = System.Array.IndexOf(botonesConstruccion, botonHover);
        if (index != -1 && index < prefabsConstruccion.Length)
        {
            // Destruir preview anterior
            DestruirPreview();

            // Instanciar preview
            previewActual = Instantiate(
                prefabsConstruccion[index],
                posicionCasillaSeleccionada,
                Quaternion.identity,
                objetoInteractuableActual.transform
            );

            // Aplicar material de preview
            SetPreviewMaterial(previewActual);
        }
    }

    // Cuando el puntero sale del bot�n
    private void OnBotonHoverExit()
    {
        if (previewActual != null)
        {
            Destroy(previewActual);
            previewActual = null;
        }
    }

    // Aplicar material de previsualizacion
    private void SetPreviewMaterial(GameObject preview)
    {
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Crear materiales basados en el material preview, pero mantener su color base
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(materialPreview);

                // Mantener el color original del material preview, solo ajustando la transparencia
                Color materialColor = materialPreview.color;
                materials[i].color = new Color(materialColor.r, materialColor.g, materialColor.b, 0.5f);
            }
            renderer.materials = materials;
        }
    }

    // Restaurar materiales originales
    private void ResetMaterials(GameObject target, GameObject originalPrefab)
    {
        Renderer[] originalRenderers = originalPrefab.GetComponentsInChildren<Renderer>();
        Renderer[] targetRenderers = target.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < Mathf.Min(originalRenderers.Length, targetRenderers.Length); i++)
        {
            targetRenderers[i].materials = originalRenderers[i].sharedMaterials;
        }
    }
}