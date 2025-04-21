using UnityEngine;

public class DetectorCasillas : MonoBehaviour
{
    [Header("Referencias de Colliders")]
    public GameObject colliderNorte;
    public GameObject colliderSur;
    public GameObject colliderEste;
    public GameObject colliderOeste;

    [Header("Referencias de Colliders de Esquinas")]
    public GameObject esquinaNorteEste;
    public GameObject esquinaNorteOeste;
    public GameObject esquinaSurEste;
    public GameObject esquinaSurOeste;

    [Header("Configuración")]
    public LayerMask casillaAjedrezLayer;

    [Header("Referencias de Materiales")]
    public Material textura1;  // 0 colisiones
    public Material textura2;  // 1 colisión en collider Norte
    public Material textura3;  // 1 colisión en collider Este
    public Material textura4;  // 1 colisión en collider Sur
    public Material textura5;  // 1 colisión en collider Oeste
    public Material textura6;  // 2 colisiones en collider Norte y Este
    public Material textura7;  // 2 colisiones en collider Norte y Oeste
    public Material textura8;  // 2 colisiones en collider Sur y Este
    public Material textura9;  // 2 colisiones en collider Sur y Oeste
    public Material textura10; // 3 colisiones en collider Norte, Este y Oeste
    public Material textura11; // 3 colisiones en collider Sur, Este y Oeste
    public Material textura12; // 4 colisiones en todos los colliders
    public Material textura13; // 3 colisiones en Norte, Este y Sur
    public Material textura14; // 3 colisiones en Norte, Oeste y Sur
    public Material textura15; // 2 colisiones en Norte y Sur
    public Material textura16; // 2 colisiones en Este y Oeste

    [Header("Referencias de Materiales para Esquinas")]
    public Material texturaEsquinaNE; // Colisión en esquina Norte-Este
    public Material texturaEsquinaNO; // Colisión en esquina Norte-Oeste
    public Material texturaEsquinaSE; // Colisión en esquina Sur-Este
    public Material texturaEsquinaSO; // Colisión en esquina Sur-Oeste

    // Variables para rastrear colisiones
    private bool colisionNorte = false;
    private bool colisionSur = false;
    private bool colisionEste = false;
    private bool colisionOeste = false;

    // Variables para rastrear colisiones en esquinas
    private bool colisionEsquinaNE = false;
    private bool colisionEsquinaNO = false;
    private bool colisionEsquinaSE = false;
    private bool colisionEsquinaSO = false;

    private Renderer rend;

    void Start()
    {
        // Obtener el Renderer del cubo
        rend = GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError("No se encontró el componente Renderer en el cubo");
            return;
        }

        // Configurar los scripts de detección en los colliders hijos
        ConfigurarDetector(colliderNorte, "Norte");
        ConfigurarDetector(colliderSur, "Sur");
        ConfigurarDetector(colliderEste, "Este");
        ConfigurarDetector(colliderOeste, "Oeste");

        // Configurar los scripts de detección en las esquinas
        ConfigurarDetector(esquinaNorteEste, "EsquinaNE");
        ConfigurarDetector(esquinaNorteOeste, "EsquinaNO");
        ConfigurarDetector(esquinaSurEste, "EsquinaSE");
        ConfigurarDetector(esquinaSurOeste, "EsquinaSO");

