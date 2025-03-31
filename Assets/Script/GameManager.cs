using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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

    public bool juegoActivo = true;
    public delegate void TorreReyDestruidaHandler();
    public static event TorreReyDestruidaHandler OnTorreReyDestruida;

    [Header("Sistema de Economia")]
    public Slider sliderOleada; // Slider que se llenará durante la oleada
    public TMP_Text textoContador; // Texto que muestra el valor actual
    public float tiempoLlenadoSlider = 10f; // Tiempo en segundos para llenar el slider
    public int valorMaximoPorOleada = 5; // Valor máximo que puede alcanzar el contador por oleada
    private int incrementosRestantes;
    private int valorRestante;

    public int valorInicial = 9; // Valor inicial del contador al comenzar la oleada

    public int valorActual = 0; // Valor actual del contador
    private bool llenandoSlider = false; // Indica si el slider se está llenando
    private float tiempoInicioLlenado; // Tiempo en que comenzó a llenarse el slider

    public delegate void OleadaFinalizadaHandler();
    public event OleadaFinalizadaHandler OnOleadaFinalizada;

    public delegate void OleadaEventHandler();
    public event OleadaEventHandler OnOleadaIniciada;

    void Start()
    {
        torreRey = FindFirstObjectByType<TorreRey>();

        // Activar el primer portal al inicio
        if (portalesEnemigos.Count > 0)
        {
            portalesEnemigos[0].ActivarPortal(true);
            portalesEnemigos[0].InstanciarCaminoAleatorio();
        }

        // Desactivar los demás portales al inicio
        for (int i = 1; i < portalesEnemigos.Count; i++)
        {
            portalesEnemigos[i].ActivarPortal(false);
        }

        // Desactivar el panel de Game Over al inicio
        panelGameOver.SetActive(false);
        juegoActivo = true;
        oleadaEnCurso = false;

        valorActual = valorInicial;
        textoContador.text = valorActual.ToString();
        sliderOleada.value = 0f; // Reiniciar el slider visualmente
    }

    void Update()
    {
        // Iniciar oleada si se puede
        if (Input.GetKeyUp(KeyCode.Space) && !oleadaEnCurso && juegoActivo)
        {
            IniciarOleada();
        }

        // Llenar el slider si está en proceso
        if (llenandoSlider && oleadaEnCurso && incrementosRestantes < valorMaximoPorOleada)
        {
            float tiempoTranscurrido = Time.time - tiempoInicioLlenado;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / tiempoLlenadoSlider);

            // Actualizar el valor del slider
            sliderOleada.value = progreso;

            // Si el slider llega al máximo, incrementar el contador
            if (progreso >= 1f)
            {
                IncrementarContador();
                tiempoInicioLlenado = Time.time; // Reiniciar el tiempo de llenado
            }
        }
    }

    // Método para iniciar una nueva oleada
    void IniciarOleada()
    {
        OnOleadaIniciada?.Invoke();
        // Desactivar la advertencia de jefe en todos los portales al comenzar una nueva oleada
        DesactivarAdvertenciaJefe();

        tiempoEntreEnemigos = GenerarTiempoDivisiblePor02(tiempoMin, tiempoMax);

        // Calcular la cantidad de enemigos para la oleada actual usando la fórmula
        enemigosGenerados = Mathf.CeilToInt(2 * Mathf.Pow(1.3f, oleadaActual - 1)) + 4;

        // Reiniciar el contador de enemigos destruidos
        enemigosDestruidos = 0;

        oleadaEnCurso = true;

        // Verificar si en la siguiente oleada se activará un nuevo portal
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

        // Distribuir la generación de enemigos entre los portales activos
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

            // Iniciar la generación de enemigos en el portal actual
            portalesActivos[i].IniciarInstanciacion(cantidad, tiempoEntreEnemigos);
        }

        Debug.Log($"Iniciando oleada {oleadaActual} con {enemigosGenerados} enemigos y un tiempo entre enemigos de {tiempoEntreEnemigos} segundos.");

        sliderOleada.value = 0f;

        // Iniciar el llenado del slider
        llenandoSlider = true;
        tiempoInicioLlenado = Time.time;
    }

    // Método para incrementar el contador
    private void IncrementarContador()
    {
        if (incrementosRestantes < valorMaximoPorOleada)
        {
            valorActual++;
            textoContador.text = valorActual.ToString();
            incrementosRestantes += 1;
        }

    }

    public void RestarUnidades(int cantidad)
    {
        valorActual = valorActual - cantidad;
        textoContador.text = valorActual.ToString();
    }   

    public void SumarUnidades(int cantidad)
    {
        valorActual = valorActual + cantidad;
        textoContador.text = valorActual.ToString();
    }

    public int ObtenerValorSlider()
    {
        return valorActual; // Devuelve el valor actual del contador
    }

    public void NotificarTorreReyDestruida()
    {
        juegoActivo = false;
        oleadaEnCurso = false;

        // Iniciar el movimiento de la cámara hacia el objetivo
        moverCamara.IniciarMovimiento();
        GetComponent<ControladorVolumen>().CambiarModoGrave();

        // Activar el panel de Game Over después de un retraso
        if (panelGameOver != null)
        {
            Invoke("ActivarPanelGameOver", retrasoCanvas); // Asegúrate de que el nombre del método sea correcto
        }

        // Notificar a los suscriptores que la TorreRey ha sido destruida
        OnTorreReyDestruida?.Invoke();

        Debug.Log("¡Game Over! La TorreRey ha sido destruida.");
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

    // Método para obtener los portales activos
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

    // Método para notificar que un enemigo ha sido destruido
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

    // Método para notificar que un jefe ha sido destruido
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
    // Método para seleccionar el portal que va a spawnear al jefe
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

    // Método para desactivar la advertencia de jefe en todos los portales
    private void DesactivarAdvertenciaJefe()
    {
        foreach (var portal in portalesEnemigos)
        {
            portal.DesactivarAdvertenciaJefe();
        }
    }


    // Método para finalizar la oleada
    private void FinalizarOleada()
    {
        oleadaEnCurso = false; // Marcar que la oleada ha terminado

        if (incrementosRestantes < valorMaximoPorOleada)
        {
            valorRestante = valorMaximoPorOleada - incrementosRestantes;
            valorActual += valorRestante;
            textoContador.text = valorActual.ToString();
            valorRestante = 0;
            sliderOleada.value = 0;

        }

        incrementosRestantes = 0;

        // Verificar si es el momento de recuperar la vida de la TorreRey
        if (oleadaActual % curarRey == 0)
        {
            RecuperarVidaTorreRey();
        }

        // Activar la advertencia de jefe en el portal que spawneará el jefe en la siguiente oleada
        if ((oleadaActual + 1) % oleadasParaJefe == 0)
        {
            SeleccionarPortalParaJefe();
        }

        oleadaActual++; // Pasar a la siguiente oleada
        Debug.Log($"Oleada {oleadaActual - 1} completada. Preparándose para la oleada {oleadaActual}.");
        OnOleadaFinalizada?.Invoke();

    }

    // Método para recuperar la vida de la TorreRey
    private void RecuperarVidaTorreRey()
    {
        if (torreRey != null)
        {
            torreRey.RecuperarVidaCompleta();
            Debug.Log("¡La TorreRey ha recuperado toda su vida!");
        }
    }
}