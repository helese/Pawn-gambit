using UnityEngine;

public class Pausa : MonoBehaviour
{
    [Header("Configuración de Pausa")]
    public GameObject panelPausa;
    public GameObject[] panelesAdicionales; // Paneles que NO son de pausa
    public Jugador jugador;

    private bool juegoPausado = false;
    private bool seDestruyo = false;

    private void Start()
    {
        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !seDestruyo)
        {
            if (AlgunPanelAdicionalActivo())
            {
                // 1. Si hay paneles adicionales activos: cerrarlos
                DesactivarTodosPaneles();
            }
            else if (panelPausa.activeSelf)
            {
                // 2. Si el panel de pausa está activo: cerrarlo
                ReanudarJuego();
            }
            else
            {
                // 3. Si no hay nada activo: abrir pausa
                PausarJuego();
            }
        }
    }

    bool AlgunPanelAdicionalActivo()
    {
        foreach (GameObject panel in panelesAdicionales)
        {
            if (panel != null && panel.activeSelf) return true;
        }
        return false;
    }

    void DesactivarTodosPaneles()
    {
        foreach (GameObject panel in panelesAdicionales)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Asegurar que el canvas del jugador también se desactive
        if (jugador != null) jugador.DesactivarCanvas();
    }

    void PausarJuego()
    {
        Time.timeScale = 0f;
        panelPausa.SetActive(true);
        juegoPausado = true;
        GetComponent<ControladorVolumen>().CambiarModoGrave();
    }

    void ReanudarJuego()
    {
        Time.timeScale = 1f;
        panelPausa.SetActive(false);
        juegoPausado = false;
        GetComponent<ControladorVolumen>().CambiarModoGrave();
    }

    public void TorreDestruida()
    {
        seDestruyo = true;
        DesactivarTodosPaneles();
        ReanudarJuego();
    }
}