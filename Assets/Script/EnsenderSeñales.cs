using UnityEngine;

public class EncenderSeñales : MonoBehaviour
{
    [System.Serializable]
    public class Señal
    {
        public string tagObjeto;         // Tag del objeto a buscar
        public GameObject imagenSeñal;   // Imagen asociada
        [HideInInspector] public bool estadoAnterior;
        [HideInInspector] public bool estadoGuardado;
        [HideInInspector] public GameObject objetoEncontrado; // Objeto encontrado con el tag
    }

    [Header("Configuración de Señales")]
    public Señal[] señales = new Señal[4];
    public float intervaloBusqueda = 1f; // Intervalo para buscar objetos (optimización)

    private bool señalesApagadas = false;
    private float tiempoUltimaBusqueda;

    private void Start()
    {
        InicializarSeñales();
        BuscarObjetosPorTag(); // Buscar objetos al inicio
    }

    private void Update()
    {
        // Buscar objetos periódicamente para optimizar
        if (Time.time - tiempoUltimaBusqueda > intervaloBusqueda)
        {
            BuscarObjetosPorTag();
            tiempoUltimaBusqueda = Time.time;
        }

        ActualizarSeñales();
    }

    private void BuscarObjetosPorTag()
    {
        foreach (var señal in señales)
        {
            if (!string.IsNullOrEmpty(señal.tagObjeto))
            {
                GameObject[] objetos = GameObject.FindGameObjectsWithTag(señal.tagObjeto);
                señal.objetoEncontrado = objetos.Length > 0 ? objetos[0] : null;
            }
        }
    }

    private void InicializarSeñales()
    {
        foreach (var señal in señales)
        {
            if (señal.imagenSeñal != null)
            {
                señal.imagenSeñal.SetActive(false);
                señal.estadoAnterior = false;
                señal.estadoGuardado = false;
            }
        }
    }

    private void ActualizarSeñales()
    {
        foreach (var señal in señales)
        {
            if (string.IsNullOrEmpty(señal.tagObjeto) || señal.imagenSeñal == null)
                continue;

            bool estadoActual = (señal.objetoEncontrado != null && señal.objetoEncontrado.activeSelf) && !señalesApagadas;

            if (estadoActual != señal.estadoAnterior)
            {
                señal.imagenSeñal.SetActive(estadoActual);
                señal.estadoAnterior = estadoActual;

                if (!señalesApagadas)
                {
                    señal.estadoGuardado = estadoActual;
                }
            }
        }
    }

    public void AlternarTodasLasSeñales()
    {
        señalesApagadas = !señalesApagadas;

        foreach (var señal in señales)
        {
            if (señal.imagenSeñal != null)
            {
                if (señalesApagadas)
                {
                    señal.imagenSeñal.SetActive(false);
                }
                else
                {
                    señal.imagenSeñal.SetActive(señal.estadoGuardado);
                    señal.estadoAnterior = señal.estadoGuardado;
                }
            }
        }

        Debug.Log($"Señales {(señalesApagadas ? "APAGADAS" : "RESTAURADAS")}");
    }
}