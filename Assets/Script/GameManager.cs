﻿using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Oleadas y Torre ")]
    public List<PortalEnemigo> portalesEnemigos;
    public int oleadaActual = 1;
    public int activarPortal = 4;
    public int curarRey = 3;
    public int oleadasParaJefe = 6;

    // Lista para llevar registro de qué portales ya han sido activados
    private List<int> indicesPortalesActivados = new List<int>();

    private bool oleadaEnCurso = false;
    private TorreRey torreRey;

    [Header("Enemigos y Jefes")]
    public float tiempoMin = 1.0f;
    public float tiempoMax = 3.0f;

    private float tiempoEntreEnemigos;
    private int jefesVivos = 0;
    private int enemigosGenerados = 0;
    private int enemigosDestruidos = 0;
    private int enemigosTotalesOleada;

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

    private bool verificandoEnemigos = false;
    private Coroutine verificacionCoroutine;

    // Variable para almacenar el próximo portal a activar
    private int proximoPortalAleatorio = -1;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        torreRey = FindFirstObjectByType<TorreRey>();

        // Activar el primer portal al inicio (ahora aleatorio)
        if (portalesEnemigos.Count > 0)
        {
            // Elegir un portal aleatorio para empezar
            int primerPortalIndex = Random.Range(0, portalesEnemigos.Count);
            portalesEnemigos[primerPortalIndex].ActivarPortal(true);
            portalesEnemigos[primerPortalIndex].InstanciarCaminoAleatorio();

            // Registrar el portal activado
            indicesPortalesActivados.Add(primerPortalIndex);

            Debug.Log($"Portal inicial activado aleatoriamente: {primerPortalIndex + 1}");
        }

        // Desactivar todos los demás portales al inicio
        for (int i = 0; i < portalesEnemigos.Count; i++)
        {
            if (!indicesPortalesActivados.Contains(i))
            {
                portalesEnemigos[i].ActivarPortal(false);
            }
        }

        // Seleccionar el próximo portal aleatorio que se activará
        SeleccionarProximoPortalAleatorio();

        // Desactivar el panel de Game Over al inicio
        panelGameOver.SetActive(false);
        juegoActivo = true;
        oleadaEnCurso = false;

        valorActual = valorInicial;
        textoContador.text = valorActual.ToString();
        sliderOleada.value = 0f; // Reiniciar el slider visualmente
    }

    // Método para seleccionar el próximo portal aleatorio a activar
    private void SeleccionarProximoPortalAleatorio()
    {
        // Solo seleccionamos de los portales que aún no han sido activados
        List<int> portalesDisponibles = new List<int>();

        for (int i = 0; i < portalesEnemigos.Count; i++)
        {
            if (!indicesPortalesActivados.Contains(i))
            {
                portalesDisponibles.Add(i);
            }
        }

        // Si aún hay portales disponibles, seleccionar uno al azar
        if (portalesDisponibles.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, portalesDisponibles.Count);
            proximoPortalAleatorio = portalesDisponibles[indiceAleatorio];

            Debug.Log($"Próximo portal seleccionado aleatoriamente: {proximoPortalAleatorio + 1}");
        }
        else
        {
            proximoPortalAleatorio = -1;
            Debug.Log("Todos los portales ya han sido activados.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            valorActual = 99;
            textoContador.text = valorActual.ToString();
        }

        // Iniciar oleada si se puede
        if (Input.GetKeyUp(KeyCode.Space) && !oleadaEnCurso && juegoActivo && !HayEnemigosEnEscena())
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

        // Verificación periódica para casos extremos cuando estamos en una oleada
        if (oleadaEnCurso && !verificandoEnemigos &&
            enemigosDestruidos >= enemigosTotalesOleada && jefesVivos == 0)
        {
            ForzarVerificacionEnemigos();
        }
    }

    private bool HayEnemigosEnEscena()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        return enemigos.Length > 0;
    }

    private IEnumerator VerificarEnemigosPeriodicamente()
    {
        verificandoEnemigos = true;

        // Primera verificación inmediata
        GameObject[] enemigosInicial = GameObject.FindGameObjectsWithTag("Enemigo");
        if (enemigosInicial.Length == 0 && oleadaEnCurso)
        {
            FinalizarOleada();
            verificandoEnemigos = false;
            yield break;
        }

        while (juegoActivo && oleadaEnCurso)
        {
            // Contar enemigos explícitamente
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
            int cantidadEnemigos = enemigos.Length;


            if (cantidadEnemigos == 0)
            {
                FinalizarOleada();
                verificandoEnemigos = false;
                yield break;
            }

            yield return new WaitForSeconds(2f);
        }

        verificandoEnemigos = false;
    }

    public void ForzarVerificacionEnemigos()
    {
        // Detener verificación anterior si existe
        if (verificacionCoroutine != null)
        {
            StopCoroutine(verificacionCoroutine);
            verificandoEnemigos = false;
        }

        verificacionCoroutine = StartCoroutine(VerificarEnemigosPeriodicamente());
    }

    // Método para iniciar una nueva oleada
    void IniciarOleada()
    {
        // Detener verificación anterior si existe
        if (verificacionCoroutine != null)
        {
            StopCoroutine(verificacionCoroutine);
            verificandoEnemigos = false;
        }

        incrementosRestantes = 0;
        OnOleadaIniciada?.Invoke();
        DesactivarAdvertenciaJefe();

        // Reiniciar contadores
        enemigosGenerados = 0;
        enemigosDestruidos = 0;
        enemigosTotalesOleada = 0;

        // 1. Configuración base
        tiempoEntreEnemigos = GenerarTiempoDivisiblePor02(tiempoMin, tiempoMax);
        oleadaEnCurso = true;

        // 2. Fórmula exponencial ajustada
        enemigosGenerados = Mathf.FloorToInt(4 * Mathf.Pow(1.15f, oleadaActual));

        // 3. Gestión de portales

        // Si estamos en la oleada justa antes de activar un nuevo portal, instanciar el camino
        if ((oleadaActual + 1) % activarPortal == 0 && proximoPortalAleatorio != -1)
        {
            portalesEnemigos[proximoPortalAleatorio].InstanciarCaminoAleatorio();
        }

        // Activar el portal aleatorio seleccionado si corresponde a esta oleada
        if (oleadaActual % activarPortal == 0 && proximoPortalAleatorio != -1)
        {
            portalesEnemigos[proximoPortalAleatorio].ActivarPortal(true);
            indicesPortalesActivados.Add(proximoPortalAleatorio);

            // Seleccionar el próximo portal aleatorio para la siguiente activación
            SeleccionarProximoPortalAleatorio();
        }

        // 4. Generación de jefe (si es oleada de jefe)
        if (oleadaActual % oleadasParaJefe == 0)
        {
            SpawnearJefe();
        }

        // 5. Distribución inteligente de enemigos
        List<PortalEnemigo> portalesActivos = ObtenerPortalesActivos();
        if (portalesActivos.Count > 0)
        {
            int enemigosPorPortal = enemigosGenerados / portalesActivos.Count;
            int sobrantes = enemigosGenerados % portalesActivos.Count;

            // Asegurar distribución exacta
            for (int i = 0; i < portalesActivos.Count; i++)
            {
                int cantidad = enemigosPorPortal + (i < sobrantes ? 1 : 0);
                portalesActivos[i].IniciarInstanciacion(cantidad, tiempoEntreEnemigos);
            }

            enemigosTotalesOleada = enemigosGenerados + jefesVivos;
        }

        sliderOleada.value = 0f;
        llenandoSlider = true;
        tiempoInicioLlenado = Time.time;

        // Programar una verificación obligatoria después de un tiempo 
        // para asegurarse de que la oleada no se quede atascada
        float tiempoMaximoOleada = tiempoEntreEnemigos * (enemigosGenerados + 5) + 10f;
        Invoke("VerificarOleadaAtascada", tiempoMaximoOleada);
    }

    // Método para verificar si la oleada está atascada
    private void VerificarOleadaAtascada()
    {
        if (oleadaEnCurso)
        {
            ForzarVerificacionEnemigos();
        }
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

        // Detener verificación de enemigos
        if (verificacionCoroutine != null)
        {
            StopCoroutine(verificacionCoroutine);
            verificandoEnemigos = false;
        }

        // Cancelar invocaciones pendientes
        CancelInvoke();

        // Iniciar el movimiento de la cámara hacia el objetivo
        moverCamara.IniciarMovimiento();
        GetComponent<ControladorVolumen>().CambiarModoGrave();

        // Activar el panel de Game Over después de un retraso
        if (panelGameOver != null)
        {
            Invoke("ActivarPanelGameOver", retrasoCanvas);
        }

        // Notificar a los suscriptores que la TorreRey ha sido destruida
        OnTorreReyDestruida?.Invoke();
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
            jefesVivos++;
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
        enemigosDestruidos++;

        // Si llegamos al total o lo superamos
        if (enemigosDestruidos >= enemigosTotalesOleada && jefesVivos == 0)
        {
            // Verificar si realmente no hay enemigos en escena
            if (!HayEnemigosEnEscena())
            {
                FinalizarOleada();
            }
            else if (!verificandoEnemigos)
            {
                // Si todavía hay enemigos, iniciar verificación periódica
                ForzarVerificacionEnemigos();
            }
        }
    }

    // Método para notificar que un jefe ha sido destruido
    public void NotificarJefeDestruido()
    {
        jefesVivos = Mathf.Max(0, jefesVivos - 1); // Evitar negativos
        enemigosDestruidos++;

        // Si llegamos al total o lo superamos
        if (enemigosDestruidos >= enemigosTotalesOleada && jefesVivos == 0)
        {
            // Verificar si realmente no hay enemigos en escena
            if (!HayEnemigosEnEscena())
            {
                FinalizarOleada();
            }
            else if (!verificandoEnemigos)
            {
                // Si todavía hay enemigos, iniciar verificación periódica
                ForzarVerificacionEnemigos();
            }
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
            }
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
        // Cancelar invocaciones pendientes
        CancelInvoke("VerificarOleadaAtascada");

        // Si ya no estamos en una oleada, ignorar
        if (!oleadaEnCurso)
        {
            return;
        }

        oleadaEnCurso = false; // Marcar que la oleada ha terminado
        llenandoSlider = false;

        if (incrementosRestantes < valorMaximoPorOleada)
        {
            valorRestante = valorMaximoPorOleada - incrementosRestantes;
            valorActual += valorRestante;
            textoContador.text = valorActual.ToString();
            valorRestante = 0;
            sliderOleada.value = 0;
        }

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

        // Reiniciar contadores
        jefesVivos = 0;
        enemigosDestruidos = 0;
        enemigosTotalesOleada = 0;
        OnOleadaFinalizada?.Invoke();
    }

    // Método para recuperar la vida de la TorreRey
    private void RecuperarVidaTorreRey()
    {
        if (torreRey != null)
        {
            torreRey.RecuperarVidaCompleta();
        }
    }
}