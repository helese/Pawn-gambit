using UnityEngine;
using UnityEngine.UI;

public class BestiarioBoton : MonoBehaviour
{
    [Header("Configuración")]
    public string idEnemigoAsociado = "FuerteRojo"; // ¡Debe coincidir con el ID del enemigo!

    private void Start()
    {
        // Desactivar el botón al inicio (solo se activa si el enemigo fue derrotado)
        gameObject.SetActive(false);

        // Verificar si el enemigo ya fue derrotado
        VerificarEstado();
    }

    public void VerificarEstado()
    {
        if (BestiarioManager.Instance.enemigosDerrotados.Contains(idEnemigoAsociado))
        {
            gameObject.SetActive(true); // Activar el botón
        }
    }
}