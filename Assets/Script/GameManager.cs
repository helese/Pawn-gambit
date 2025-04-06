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
        Debug.LogWarning("Iniciando verificación periódica de enemigos...");

        // Primera verificación inmediata
        GameObject[] enemigosInicial = GameObject.FindGameObjectsWithTag("Enemigo");
        if (enemigosInicial.Length == 0 && oleadaEnCurso)
        {
            Debug.LogWarning("¡No se encontraron enemigos en verificación inicial! Finalizando oleada.");
            FinalizarOleada();
            verificandoEnemigos = false;
            yield break;
        }

        int iteraciones = 0;
        while (juegoActivo && oleadaEnCurso)
        {
            // Contar enemigos explícitamente
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
            int cantidadEnemigos = enemigos.Length;

            Debug.LogWarning($"Verificación #{++iteraciones}: Encontrados {cantidadEnemigos} enemigos");

            if (cantidadEnemigos == 0)
            {
                Debug.LogWarning("¡No se encontraron más enemigos! Finalizando oleada.");
                FinalizarOleada();
                verificandoEnemigos = false;
                yield break;
            }

            // Mostrar los enemigos encontrados (limitado a 5 para no sobrecargar el log)
            if (cantidadEnemigos > 0 && cantidadEnemigos <= 5)
            {
                foreach (GameObject enemigo in enemigos)
                {
                    Debug.LogWarning($"Enemigo encontrado: {enemigo.name} en posición {enemigo.transform.position}");
                }
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

        // 3. Gestión de portales (cada 'activarPortal' oleadas)
        int portalIndex = oleadaActual / activarPortal;

        // Preparar portal siguiente (1 oleada antes)
        if ((oleadaActual + 1) % activarPortal == 0)
        {
            int nextPortal = (oleadaActual + 1) / activarPortal;
            if (nextPortal < portalesEnemigos.Count)
            {
                portalesEnemigos[nextPortal].InstanciarCaminoAleatorio();
                Debug.Log($"Preparando portal {nextPortal + 1} en oleada {oleadaActual}");
            }
        }

        // Activar portal si corresponde
        if (oleadaActual % activarPortal == 0)
        {
            int portalAActivar = oleadaActual / activarPortal;
            if (portalAActivar < portalesEnemigos.Count)
            {
                portalesEnemigos[portalAActivar].ActivarPortal(true);
            }
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
            Debug.LogWarning($"Enemigos totales: {enemigosTotalesOleada} (Normales: {enemigosGenerados}, Jefes: {jefesVivos})");
        }

        // 6. Sistema de progresión UI
        Debug.LogWarning($"Iniciando oleada {oleadaActual}: {enemigosGenerados} enemigos " + $"({portalesActivos.Count} portales activos)");

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
            Debug.LogWarning("Verificación automática de oleada atascada...");
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
            jefesVivos++; // Contador de jefes activos
            Debug.Log($"Jefe generado. Jefes vivos: {jefesVivos}");
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
        Debug.Log($"Destruidos: {enemigosDestruidos}/{enemigosTotalesOleada}");

        // Si llegamos al total o lo superamos
        if (enemigosDestruidos >= enemigosTotalesOleada && jefesVivos == 0)
        {
            // Verificar si realmente no hay enemigos en escena
            if (!HayEnemigosEnEscena())
            {
                Debug.LogWarning("No hay enemigos detectados en escena después de destrucción. Finalizando oleada directamente.");
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
        Debug.Log($"Jefe destruido. Jefes vivos: {jefesVivos}, Enemigos destruidos: {enemigosDestruidos}/{enemigosTotalesOleada}");

        // Si llegamos al total o lo superamos
        if (enemigosDestruidos >= enemigosTotalesOleada && jefesVivos == 0)
        {
            // Verificar si realmente no hay enemigos en escena
            if (!HayEnemigosEnEscena())
            {
                Debug.LogWarning("No hay enemigos detectados en escena después de destrucción del jefe. Finalizando oleada directamente.");
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
        // Cancelar invocaciones pendientes
        CancelInvoke("VerificarOleadaAtascada");

        // Si ya no estamos en una oleada, ignorar
        if (!oleadaEnCurso)
        {
            Debug.LogWarning("Intento de finalizar oleada cuando ya no está en curso. Ignorando.");
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