        // Asignar textura inicial
        ActualizarMaterial();
    }

    private void ConfigurarDetector(GameObject colliderObj, string direccion)
    {
        if (colliderObj != null)
        {
            DetectorColision detector = colliderObj.GetComponent<DetectorColision>();
            if (detector == null)
            {
                detector = colliderObj.AddComponent<DetectorColision>();
            }
            detector.padre = this;
            detector.direccion = direccion;

            // Asegurarse de que tiene un collider y está configurado como trigger
            BoxCollider boxCollider = colliderObj.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }
            else
            {
                Debug.LogError("El objeto " + direccion + " no tiene un BoxCollider");
            }
        }
        else
        {
            Debug.LogError("Falta asignar el collider " + direccion);
        }
    }

    // Métodos para registrar colisiones desde los detectores
    public void RegistrarColision(string direccion, bool estado)
    {
        Debug.Log("Registrando colisión en " + direccion + ": " + estado);

        switch (direccion)
        {
            case "Norte":
                colisionNorte = estado;
                break;
            case "Sur":
                colisionSur = estado;
                break;
            case "Este":
                colisionEste = estado;
                break;
            case "Oeste":
                colisionOeste = estado;
                break;
            case "EsquinaNE":
                colisionEsquinaNE = estado;
                break;
            case "EsquinaNO":
                colisionEsquinaNO = estado;
                break;
            case "EsquinaSE":
                colisionEsquinaSE = estado;
                break;
            case "EsquinaSO":
                colisionEsquinaSO = estado;
                break;
        }

        ActualizarMaterial();
    }

    private void ActualizarMaterial()
    {
        // Verificar estado actual de todas las colisiones
        Debug.Log("Estado colisiones - Norte: " + colisionNorte +
                 ", Sur: " + colisionSur +
                 ", Este: " + colisionEste +
                 ", Oeste: " + colisionOeste +
                 ", EsquinaNE: " + colisionEsquinaNE +
                 ", EsquinaNO: " + colisionEsquinaNO +
                 ", EsquinaSE: " + colisionEsquinaSE +
                 ", EsquinaSO: " + colisionEsquinaSO);

        // Contar colisiones principales (sin esquinas)
        int contadorColisionesPrincipales = 0;
        if (colisionNorte) contadorColisionesPrincipales++;
        if (colisionSur) contadorColisionesPrincipales++;
        if (colisionEste) contadorColisionesPrincipales++;
        if (colisionOeste) contadorColisionesPrincipales++;

        // Verificar si solo hay una esquina colisionando sin otras colisiones principales
        if (contadorColisionesPrincipales == 0)
        {
            // Si solo hay colisión en una esquina
            if (colisionEsquinaNE && !colisionEsquinaNO && !colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = texturaEsquinaNE;
                Debug.Log("Aplicando texturaEsquinaNE");
                return;
            }
            else if (!colisionEsquinaNE && colisionEsquinaNO && !colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = texturaEsquinaNO;
                Debug.Log("Aplicando texturaEsquinaNO");
                return;
            }
            else if (!colisionEsquinaNE && !colisionEsquinaNO && colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = texturaEsquinaSE;
                Debug.Log("Aplicando texturaEsquinaSE");
                return;
            }
            else if (!colisionEsquinaNE && !colisionEsquinaNO && !colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = texturaEsquinaSO;
                Debug.Log("Aplicando texturaEsquinaSO");
                return;
            }
        }

        // Si no hay solamente una esquina colisionando, continuamos con la lógica original
        Debug.Log("Total colisiones principales: " + contadorColisionesPrincipales);

        // Aplicar material según la combinación de colisiones
        switch (contadorColisionesPrincipales)
        {
            case 0:
                rend.material = textura1;
                Debug.Log("Aplicando textura1");
                break;
            case 1:
                if (colisionNorte)
                {
                    rend.material = textura2;
                    Debug.Log("Aplicando textura2 (Norte)");
                }
                else if (colisionEste)
                {
                    rend.material = textura3;
                    Debug.Log("Aplicando textura3 (Este)");
                }
                else if (colisionSur)
                {
                    rend.material = textura4;
                    Debug.Log("Aplicando textura4 (Sur)");
                }
                else if (colisionOeste)
                {
                    rend.material = textura5;
                    Debug.Log("Aplicando textura5 (Oeste)");
                }
                break;
            case 2:
                if (colisionNorte && colisionEste)
                {
                    rend.material = textura6;
                    Debug.Log("Aplicando textura6 (Norte-Este)");
                }
                else if (colisionNorte && colisionOeste)
                {
                    rend.material = textura7;
                    Debug.Log("Aplicando textura7 (Norte-Oeste)");
                }
                else if (colisionSur && colisionEste)
                {
                    rend.material = textura8;
                    Debug.Log("Aplicando textura8 (Sur-Este)");
                }
                else if (colisionSur && colisionOeste)
                {
                    rend.material = textura9;
                    Debug.Log("Aplicando textura9 (Sur-Oeste)");
                }
                else if (colisionNorte && colisionSur)
                {
                    rend.material = textura15;
                    Debug.Log("Aplicando textura15 (Norte-Sur)");
                }
                else if (colisionEste && colisionOeste)
                {
                    rend.material = textura16;
                    Debug.Log("Aplicando textura16 (Este-Oeste)");
                }
                else
                {
                    rend.material = textura1;
                    Debug.Log("Aplicando textura1 (combinación no especificada)");
                }
                break;
            case 3:
                if (colisionNorte && colisionEste && colisionOeste)
                {
                    rend.material = textura10;
                    Debug.Log("Aplicando textura10 (Norte-Este-Oeste)");
                }
                else if (colisionSur && colisionEste && colisionOeste)
                {
                    rend.material = textura11;
                    Debug.Log("Aplicando textura11 (Sur-Este-Oeste)");
                }
                else if (colisionNorte && colisionEste && colisionSur)
                {
                    rend.material = textura13;
                    Debug.Log("Aplicando textura13 (Norte-Este-Sur)");
                }
                else if (colisionNorte && colisionOeste && colisionSur)
                {
                    rend.material = textura14;
                    Debug.Log("Aplicando textura14 (Norte-Oeste-Sur)");
                }
                else
                {
                    rend.material = textura1;
                    Debug.Log("Aplicando textura1 (combinación no especificada)");
                }
                break;
            case 4:
                rend.material = textura12;
                Debug.Log("Aplicando textura12 (todas)");
                break;
        }
    }
}