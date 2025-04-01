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


    [Header("Interacción")]
    public float radioInteraccion = 3f;
    private bool puedeMoverse = true;
    private GameObject canvasConstruccion;
    private bool canvasActivo = false;
    public Button[] botonesConstruccion;
    public GameObject[] prefabsConstruccion;
    private Vector3 posicionCasillaSeleccionada;
    private Rigidbody rb; // Referencia al Rigidbody

    [Header("Destrucción de Torres")]
    public GameObject canvasDestruccion;
    private GameObject torreSeleccionada;
    public TextMeshProUGUI textoPuntosRecuperar;

    [Header("Previsualización")]
    public Material materialPreview;
    private GameObject previewActual;
    private GameObject objetoInteractuableActual;

    [Header("Detección de Enemigos")]
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

    void Start()
    {

        // Configura los colliders como triggers (si no lo están)
        colliderFrontal.GetComponent<Collider>().isTrigger = true;
        colliderTrasero.GetComponent<Collider>().isTrigger = true;
        colliderIzquierdo.GetComponent<Collider>().isTrigger = true;
        colliderDerecho.GetComponent<Collider>().isTrigger = true;


        posicionObjetivo = transform.position;
        camara = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        // Inicialización de UI (mantener tu código existente)
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
            if (!canvasActivo && !canvasDestruccion.activeSelf)
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

            // Determinar dirección primaria
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

    // Métodos para detectar enemigos (llamados desde los colliders hijos)
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

        // Fuerza la posición final a coordenadas enteras
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


    // Método para desactivar el movimiento del jugador
    public void DesactivarMovimiento()
    {
        puedeMoverse = false;

    }

    // Método para activar el movimiento del jugador
    public void ActivarMovimiento()
    {

        puedeMoverse = true;
    }

    // Método para verificar la interacción con objetos interactuables
    private void VerificarInteraccion()
    {
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);

        // --- Visualización del Raycast (solo en el Editor) ---
        Debug.DrawRay(rayo.origin, rayo.direction * 100f, Color.green, 1f); // Línea verde durante 1 segundo

        RaycastHit hit = Physics.RaycastAll(rayo)
            .Where(h => !h.collider.CompareTag("TagAIgnorar"))
            .OrderBy(h => h.distance)
            .FirstOrDefault();

        if (hit.collider != null)
        {
            Debug.Log($"Objeto pulsado: {hit.collider.gameObject.name} | Posición: {hit.point}");


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

                        // --- Visualización del punto de instancia (opcional) ---
                        Debug.DrawLine(hit.point, puntoDeInstancia.position, Color.blue, 1f);
                    }
                    else
                    {
                        Debug.LogWarning("No se encontró el objeto hijo 'PuntoDeInstancia' en la casilla.");
                        posicionCasillaSeleccionada = hit.point;
                    }

                    if (canvasConstruccion != null)
                    {
                        canvasConstruccion.SetActive(true);
                        canvasActivo = true;
                    }
                }
            }
        }
    }

    private void OnBotonConstruccionClic(int index)
    {
        if (previewActual != null)
        {
            // Instanciar versión final
            GameObject construccion = Instantiate(
                prefabsConstruccion[index],
                previewActual.transform.position,
                previewActual.transform.rotation,
                objetoInteractuableActual.transform
            );

            // Aplicar materiales originales
            ResetMaterials(construccion, prefabsConstruccion[index]);

            DestruirPreview(); // Destruir la previsualización
            DesactivarCanvas();
        }
    }

    private void DesactivarCanvas()
    {
        if (canvasConstruccion != null)
        {
            canvasConstruccion.SetActive(false);
            canvasActivo = false; // Marcar el canvas como inactivo
            DestruirPreview(); // Destruir la previsualización al desactivar el canvas
            objetoInteractuableActual = null; // Limpiar la referencia al objeto interactuable
        }
    }

    private void VerificarInteraccionTorre()
    {
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(rayo, out hit))
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
                    canvasDestruccion.SetActive(true);
                }
            }
        }
    }

// Método para destruir la torre seleccionada
private void DestruirTorre()
    {
        if (torreSeleccionada != null)
        {
            Destroy(torreSeleccionada); // Destruir la torre
            torreSeleccionada = null; // Limpiar la referencia
        }

        // Ocultar el canvas de destrucción
        if (canvasDestruccion != null)
        {
            canvasDestruccion.SetActive(false);
        }
    }

    // Dibujar gizmo para visualizar el radio de interacción
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
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

    // Cuando el puntero entra en el botón
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

    // Cuando el puntero sale del botón
    private void OnBotonHoverExit()
    {
        if (previewActual != null)
        {
            Destroy(previewActual);
            previewActual = null;
        }
    }

    // Aplicar material de previsualización
    private void SetPreviewMaterial(GameObject preview)
    {
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Copiar material y hacerlo semitransparente
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(materialPreview);
                materials[i].color = new Color(1, 1, 1, 0.5f);
            }
            renderer.materials = materials;
        }
    }

    // Método de clic modificado


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