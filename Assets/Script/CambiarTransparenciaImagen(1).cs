using UnityEngine;
using UnityEngine.UI;

public class CambiarTransparenciaImagen : MonoBehaviour
{
    [Header("Configuración de transparencia")]
    [Range(0f, 1f)] public float transparenciaInicial = 1f; 
    [Range(0f, 1f)] public float transparenciaFinal = 0f;  
    public float duracion = 2f;

    private Image imagen;
    private float tiempoInicio;
    private bool transicionActiva = false; 

    void Start()
    {

        imagen = GetComponent<Image>();

        // Establecer la transparencia inicial
        Color colorInicial = imagen.color;
        colorInicial.a = transparenciaInicial;
        imagen.color = colorInicial;

        // Iniciar la transición automáticamente al inicio
        IniciarTransicion();
    }

    void Update()
    {
        // Si la transición está activa, actualizar la transparencia
        if (transicionActiva)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / duracion);

            // Calcular la transparencia actual
            float transparenciaActual = Mathf.Lerp(transparenciaInicial, transparenciaFinal, progreso);

            // Aplicar la transparencia a la imagen
            Color colorActual = imagen.color;
            colorActual.a = transparenciaActual;
            imagen.color = colorActual;

            // Detener la transición cuando se alcance la duración
            if (progreso >= 1f)
            {
                transicionActiva = false;
            }
        }
    }
    public void IniciarTransicion()
    {
        if (imagen != null)
        {
            tiempoInicio = Time.time;
            transicionActiva = true;
        }
    }
}