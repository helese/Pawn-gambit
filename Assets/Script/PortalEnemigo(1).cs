using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PortalEnemigo : MonoBehaviour
{
    [System.Serializable]
    public struct EnemigoConfig
    {
        public GameObject prefab; // Prefab del enemigo
        [Range(0, 100)] public int probabilidad; // Probabilidad de spawn (0 a 100)
    }

    [Header("Configuraci�n de enemigos")]
    public List<EnemigoConfig> enemigosConfig;  // Lista de enemigos y sus probabilidades

    [Header("Configuraci�n de jefes")]
    public List<GameObject> jefes; // Lista de prefabs de jefes

    [Header("Configuraci�n del portal")]
    public List<GameObject> caminosPrefabs; // Lista de prefabs de caminos para este portal
    public Transform puntoDeAparicion; // Objeto que define la posici�n de aparici�n de los enemigos
    private bool estaActivo = false; // Indica si el portal est� activo
    private GameObject caminoInstanciado; // Referencia al camino instanciado
    private bool caminoGenerado = false; // Bandera para evitar generar el camino m�s de una vez

    [Header("Advertencia de jefe")]
    public GameObject advertenciaJefe; // Objeto que se activa antes de que aparezca un jefe (ajustable desde el Inspector)

    // Referencia al GameManager
    private GameManager gameManager;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Desactivar la advertencia de jefe al inicio
        if (advertenciaJefe != null)
        {
            advertenciaJefe.SetActive(false);
        }
    }

    // M�todo para activar la advertencia de jefe
    public void ActivarAdvertenciaJefe()
    {
        if (advertenciaJefe != null)
        {
            advertenciaJefe.SetActive(true);
        }
    }

    // M�todo para desactivar la advertencia de jefe
    public void DesactivarAdvertenciaJefe()
    {
        if (advertenciaJefe != null)
        {
            advertenciaJefe.SetActive(false);
        }
    }

    // M�todo para activar o desactivar el portal
    public void ActivarPortal(bool activar)
    {
        estaActivo = activar;
        gameObject.SetActive(activar); // Activar o desactivar el objeto del portal

        // Si se desactiva el portal, destruir el camino instanciado
        if (!activar && caminoInstanciado != null)
        {
            Destroy(caminoInstanciado);
            caminoGenerado = false; // Reiniciar la bandera
        }
    }

    // M�todo para verificar si el portal est� activo
    public bool EstaActivo()
    {
        return estaActivo;
    }

    // M�todo para instanciar un camino aleatorio
    public void InstanciarCaminoAleatorio()
    {
        if (!caminoGenerado && caminosPrefabs.Count > 0)
        {
            int caminoAleatorio = Random.Range(0, caminosPrefabs.Count);
            caminoInstanciado = Instantiate(caminosPrefabs[caminoAleatorio]);
            caminoGenerado = true; // Marcar que el camino ya se gener�
            Debug.Log($"Camino aleatorio instanciado para el portal en {transform.position}.");
        }
    }

    // M�todo para iniciar la instanciaci�n de enemigos
    public void IniciarInstanciacion(int cantidad, float intervalo)
    {
        if (estaActivo)
        {
            StartCoroutine(InstanciarEnemigosUnoPorUno(cantidad, intervalo));
        }
    }

    // Corrutina para instanciar enemigos uno por uno
    private IEnumerator InstanciarEnemigosUnoPorUno(int cantidad, float intervalo)
    {
        for (int i = 0; i < cantidad; i++)
        {
            // Seleccionar un enemigo aleatorio basado en las probabilidades
            GameObject enemigoPrefab = SeleccionarEnemigoAleatorio();

            if (enemigoPrefab != null)
            {
                // Instanciar el enemigo en la posici�n del punto de aparici�n
                Instantiate(enemigoPrefab, puntoDeAparicion.position, puntoDeAparicion.rotation);
            }

            // Esperar el intervalo de tiempo antes de instanciar el siguiente enemigo
            yield return new WaitForSeconds(intervalo);
        }
    }

    // M�todo para seleccionar un enemigo aleatorio basado en las probabilidades
    private GameObject SeleccionarEnemigoAleatorio()
    {
        // Calcular la suma total de probabilidades
        int totalProbabilidad = 0;
        foreach (var config in enemigosConfig)
        {
            totalProbabilidad += config.probabilidad;
        }

        // Si la suma total es 0, no hay enemigos para spawnear
        if (totalProbabilidad == 0)
        {
            Debug.LogWarning("No hay enemigos configurados o las probabilidades suman 0.");
            return null;
        }

        // Generar un n�mero aleatorio dentro del rango de la suma total
        int valorAleatorio = Random.Range(0, totalProbabilidad);

        // Seleccionar el enemigo correspondiente al valor aleatorio
        int acumulado = 0;
        foreach (var config in enemigosConfig)
        {
            acumulado += config.probabilidad;
            if (valorAleatorio < acumulado)
            {
                return config.prefab;
            }
        }

        // Si no se selecciona ning�n enemigo (por ejemplo, si no hay configuraciones)
        return null;
    }

    // M�todo para spawnear un jefe
    public void SpawnearJefe()
    {
        if (jefes.Count > 0)
        {
            int jefeAleatorio = Random.Range(0, jefes.Count);
            GameObject jefe = Instantiate(jefes[jefeAleatorio], puntoDeAparicion.position, puntoDeAparicion.rotation);

            // Asignar un script al jefe para notificar su destrucci�n
            Enemigo jefeScript = jefe.GetComponent<Enemigo>();
            if (jefeScript != null)
            {
                jefeScript.esJefe = true; // Marcar como jefe
            }

            Debug.Log($"Jefe {jefes[jefeAleatorio].name} spawnedo en el portal en {transform.position}.");
        }
        else
        {
            Debug.LogWarning("No hay jefes configurados en el portal.");
        }
    }

    // Corrutina para esperar y luego spawnear el jefe
    //private IEnumerator EsperarYSpawnearJefe()
    //{
    //    // Esperar un momento antes de spawnear el jefe (por ejemplo, 2 segundos)
    //    yield return new WaitForSeconds(2f);
    //
    //    // Spawnear el jefe
    //    int jefeAleatorio = Random.Range(0, jefes.Count);
    //    GameObject jefe = Instantiate(jefes[jefeAleatorio], puntoDeAparicion.position, puntoDeAparicion.rotation);
    //
    //    // Asignar un script al jefe para notificar su destrucci�n
    //    Enemigo jefeScript = jefe.GetComponent<Enemigo>();
    //    if (jefeScript != null)
    //    {
    //        jefeScript.esJefe = true; // Marcar como jefe
    //    }
    //
    //    Debug.Log($"Jefe {jefes[jefeAleatorio].name} spawnedo en el portal en {transform.position}.");
    //
    //    // Desactivar la advertencia de jefe
    //    if (advertenciaJefe != null)
    //    {
    //        advertenciaJefe.SetActive(false);
    //    }
    //}
}