using UnityEngine;

public class EscaladorY : MonoBehaviour
{
    [Header("Configuraci�n de Escala")]
    [Tooltip("Escala m�nima relativa en el eje Y")]
    public float escalaMinY = 0.5f;

    [Tooltip("Escala m�xima relativa en el eje Y")]
    public float escalaMaxY = 2.0f;

    void Start()
    {
        // Guardar la escala original
        Vector3 escalaOriginal = transform.localScale;

        // Calcular nuevo valor de escala Y
        float nuevaEscalaY = Random.Range(escalaMinY, escalaMaxY);

        // Aplicar nueva escala manteniendo X y Z originales
        transform.localScale = new Vector3(
            escalaOriginal.x,
            escalaOriginal.y * nuevaEscalaY,
            escalaOriginal.z
        );
    }
}