using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Jugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f; // Velocidad de movimiento normal
    public float velocidadDash = 15f; // Velocidad del dash
    public float duracionDash = 0.2f; // Duraci�n del dash
    public float cooldownDash = 1f; // Tiempo de espera entre dashes
    public float velocidadRotacion = 10f; // Velocidad de rotaci�n del jugador

    [Header("Interacci�n")]
    public float radioInteraccion = 3f; // Radio de detecci�n de objetos interactuables

    private Rigidbody rb; // Referencia al Rigidbody
    private Vector3 direccionMovimiento; // Direcci�n de movimiento
    private bool puedeDash = true; // Indica si el jugador puede hacer dash
    private bool estaDashing = false; // Indica si el jugador est� haciendo dash
    private Transform camara; // Referencia a la c�mara
    private bool puedeMoverse = true; // Indica si el jugador puede moverse
    private GameObject canvasConstruccion; // Referencia al canvas de construcci�n
    private bool canvasActivo = false; // Indica si el canvas est� activo

    public Button[] botonesConstruccion; // Array de botones del panel
    public GameObject[] prefabsConstruccion; // Array de prefabs para instanciar

    private Vector3 posicionCasillaSeleccionada; // Posici�n de la casilla seleccionada

    [Header("Destrucci�n de Torres")]
    public GameObject canvasDestruccion; // Referencia al canvas de destrucci�n
    private GameObject torreSeleccionada; // Referencia a la torre seleccionada

    [Header("Previsualizaci�n")]
    public Material materialPreview; // Material semitransparente para la previsualizaci�n
    private GameObject previewActual; // Objeto de previsualizaci�n actual


    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtener el componente Rigidbody
        camara = Camera.main.transform; // Obtener la referencia a la c�mara principal

        // Buscar el canvas de construcci�n por tag
        canvasConstruccion = GameObject.FindGameObjectWithTag("CanvasConstruccion");
        if (canvasConstruccion != null)
        {
            canvasConstruccion.SetActive(false); // Desactivar el canvas al inicio
        }
        else
        {
            Debug.LogWarning("No se encontr� un objeto con el tag 'CanvasConstruccion'.");
        }

        // Buscar el canvas de destrucci�n por tag
        canvasDestruccion = GameObject.FindGameObjectWithTag("CanvasDestruccion");
        if (canvasDestruccion != null)
        {
            canvasDestruccion.SetActive(false); // Desactivar el canvas al inicio
        }
        else
        {
            Debug.LogWarning("No se encontr� un objeto con el tag 'CanvasDestruccion'.");
        }

                // Asignar evento al bot�n de destrucci�n
        Button botonDestruir = canvasDestruccion.GetComponentInChildren<Button>();
        if (botonDestruir != null)
        {
            botonDestruir.onClick.AddListener(DestruirTorre);
        }
        else
        {
            Debug.LogWarning("No se encontr� un bot�n en el canvas de destrucci�n.");
        }

        // Suscribirse al evento de destrucci�n de la TorreRey
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            GameManager.OnTorreReyDestruida += DesactivarMovimiento;
        }


        // Asignar eventos a los botones
        for (int i = 0; i < botonesConstruccion.Length; i++)
        {
            int index = i; // Capturar el �ndice para el evento
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

            // Calcular la direcci�n de movimiento en relaci�n con la c�mara
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
                    VerificarInteraccion(); // Verificar interacci�n solo si el canvas no est� activo
                    VerificarInteraccionTorre();
                }
                //else
                //{
                //    DesactivarCanvas(); // Desactivar el canvas si ya est� activo
                //}
            }

            // Verificar si el objeto interactuable actual est� fuera del radio de interacci�n
            if (canvasActivo && objetoInteractuableActual != null)
            {
                float distancia = Vector3.Distance(transform.position, objetoInteractuableActual.transform.position);
                if (distancia > radioInteraccion)
                {
                    DesactivarCanvas(); // Desactivar el canvas si el objeto est� fuera del radio
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Mover al jugador solo si no est� haciendo dash y puede moverse
        if (!estaDashing && puedeMoverse)
        {
            rb.linearVelocity = direccionMovimiento * velocidadMovimiento;

            // Rotar el jugador hacia la direcci�n del movimiento
            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }
    }

    // M�todo para rotar el jugador hacia la direcci�n del movimiento
    private void RotarHaciaDireccion(Vector3 direccion)
    {
        // Calcular la rotaci�n deseada
        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);

        // Suavizar la rotaci�n hacia la direcci�n deseada
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
    }

    // Corrutina para realizar el dash
    private IEnumerator Dash()
    {
        puedeDash = false; // Desactivar el dash
        estaDashing = true; // Indicar que el jugador est� haciendo dash

        // Aplicar la velocidad del dash en la direcci�n actual
        rb.linearVelocity = direccionMovimiento * velocidadDash;

        // Esperar la duraci�n del dash
        yield return new WaitForSeconds(duracionDash);

        // Detener el dash y restaurar la velocidad normal
        rb.linearVelocity = Vector3.zero;
        estaDashing = false;

        // Esperar el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(cooldownDash);
        puedeDash = true; // Reactivar el dash
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
        // Lanzar un rayo desde la c�mara hacia el punto donde el jugador hizo clic
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verificar si el rayo golpea un objeto
        if (Physics.Raycast(rayo, out hit))
        {
            // Verificar si el objeto golpeado tiene el tag "ObjetoInteractuable"
            if (hit.collider.CompareTag("ObjetoInteractuable"))
            {
                // Verificar si el objeto est� dentro del radio de interacci�n
                float distancia = Vector3.Distance(transform.position, hit.transform.position);
                if (distancia <= radioInteraccion)
                {
                    // Guardar referencia al objeto interactuable actual
                    objetoInteractuableActual = hit.collider.gameObject;

                    // Buscar el objeto hijo "PuntoDeInstancia" en la jerarqu�a de la casilla
                    Transform puntoDeInstancia = hit.collider.transform.Find("PuntoDeInstancia");
                    if (puntoDeInstancia != null)
                    {
                        // Guardar la posici�n del objeto hijo
                        posicionCasillaSeleccionada = puntoDeInstancia.position;
                    }
                    else
                    {
                        // Si no se encuentra el objeto hijo, usar la posici�n de la casilla
                        Debug.LogWarning("No se encontr� el objeto hijo 'PuntoDeInstancia' en la casilla.");
                        posicionCasillaSeleccionada = hit.point;
                    }

                    // Activar el canvas de construcci�n
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
            // Instanciar versi�n final
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
        // Lanzar un rayo desde la c�mara hacia el punto donde el jugador hizo clic
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

                // Activar el canvas de destrucci�n
                if (canvasDestruccion != null)
                {
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
            Destroy(torreSeleccionada); // Destruir la torre
            torreSeleccionada = null; // Limpiar la referencia
        }

        // Ocultar el canvas de destrucci�n
        if (canvasDestruccion != null)
        {
            canvasDestruccion.SetActive(false);
        }
    }

    // Dibujar gizmo para visualizar el radio de interacci�n
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

    // Cuando el puntero entra en el bot�n
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

    // Cuando el puntero sale del bot�n
    private void OnBotonHoverExit()
    {
        if (previewActual != null)
        {
            Destroy(previewActual);
            previewActual = null;
        }
    }

    // Aplicar material de previsualizaci�n
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

    // M�todo de clic modificado


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