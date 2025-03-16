using UnityEngine;
using UnityEngine.UI;

public class BestiarioBoton : MonoBehaviour
{
    [Header("Configuraci�n")]
    public string idEnemigoAsociado = "FuerteRojo"; // �Debe coincidir con el ID del enemigo!

    private void Start()
    {
        // Desactivar el bot�n al inicio (solo se activa si el enemigo fue derrotado)
        gameObject.SetActive(false);

        // Verificar si el enemigo ya fue derrotado
        VerificarEstado();
    }

    public void VerificarEstado()
    {
        if (BestiarioManager.Instance.enemigosDerrotados.Contains(idEnemigoAsociado))
        {
            gameObject.SetActive(true); // Activar el bot�n
        }
    }
}