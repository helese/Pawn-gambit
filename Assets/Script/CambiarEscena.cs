using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public GameObject panel; // Asegúrate de que está desactivado inicialmente
    public float fadeDuration = 1f;
    private string escenaACargar;

    public void CambiarNivel(string nombreEscena)
    {
        escenaACargar = nombreEscena; // Guarda el nombre
        panel.SetActive(true); // Activa el panel (FadeIn)
        Invoke("CargarEscena", fadeDuration); // Retraso
    }

    private void CargarEscena()
    {
        SceneManager.LoadScene(escenaACargar);
    }
}