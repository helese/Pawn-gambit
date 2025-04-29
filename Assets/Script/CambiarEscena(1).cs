using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
public class CambiarEscena : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject fadeOut; // Panel de transición (debe tener CanvasGroup)
    public GameObject panelAviso;
    public GameObject segundoPanel; // Nuevo GameObject para el segundo panel
    public float fadeDuration = 1f;
    private string escenaACargar;
    private CanvasGroup canvasGroup;
    // Método principal para cambiar escena con fade
    public void CambiarNivel(string nombreEscena)
    {
        // Restaurar timeScale si está pausado
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = 1f;
        }
        escenaACargar = nombreEscena; // Guarda el nombre
        fadeOut.SetActive(true); // Activa el panel (FadeIn)
        Invoke("CargarEscena", fadeDuration); // Retraso
    }
    // 1. Método para activar panel (con fade opcional)
    public void ActivarPanel()
    {
        if (panelAviso != null)
        {
            panelAviso.SetActive(true);
        }
    }
    // 2. Método para desactivar panel (con fade opcional)
    public void DesactivarPanel()
    {
        if (panelAviso != null)
        {
            panelAviso.SetActive(false);
        }
    }

    // Nuevos métodos para el segundo panel
    public void ActivarSegundoPanel()
    {
        if (segundoPanel != null)
        {
            segundoPanel.SetActive(true);
        }
    }

    public void DesactivarSegundoPanel()
    {
        if (segundoPanel != null)
        {
            segundoPanel.SetActive(false);
        }
    }

    public void ActivarSalidaJuego()
    {
        fadeOut.SetActive(true);
        Invoke("SalirDelJuego", fadeDuration); // Retraso
    }
    // 3. Método para salir del juego
    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void CargarEscena()
    {
        SceneManager.LoadScene(escenaACargar);
    }
}