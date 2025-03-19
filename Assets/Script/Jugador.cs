using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Jugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f; // Velocidad de movimiento normal
    public float velocidadDash = 15f; // Velocidad del dash
    public float duracionDash = 0.2f; // Duración del dash
    public float cooldownDash = 1f; // Tiempo de espera entre dashes
    public float velocidadRotacion = 10f; // Velocidad de rotación del jugador

    [Header("Interacción")]
    public float radioInteraccion = 3f; // Radio de detección de objetos interactuables

    private Rigidbody rb; // Referencia al Rigidbody
    private Vector3 direccionMovimiento; // Dirección de movimiento
    private bool puedeDash = true; // Indica si el jugador puede hacer dash
    private bool estaDashing = false; // Indica si el jugador está haciendo dash
    private Transform camara; // Referencia a la cámara
    private bool puedeMoverse = true; // Indica si el jugador puede moverse
    private GameObject canvasConstruccion; // Referencia al canvas de construcción
    private bool canvasActivo = false; // Indica si el canvas está activo

    public Button[] botonesConstruccion; // Array de botones del panel
    public GameObject[] prefabsConstruccion; // Array de prefabs para instanciar

    private Vector3 posicionCasillaSeleccionada; // Posición de la casilla seleccionada

    [Header("Destrucción de Torres")]
    public GameObject canvasDestruccion; // Referencia al canvas de destrucción
    private GameObject torreSeleccionada; // Referencia a la torre seleccionada

    [Header("Previsualización")]
    public Material materialPreview; // Material semitransparente para la previsualización
    private GameObject previewActual; // Objeto de previsualización actual


    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtener el componente Rigidbody
        camara = Camera.main.transform; // Obtener la referencia a la cámara principal

        // Buscar el canvas de construcción por tag
        canvasConstruccion = GameObject.FindGameObjectWithTag("CanvasConstruccion");
        if (canvasConstruccion != null)
        {
            canvasConstruccion.SetActive(false); // Desactivar el canvas al inicio
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con el tag 'CanvasConstruccion'.");
        }

        // Buscar el canvas de destrucción por tag
        canvasDestruccion = GameObject.FindGameObjectWithTag("CanvasDestruccion");
        if (canvasDestruccion != null)
        {
            canvasDestruccion.SetActive(false); // Desactivar el canvas al inicio
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con el tag 'CanvasDestruccion'.");
        }

                // Asignar evento al botón de destrucción
        Button botonDestruir = canvasDestruccion.GetComponentInChildren<Button>();
        if (botonDestruir != null)
        {
            botonDestruir.onClick.AddListener(DestruirTorre);
        }
        else
        {
            Debug.LogWarning("No se encontró un botón en el canvas de destrucción.");
        }

        // Suscribirse al evento de destrucción de la TorreRey
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            GameManager.OnTorreReyDestruida += DesactivarMovimiento;
        }


        // Asignar eventos a los botones
        for (int i = 0; i < botonesConstruccion.Length; i++)
        {
            int index = i; // Capturar el índice para el evento
            botonesConstruccion[i].onClick.AddListener(() => OnBotonConstruccionClic(index));
        }

        foreach (Button boton in botonesConstruccion)
        {
            AddHoverEvents(boton);
        }
    }
    private GameObject objetoInteractuableActual; // Referencia al objeto interactuable actual

    void Update()
    {
        // Solo procesar la entrada si el jugador puede moverse
        if (puedeMoverse)
        {
            // Obtener la entrada del jugador (WASD o flechas)
            float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
            float movimientoVertical = Input.GetAxisRaw("Vertical");

            // Calcular la dirección de movimiento en relación con la cámara
            direccionMovimiento = (camara.forward * movimientoVertical + camara.right * movimientoHorizontal).normalized;
            direccionMovimiento.y = 0; // Ignorar la componente Y para evitar movimientos verticales

            // Verificar si el jugador presiona Shift y puede hacer dash
            if (Input.GetMouseButtonDown(1) && puedeDash)
            {
                StartCoroutine(Dash());
            }

            // Verificar si el jugador hace clic izquierdo
            if (Input.GetMouseButtonUp(0))
            {
                if (!canvasActivo && !canvasDestruccion.activeSelf)
                {
                    VerificarInteraccion(); // Verificar interacción solo si el canvas no está activo
                    VerificarInteraccionTorre();
                }
                //else
                //{
                //    DesactivarCanvas(); // Desactivar el canvas si ya está activo
                //}
            }

            // Verificar si el objeto interactuable actual está fuera del radio de interacción
            if (canvasActivo && objetoInteractuableActual != null)
            {
                float distancia = Vector3.Distance(transform.position, objetoInteractuableActual.transform.position);
                if (distancia > radioInteraccion)
                {
                    DesactivarCanvas(); // Desactivar el canvas si el objeto está fuera del radio
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Mover al jugador solo si no está haciendo dash y puede moverse
        if (!estaDashing && puedeMoverse)
        {
            rb.linearVelocity = direccionMovimiento * velocidadMovimiento;

            // Rotar el jugador hacia la dirección del movimiento
            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }
    }

    // Método para rotar el jugador hacia la dirección del movimiento
    private void RotarHaciaDireccion(Vector3 direccion)
    {
        // Calcular la rotación deseada
        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);

        // Suavizar la rotación hacia la dirección deseada
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
    }

    // Corrutina para realizar el dash
    private IEnumerator Dash()
    {
        puedeDash = false; // Desactivar el dash
        estaDashing = true; // Indicar que el jugador está haciendo dash

        // Aplicar la velocidad del dash en la dirección actual
        rb.linearVelocity = direccionMovimiento * velocidadDash;

        // Esperar la duración del dash
        yield return new WaitForSeconds(duracionDash);

        // Detener el dash y restaurar la velocidad normal
        rb.linearVelocity = Vector3.zero;
        estaDashing = false;

        // Esperar el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(cooldownDash);
        puedeDash = true; // Reactivar el dash
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
        // Lanzar un rayo desde la cámara hacia el punto donde el jugador hizo clic
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verificar si el rayo golpea un objeto
        if (Physics.Raycast(rayo, out hit))
        {
            // Verificar si el objeto golpeado tiene el tag "ObjetoInteractuable"
            if (hit.collider.CompareTag("ObjetoInteractuable"))
            {
                // Verificar si el objeto está dentro del radio de interacción
                float distancia = Vector3.Distance(transform.position, hit.transform.position);
                if (distancia <= radioInteraccion)
                {
                    // Guardar referencia al objeto interactuable actual
                    objetoInteractuableActual = hit.collider.gameObject;

                    // Buscar el objeto hijo "PuntoDeInstancia" en la jerarquía de la casilla
                    Transform puntoDeInstancia = hit.collider.transform.Find("PuntoDeInstancia");
                    if (puntoDeInstancia != null)
                    {
                        // Guardar la posición del objeto hijo
                        posicionCasillaSeleccionada = puntoDeInstancia.position;
                    }
                    else
                    {
                        // Si no se encuentra el objeto hijo, usar la posición de la casilla
                        Debug.LogWarning("No se encontró el objeto hijo 'PuntoDeInstancia' en la casilla.");
                        posicionCasillaSeleccionada = hit.point;
                    }

                    // Activar el canvas de construcción
                    if (canvasConstruccion != null)
                    {
                        canvasConstruccion.SetActive(true);
                        canvasActivo = true; // Marcar el canvas como activo
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

            Destroy(previewActual);
            DesactivarCanvas();
        }
    }

    private void DesactivarCanvas()
    {
        if (canvasConstruccion != null)
        {
            canvasConstruccion.SetActive(false);
            canvasActivo = false; // Marcar el canvas como inactivo
            objetoInteractuableActual = null; // Limpiar la referencia al objeto interactuable
        }
    }

    private void VerificarInteraccionTorre()
    {
        // Lanzar un rayo desde la cámara hacia el punto donde el jugador hizo clic
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verificar si el rayo golpea un objeto
        if (Physics.Raycast(rayo, out hit))
        {
            // Verificar si el objeto golpeado tiene el tag "Torre"
            if (hit.collider.CompareTag("Torreta"))
            {
                // Guardar referencia a la torre seleccionada
                torreSeleccionada = hit.collider.gameObject;

                // Activar el canvas de destrucción
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

    // Cuando el puntero entra en el botón
    private void OnBotonHover(Button botonHover)
    {
        int index = System.Array.IndexOf(botonesConstruccion, botonHover);
        if (index != -1 && index < prefabsConstruccion.Length)
        {
            // Destruir preview anterior
            if (previewActual != null) Destroy(previewActual);

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