using UnityEngine;

public class Pausa : MonoBehaviour
{
    [Header("Configuración de Pausa")]
    public GameObject panelPausa;

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
            if (juegoPausado)
            {
                ReanudarJuego();
                GetComponent<ControladorVolumen>().CambiarModoGrave();
            }
            else
            {
                PausarJuego();
                GetComponent<ControladorVolumen>().CambiarModoGrave();
            }
        }
    }

    void PausarJuego()
    {
        // Pausar el tiempo del juego
        Time.timeScale = 0f;

        panelPausa.SetActive(true);

        juegoPausado = true;
    }

    void ReanudarJuego()
    {
        // Reanudar el tiempo del juego
        Time.timeScale = 1f;

        panelPausa.SetActive(false);

        juegoPausado = false;
    }

    public void Pausar()
    {
        if (!juegoPausado)
        {
            PausarJuego();
        }
    }
    public void Reanudar()
    {
        if (juegoPausado)
        {
            ReanudarJuego();
        }
    }
    public void TorreDestruida()
    {
        seDestruyo = true;
    }
}