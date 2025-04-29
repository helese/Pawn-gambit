using UnityEngine;
using UnityEngine.UI;

public class TogglePanel : MonoBehaviour
{
    [Header("Referencias")]
    public Button botonControl; // El bot�n que activar�/desactivar� el panel
    public GameObject panel;   // El panel que quieres controlar

    [Header("Configuraci�n")]
    public bool estadoInicial = false; // Estado inicial del panel
    public AudioClip sonidoClick;      // Sonido opcional al tocar el bot�n

    private AudioSource audioSource;

    void Start()
    {
        // Configuraci�n inicial
        panel.SetActive(estadoInicial);

        // Asignar el evento del bot�n
        if (botonControl != null)
        {
            botonControl.onClick.AddListener(TogglePanelEstado);
        }
        else
        {
            Debug.LogError("No se ha asignado un bot�n al script TogglePanel");
        }

        // Configurar AudioSource si hay sonido
        if (sonidoClick != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = sonidoClick;
        }
    }

    public void TogglePanelEstado()
    {
        // Reproducir sonido si existe
        if (sonidoClick != null && audioSource != null)
        {
            audioSource.Play();
        }

        // Cambiar estado del panel
        bool nuevoEstado = !panel.activeSelf;
        panel.SetActive(nuevoEstado);

        Debug.Log($"Panel {panel.name} ahora est� {(nuevoEstado ? "ACTIVADO" : "DESACTIVADO")}");
    }

    // M�todo p�blico para cambiar el estado desde otros scripts
    public void SetPanelEstado(bool activar)
    {
        panel.SetActive(activar);
    }
}