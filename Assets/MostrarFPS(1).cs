using UnityEngine;
using TMPro;

public class MostrarFPS : MonoBehaviour
{
    [Tooltip("Componente TextMeshProUGUI para mostrar los FPS")]
    [SerializeField] private TextMeshProUGUI textoFPS;

    [Tooltip("Prefijo para el texto de FPS (opcional)")]
    [SerializeField] private string prefijo = "FPS: ";

    private float contadorTiempo = 0f;
    private float intervaloActualizacion = 1f;  // Actualizar cada segundo
    private int fotogramasAcumulados = 0;
    private int fpsActuales = 0;

    private void Start()
    {
        // Verificar que tengamos una referencia válida al texto
        if (textoFPS == null)
        {
            Debug.LogError("¡Falta el componente TextMeshProUGUI! Asigna un texto TMP en el inspector.");
            enabled = false;
            return;
        }

        // Inicializar texto
        textoFPS.text = prefijo + "0";
    }

    private void Update()
    {
        // Contar fotogramas
        fotogramasAcumulados++;

        // Actualizar el contador de tiempo
        contadorTiempo += Time.unscaledDeltaTime;

        // Comprobar si ha pasado el intervalo de actualización
        if (contadorTiempo >= intervaloActualizacion)
        {
            // Calcular FPS y redondear a la unidad
            fpsActuales = Mathf.RoundToInt(fotogramasAcumulados / contadorTiempo);

            // Actualizar el texto
            textoFPS.text = prefijo + fpsActuales.ToString();

            // Resetear contadores
            fotogramasAcumulados = 0;
            contadorTiempo = 0f;
        }
    }
}