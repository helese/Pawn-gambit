using UnityEngine;

public class MoverPanel : MonoBehaviour
{
    [Header("Configuración del Panel")]
    public RectTransform panel; // El panel que se moverá
    public Vector2 posicionObjetivo; // La posición a la que se moverá el panel

    [Header("Configuración del Movimiento")]
    public float tiempoMovimiento = 2f; // Tiempo que tarda en llegar a la posición objetivo o volver

    private Vector2 posicionInicial; // Posición inicial del panel
    private bool moverAPosicionObjetivo = false; // Indica si el panel debe moverse hacia la posición objetivo
    private bool moverAPosicionInicial = false; // Indica si el panel debe volver a la posición inicial
    private float tiempoTranscurrido = 0f; // Tiempo transcurrido durante el movimiento

    void Start()
    {
        // Guardar la posición inicial del panel
        if (panel != null)
        {
            posicionInicial = panel.anchoredPosition;
        }
        else
        {
            Debug.LogError("No se ha asignado un panel.");
        }
    }

    void Update()
    {
        // Mover el panel hacia la posición objetivo
        if (moverAPosicionObjetivo && panel != null)
        {
            MoverHaciaPosicion(posicionObjetivo);
        }

        // Mover el panel de vuelta a la posición inicial
        if (moverAPosicionInicial && panel != null)
        {
            MoverHaciaPosicion(posicionInicial);
        }
    }

    // Método para activar el movimiento hacia la posición objetivo
    public void ActivarMovimiento()
    {
        moverAPosicionObjetivo = true;
        moverAPosicionInicial = false;
        tiempoTranscurrido = 0f; // Reiniciar el tiempo transcurrido
    }

    // Método para activar el movimiento de vuelta a la posición inicial
    public void ReiniciarPosicion()
    {
        moverAPosicionInicial = true;
        moverAPosicionObjetivo = false;
        tiempoTranscurrido = 0f; // Reiniciar el tiempo transcurrido
    }

    // Método para mover el panel hacia una posición específica
    private void MoverHaciaPosicion(Vector2 posicionDestino)
    {
        // Incrementar el tiempo transcurrido
        tiempoTranscurrido += Time.deltaTime;

        // Calcular el progreso del movimiento (0 a 1)
        float progreso = Mathf.Clamp01(tiempoTranscurrido / tiempoMovimiento);

        // Mover el panel suavemente hacia la posición destino
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, posicionDestino, progreso);

        // Detener el movimiento cuando se alcance el tiempoMovimiento
        if (progreso >= 1f)
        {
            moverAPosicionObjetivo = false;
            moverAPosicionInicial = false;
        }
    }
}