using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class OleadaUI : MonoBehaviour
{
    [Header("Configuración Paneles")]
    public CanvasGroup panelOleada; // Panel de notificación de oleada
    public CanvasGroup panelAdvertencia; // Panel de advertencia durante oleada
    public TMP_Text textoOleada;

    [Header("Tiempos")]
    public float tiempoMostrarPanel = 3f;
    public float fadeDuration = 0.5f;
    public float advertenciaIntensity = 0.7f; // Alpha máximo para el panel de advertencia

    [Header("Referencias")]
    public GameManager gameManager;

    private Coroutine currentFadeOleada;
    private Coroutine currentFadeAdvertencia;
    private bool oleadaEnCurso = false;

    private void Start()
    {

        HandleOleadaFinalizada();
        // Inicializar ambos paneles transparentes
        SetPanelAlpha(panelOleada, 0f);
        SetPanelAlpha(panelAdvertencia, 0f);

        if (gameManager != null)
        {
            gameManager.OnOleadaFinalizada += HandleOleadaFinalizada;
            gameManager.OnOleadaIniciada += HandleOleadaIniciada;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnOleadaFinalizada -= HandleOleadaFinalizada;
            gameManager.OnOleadaIniciada -= HandleOleadaIniciada;
        }
    }

    private void HandleOleadaIniciada()
    {
        oleadaEnCurso = true;
        // Mostrar advertencia gradualmente
        StartFade(panelAdvertencia, advertenciaIntensity, fadeDuration * 2, ref currentFadeAdvertencia);
    }

    private void HandleOleadaFinalizada()
    {
        oleadaEnCurso = false;

        // Mostrar notificación de oleada completada
        textoOleada.text = $"Oleada {gameManager.oleadaActual}";
        StartFade(panelOleada, 1f, fadeDuration, ref currentFadeOleada, () =>
        {
            StartCoroutine(OcultarPanelDespuesDeTiempo(panelOleada, tiempoMostrarPanel));
        });

        // Ocultar advertencia gradualmente
        StartFade(panelAdvertencia, 0f, fadeDuration * 1.5f, ref currentFadeAdvertencia);
    }

    private void StartFade(CanvasGroup panel, float targetAlpha, float duration, ref Coroutine trackingCoroutine, System.Action onComplete = null)
    {
        if (panel == null) return;

        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
        }

        trackingCoroutine = StartCoroutine(FadePanel(panel, panel.alpha, targetAlpha, duration, onComplete));
    }

    private IEnumerator FadePanel(CanvasGroup panel, float startAlpha, float targetAlpha, float duration, System.Action onComplete = null)
    {
        panel.blocksRaycasts = targetAlpha > 0.1f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panel.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    private IEnumerator OcultarPanelDespuesDeTiempo(CanvasGroup panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartFade(panel, 0f, fadeDuration, ref currentFadeOleada);
    }

    private void SetPanelAlpha(CanvasGroup panel, float alpha)
    {
        if (panel != null)
        {
            panel.alpha = alpha;
            panel.blocksRaycasts = alpha > 0.1f;
        }
    }

    // Llamar desde Update si necesitas ajuste continuo durante la oleada
    private void UpdateAdvertenciaDuringCombat()
    {
        if (oleadaEnCurso && panelAdvertencia != null)
        {
            // Ejemplo: pulsación sutil durante combate
            float pulse = Mathf.PingPong(Time.time * 0.5f, 0.1f);
            panelAdvertencia.alpha = Mathf.Clamp(advertenciaIntensity + pulse, 0f, 0.9f);
        }
    }
}