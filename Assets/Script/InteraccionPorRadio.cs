using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteraccionPorRadio : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float radioInteraccion = 5f; // Radio de acción del jugador
    [SerializeField] private LayerMask capaInteractuable; // Capa para filtrar objetos
    [SerializeField] private Color colorHighlight = Color.gray; // Color al resaltar

    private Material materialActual;
    private Color colorOriginal;
    private GameObject objetoDetectado;
    private Camera camaraPrincipal;
    private bool cursorSobreObjeto = false;

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (capaInteractuable == 0) capaInteractuable = LayerMask.GetMask("Default");
    }

    void Update()
    {
        VerificarInteraccion();
        ActualizarColor();
    }

    void VerificarInteraccion()
    {
        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool deteccionEsteFrame = false;

        if (Physics.Raycast(rayo, out hit, Mathf.Infinity, capaInteractuable))
        {
            GameObject nuevoObjeto = hit.collider.gameObject;

            if (nuevoObjeto.CompareTag("ObjetoInteractuable"))
            {
                float distancia = Vector3.Distance(transform.position, nuevoObjeto.transform.position);

                if (distancia <= radioInteraccion)
                {
                    if (objetoDetectado != nuevoObjeto)
                    {
                        RestaurarColor();
                        objetoDetectado = nuevoObjeto;
                        Renderer renderer = nuevoObjeto.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            materialActual = renderer.material;
                            colorOriginal = materialActual.color;
                        }
                    }
                    deteccionEsteFrame = true;
                }
            }
        }

        if (!deteccionEsteFrame && cursorSobreObjeto)
        {
            RestaurarColor();
        }
        cursorSobreObjeto = deteccionEsteFrame;
    }

    void ActualizarColor()
    {
        if (cursorSobreObjeto && materialActual != null)
        {
            materialActual.color = colorHighlight;
        }
    }

    void RestaurarColor()
    {
        if (materialActual != null)
        {
            materialActual.color = colorOriginal;
            materialActual = null;
            objetoDetectado = null;
        }
    }

    void OnDisable()
    {
        RestaurarColor();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}