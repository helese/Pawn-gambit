using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    [Header("Oleadas y Torre ")]
    public List<PortalEnemigo> portalesEnemigos; 
    public int oleadaActual = 1; 
    public int activarPortal = 5; 
    public int curarRey = 3; 
    public int oleadasParaJefe = 5; 

    private bool oleadaEnCurso = false; 
    private TorreRey torreRey; 


    [Header("Enemigos y Jefes")]
    public float tiempoMin = 1.0f; 
    public float tiempoMax = 3.0f; 

    private float tiempoEntreEnemigos; 
    private int jefesVivos = 0; 
    private int enemigosGenerados = 0;
    private int enemigosDestruidos = 0; 

    private PortalEnemigo portalConJefe; 

    [Header("Game Over")]
    public GameObject panelGameOver; 
    public MoverCamara moverCamara;
    public float retrasoCanvas = 3f;
    public CambiarTransparenciaImagen cambiarTransparencia1;
    public CambiarTransparenciaImagen cambiarTransparencia2;

    private bool juegoActivo = true;
    public delegate void TorreReyDestruidaHandler();
    public static event TorreReyDestruidaHandler OnTorreReyDestruida;

    void Start()
    {
        torreRey = FindFirstObjectByType<TorreRey>();

        // Activar el primer portal al inicio
        if (portalesEnemigos.Count > 0)
        {
            portalesEnemigos[0].ActivarPortal(true); 
            portalesEnemigos[0].InstanciarCaminoAleatorio(); 
        }

        // Desactivar los dem�s portales al inicio
        for (int i = 1; i < portalesEnemigos.Count; i++)
        {
            portalesEnemigos[i].ActivarPortal(false); 
        }

        // Desactivar el panel de Game Over al inicio
        panelGameOver.SetActive(false);
        juegoActivo = true;
        oleadaEnCurso = false;
    }

    void Update()
    {
        // Iniciar oleada si se puede
        if (Input.GetKeyUp(KeyCode.Space) && !oleadaEnCurso && juegoActivo)
        {
            IniciarOleada();

        }
    }

    // M�todo para iniciar una nueva oleada
    void IniciarOleada()
    {
        // Desactivar la advertencia de jefe en todos los portales al comenzar una nueva oleada
        DesactivarAdvertenciaJefe();

        tiempoEntreEnemigos = GenerarTiempoDivisiblePor02(tiempoMin, tiempoMax);

        // Calcular la cantidad de enemigos para la oleada actual usando la f�rmula
        enemigosGenerados = Mathf.CeilToInt(3 * Mathf.Pow(1.5f, oleadaActual - 1));

        // Reiniciar el contador de enemigos destruidos
        enemigosDestruidos = 0; 

        oleadaEnCurso = true;

        // Verificar si en la siguiente oleada se activar� un nuevo portal
        if ((oleadaActual + 1) % activarPortal == 0 && (oleadaActual + 1) / activarPortal < portalesEnemigos.Count)
        {
            int portalAActivar = (oleadaActual + 1) / activarPortal;

            // Instancia un camino una ronda antes de activar el portal
            portalesEnemigos[portalAActivar].InstanciarCaminoAleatorio();
            Debug.Log($"Camino instanciado para el portal {portalAActivar + 1} en la oleada {oleadaActual}.");
        }

        // Activar un nuevo portal cada X oleadas
        if (oleadaActual % activarPortal == 0 && oleadaActual / activarPortal < portalesEnemigos.Count)
        {
            int portalAActivar = oleadaActual / activarPortal;
            portalesEnemigos[portalAActivar].ActivarPortal(true);
            Debug.Log($"Portal {portalAActivar + 1} activado en la oleada {oleadaActual}.");
        }

        // Spawnear un jefe
        if (oleadaActual % oleadasParaJefe == 0)
        {
            SpawnearJefe();
        }

        // Distribuir la generaci�n de enemigos entre los portales activos
        List<PortalEnemigo> portalesActivos = ObtenerPortalesActivos();
        int enemigosPorPortal = enemigosGenerados / portalesActivos.Count;
        int enemigosRestantes = enemigosGenerados % portalesActivos.Count;

        for (int i = 0; i < portalesActivos.Count; i++)
        {
            int cantidad = enemigosPorPortal;
            if (i < enemigosRestantes)
            {
                cantidad++; // Asignar un enemigo adicional a los primeros portales
            }

            // Iniciar la generaci�n de enemigos en el portal actual
            portalesActivos[i].IniciarInstanciacion(cantidad, tiempoEntreEnemigos);
        }

        Debug.Log($"Iniciando oleada {oleadaActual} con {enemigosGenerados} enemigos y un tiempo entre enemigos de {tiempoEntreEnemigos} segundos.");
    }

    public void NotificarTorreReyDestruida()
    {
        juegoActivo = false;
        oleadaEnCurso = false;

        // Iniciar el movimiento de la c�mara hacia el objetivo
        moverCamara.IniciarMovimiento();
        GetComponent<ControladorVolumen>().CambiarModoGrave();

        // Activar el panel de Game Over despu�s de un retraso
        if (panelGameOver != null)
        {
            Invoke("ActivarPanelGameOver", retrasoCanvas); // Aseg�rate de que el nombre del m�todo sea correcto
        }

        // Notificar a los suscriptores que la TorreRey ha sido destruida
        OnTorreReyDestruida?.Invoke();

        Debug.Log("�Game Over! La TorreRey ha sido destruida.");
    }
    private void ActivarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            cambiarTransparencia1.IniciarTransicion();
            cambiarTransparencia2.IniciarTransicion();
        }
    }

    private void SpawnearJefe()
    {
        if (portalConJefe != null)
        {
            portalConJefe.SpawnearJefe();
            jefesVivos++; // Incrementar el contador de jefes vivos
            Debug.Log($"Jefe spawnedo en el portal {portalesEnemigos.IndexOf(portalConJefe) + 1}. Jefes vivos: {jefesVivos}");
        }
        else
        {
            Debug.LogWarning("No hay un portal seleccionado para spawnear al jefe.");
        }
    }

    private float GenerarTiempoDivisiblePor02(float min, float max)
    {
        int pasosMin = Mathf.CeilToInt(min / 0.2f);
        int pasosMax = Mathf.FloorToInt(max / 0.2f);

        int pasosAleatorios = Random.Range(pasosMin, pasosMax + 1);

        // Calcular el tiempo correspondiente
        return pasosAleatorios * 0.2f;
    }

    // M�todo para obtener los portales activos
    private List<PortalEnemigo> ObtenerPortalesActivos()
    {
        List<PortalEnemigo> portalesActivos = new List<PortalEnemigo>();
        foreach (var portal in portalesEnemigos)
        {
            if (portal.EstaActivo())
            {
                portalesActivos.Add(portal);
            }
        }
        return portalesActivos;
    }

    // M�todo para notificar que un enemigo ha sido destruido
    public void NotificarEnemigoDestruido()
    {
        enemigosDestruidos++; // Incrementar el contador de enemigos destruidos
        Debug.Log($"Enemigos destruidos: {enemigosDestruidos}/{enemigosGenerados}");

        // Verificar si todos los enemigos han sido destruidos y no hay jefes vivos
        if (enemigosDestruidos >= enemigosGenerados && jefesVivos == 0)
        {
            FinalizarOleada();
        }
    }

    // M�todo para notificar que un jefe ha sido destruido
    public void NotificarJefeDestruido()
    {
        jefesVivos--; // Decrementar el contador de jefes vivos
        Debug.Log($"Jefe destruido. Jefes vivos: {jefesVivos}");

        // Verificar si todos los enemigos han sido destruidos y no hay jefes vivos
        if (enemigosDestruidos >= enemigosGenerados && jefesVivos == 0)
        {
            FinalizarOleada();
        }
    }
    // M�todo para seleccionar el portal que va a spawnear al jefe
    private void SeleccionarPortalParaJefe()
    {
        if (portalesEnemigos.Count > 0)
        {
            // Seleccionar un portal activo aleatorio
            List<PortalEnemigo> portalesActivos = ObtenerPortalesActivos();
            if (portalesActivos.Count > 0)
            {
                int portalAleatorio = Random.Range(0, portalesActivos.Count);
                portalConJefe = portalesActivos[portalAleatorio];
                portalConJefe.ActivarAdvertenciaJefe();
                Debug.Log($"Advertencia de jefe activada en el portal {portalesEnemigos.IndexOf(portalConJefe) + 1}.");
            }
            else
            {
                Debug.LogWarning("No hay portales activos para activar la advertencia de jefe.");
            }
        }
        else
        {
            Debug.LogWarning("No hay portales configurados en el GameManager.");
        }
    }

    // M�todo para desactivar la advertencia de jefe en todos los portales
    private void DesactivarAdvertenciaJefe()
    {
        foreach (var portal in portalesEnemigos)
        {
            portal.DesactivarAdvertenciaJefe();
        }
    }


    // M�todo para finalizar la oleada
    private void FinalizarOleada()
    {
        oleadaEnCurso = false; // Marcar que la oleada ha terminado

        // Verificar si es el momento de recuperar la vida de la TorreRey
        if (oleadaActual % curarRey == 0)
        {
            RecuperarVidaTorreRey();
        }

        // Activar la advertencia de jefe en el portal que spawnear� el jefe en la siguiente oleada
        if ((oleadaActual + 1) % oleadasParaJefe == 0)
        {
            SeleccionarPortalParaJefe();
        }

        oleadaActual++; // Pasar a la siguiente oleada
        Debug.Log($"Oleada {oleadaActual - 1} completada. Prepar�ndose para la oleada {oleadaActual}.");
    }

    // M�todo para recuperar la vida de la TorreRey
    private void RecuperarVidaTorreRey()
    {
        if (torreRey != null)
        {
            torreRey.RecuperarVidaCompleta();
            Debug.Log("�La TorreRey ha recuperado toda su vida!");
        }
    }
}