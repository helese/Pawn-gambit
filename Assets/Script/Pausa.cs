using UnityEngine;

public class Pausa : MonoBehaviour
{
    [Header("Configuración de Pausa")]
    public GameObject panelPausa;
    public GameObject objetoAdicional; // Nuevo GameObject a controlar

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
        Time.timeScale = 0f;
        panelPausa.SetActive(true);
        juegoPausado = true;
    }

    void ReanudarJuego()
    {
        Time.timeScale = 1f;
        panelPausa.SetActive(false);

        // Desactivar el objeto adicional si existe
        if (objetoAdicional != null)
        {
            objetoAdicional.SetActive(false);
        }

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

        // Opcional: Desactivar también el objeto adicional si la torre se destruye
        if (objetoAdicional != null)
        {
            objetoAdicional.SetActive(false);
        }
    }
}