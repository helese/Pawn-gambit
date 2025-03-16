using UnityEngine;

public class MoverPanel : MonoBehaviour
{
    [Header("Configuraci�n del Panel")]
    public RectTransform panel; // El panel que se mover�
    public Vector2 posicionObjetivo; // La posici�n a la que se mover� el panel

    [Header("Configuraci�n del Movimiento")]
    public float tiempoMovimiento = 2f; // Tiempo que tarda en llegar a la posici�n objetivo o volver

    private Vector2 posicionInicial; // Posici�n inicial del panel
    private bool moverAPosicionObjetivo = false; // Indica si el panel debe moverse hacia la posici�n objetivo
    private bool moverAPosicionInicial = false; // Indica si el panel debe volver a la posici�n inicial
    private float tiempoTranscurrido = 0f; // Tiempo transcurrido durante el movimiento

    void Start()
    {
        // Guardar la posici�n inicial del panel
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
        // Mover el panel hacia la posici�n objetivo
        if (moverAPosicionObjetivo && panel != null)
        {
            MoverHaciaPosicion(posicionObjetivo);
        }

        // Mover el panel de vuelta a la posici�n inicial
        if (moverAPosicionInicial && panel != null)
        {
            MoverHaciaPosicion(posicionInicial);
        }
    }

    // M�todo para activar el movimiento hacia la posici�n objetivo
    public void ActivarMovimiento()
    {
        moverAPosicionObjetivo = true;
        moverAPosicionInicial = false;
        tiempoTranscurrido = 0f; // Reiniciar el tiempo transcurrido
    }

    // M�todo para activar el movimiento de vuelta a la posici�n inicial
    public void ReiniciarPosicion()
    {
        moverAPosicionInicial = true;
        moverAPosicionObjetivo = false;
        tiempoTranscurrido = 0f; // Reiniciar el tiempo transcurrido
    }

    // M�todo para mover el panel hacia una posici�n espec�fica
    private void MoverHaciaPosicion(Vector2 posicionDestino)
    {
        // Incrementar el tiempo transcurrido
        tiempoTranscurrido += Time.deltaTime;

        // Calcular el progreso del movimiento (0 a 1)
        float progreso = Mathf.Clamp01(tiempoTranscurrido / tiempoMovimiento);

        // Mover el panel suavemente hacia la posici�n destino
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, posicionDestino, progreso);

        // Detener el movimiento cuando se alcance el tiempoMovimiento
        if (progreso >= 1f)
        {
            moverAPosicionObjetivo = false;
            moverAPosicionInicial = false;
        }
    }
}