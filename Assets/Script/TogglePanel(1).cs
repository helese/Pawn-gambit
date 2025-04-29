using UnityEngine;
using UnityEngine.UI;

public class TogglePanel : MonoBehaviour
{
    [Header("Referencias")]
    public Button botonControl; // El botón que activará/desactivará el panel
    public GameObject panel;   // El panel que quieres controlar

    [Header("Configuración")]
    public bool estadoInicial = false; // Estado inicial del panel
    public AudioClip sonidoClick;      // Sonido opcional al tocar el botón

    private AudioSource audioSource;

    void Start()
    {
        // Configuración inicial
        panel.SetActive(estadoInicial);

        // Asignar el evento del botón
        if (botonControl != null)
        {
            botonControl.onClick.AddListener(TogglePanelEstado);
        }
        else
        {
            Debug.LogError("No se ha asignado un botón al script TogglePanel");
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

        Debug.Log($"Panel {panel.name} ahora está {(nuevoEstado ? "ACTIVADO" : "DESACTIVADO")}");
    }

    // Método público para cambiar el estado desde otros scripts
    public void SetPanelEstado(bool activar)
    {
        panel.SetActive(activar);
    }
}