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

    [Header("Nuevas Texturas Combinadas")]
    public Material textura17; // Norte + Sur-Este
    public Material textura18; // Norte + Sur-Oeste
    public Material textura19; // Este + Norte-Oeste
    public Material textura20; // Este + Sur-Oeste
    public Material textura21; // Sur + Norte-Oeste
    public Material textura22; // Sur + Norte-Este
    public Material textura23; // Oeste + Norte-Este 
    public Material textura24; // Oeste + Sur-Este
    public Material textura25; // Norte + SE + SO
    public Material textura26; // Este + NO + SO
    public Material textura27; // Oeste + NE + SE
    public Material textura28; // Sur + NE + NO

    [Header("Texturas 2 esquinas")]
    public Material este2;
    public Material norte2;
    public Material oeste2;
    public Material sur2;
    public Material esquinasDerIzq;
    public Material esquinasIzqDer;

    [Header("Texturas 3 esquinas")]
    public Material sinNorteOeste3;
    public Material sinSurEste3;
    public Material sinSurOeste3;
    public Material SinNorteEste3;
    public Material esquinas4;

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
                return;
            }
            else if (!colisionEsquinaNE && colisionEsquinaNO && !colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = texturaEsquinaNO;
                return;
            }
            else if (!colisionEsquinaNE && !colisionEsquinaNO && colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = texturaEsquinaSE;
                return;
            }
            else if (!colisionEsquinaNE && !colisionEsquinaNO && !colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = texturaEsquinaSO;
                return;
            }


            else if (colisionEsquinaNE && colisionEsquinaNO && !colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = norte2;
                return;
            }
            else if (colisionEsquinaNE && !colisionEsquinaNO && colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = este2;
                return;
            }
            else if (!colisionEsquinaNE && colisionEsquinaNO && !colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = oeste2;
                return;
            }
            else if (!colisionEsquinaNE && !colisionEsquinaNO && colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = sur2;
                return;
            }


            else if (colisionEsquinaNE && !colisionEsquinaNO && !colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = esquinasDerIzq;
                return;
            }
            else if (!colisionEsquinaNE && colisionEsquinaNO && colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = esquinasIzqDer;
                return;
            }


            else if (colisionEsquinaNE && !colisionEsquinaNO && colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = sinNorteOeste3;
                return;
            }
            else if (colisionEsquinaNE && colisionEsquinaNO && !colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = sinSurEste3;
                return;
            }
            else if (colisionEsquinaNE && colisionEsquinaNO && colisionEsquinaSE && !colisionEsquinaSO)
            {
                rend.material = sinSurOeste3;
                return;
            }
            else if (!colisionEsquinaNE && colisionEsquinaNO && colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = SinNorteEste3;
                return;
            }

            else if (colisionEsquinaNE && colisionEsquinaNO && colisionEsquinaSE && colisionEsquinaSO)
            {
                rend.material = esquinas4;
                return;
            }
        }
        if (colisionNorte && colisionEsquinaSE && !colisionEsquinaSO && !colisionSur && !colisionEste && !colisionOeste)
        {
            rend.material = textura17;
            return;
        }
        if (colisionNorte && colisionEsquinaSO && !colisionEsquinaSE && !colisionSur && !colisionEste && !colisionOeste)
        {
            rend.material = textura18;
            return;
        }
        if (colisionEste && colisionEsquinaNO && !colisionEsquinaSO && !colisionNorte && !colisionSur && !colisionOeste)
        {
            rend.material = textura19;
            return;
        }
        if (colisionEste && colisionEsquinaSO && !colisionEsquinaNO && !colisionNorte && !colisionSur && !colisionOeste)
        {
            rend.material = textura20;
            return;
        }
        if (colisionSur && colisionEsquinaNO && !colisionEsquinaNE && !colisionNorte && !colisionEste && !colisionOeste)
        {
            rend.material = textura21;
            return;
        }
        if (colisionSur && colisionEsquinaNE && !colisionEsquinaNO && !colisionNorte && !colisionEste && !colisionOeste)
        {
            rend.material = textura22;
            return;
        }
        if (colisionOeste && colisionEsquinaNE && !colisionEsquinaSE && !colisionNorte && !colisionSur && !colisionEste)
        {
            rend.material = textura23;
            return;
        }
        if (colisionOeste && colisionEsquinaSE && !colisionEsquinaNE && !colisionNorte && !colisionSur && !colisionEste)
        {
            rend.material = textura24;
            return;
        }

        // Casos con 1 colisión principal + 2 esquinas
        if (colisionNorte && colisionEsquinaSE && colisionEsquinaSO && !colisionSur && !colisionEste && !colisionOeste)
        {
            rend.material = textura25;
            return;
        }
        if (colisionEste && colisionEsquinaNO && colisionEsquinaSO && !colisionNorte && !colisionSur && !colisionOeste)
        {
            rend.material = textura26;
            return;
        }
        if (colisionOeste && colisionEsquinaNE && colisionEsquinaSE && !colisionNorte && !colisionSur && !colisionEste)
        {
            rend.material = textura27;
            return;
        }
        if (colisionSur && colisionEsquinaNE && colisionEsquinaNO && !colisionNorte && !colisionEste && !colisionOeste)
        {
            rend.material = textura28;
            return;
        }

        // Aplicar material según la combinación de colisiones
        switch (contadorColisionesPrincipales)
        {
            case 0:
                rend.material = textura1;
  
                break;
            case 1:
                if (colisionNorte)
                {
                    rend.material = textura2;
                }
                else if (colisionEste)
                {
                    rend.material = textura3;
                }
                else if (colisionSur)
                {
                    rend.material = textura4;
                }
                else if (colisionOeste)
                {
                    rend.material = textura5;
                }
                break;
            case 2:
                if (colisionNorte && colisionEste)
                {
                    rend.material = textura6;
                }
                else if (colisionNorte && colisionOeste)
                {
                    rend.material = textura7;
                }
                else if (colisionSur && colisionEste)
                {
                    rend.material = textura8;
                }
                else if (colisionSur && colisionOeste)
                {
                    rend.material = textura9;
                }
                else if (colisionNorte && colisionSur)
                {
                    rend.material = textura15;
                }
                else if (colisionEste && colisionOeste)
                {
                    rend.material = textura16;
                }
                else
                {
                    rend.material = textura1;
                }
                break;
            case 3:
                if (colisionNorte && colisionEste && colisionOeste)
                {
                    rend.material = textura10;
                }
                else if (colisionSur && colisionEste && colisionOeste)
                {
                    rend.material = textura11;
                }
                else if (colisionNorte && colisionEste && colisionSur)
                {
                    rend.material = textura13;
                }
                else if (colisionNorte && colisionOeste && colisionSur)
                {
                    rend.material = textura14;
                }
                else
                {
                    rend.material = textura1;
                }
                break;
            case 4:
                rend.material = textura12;
                break;
        }
    }
}