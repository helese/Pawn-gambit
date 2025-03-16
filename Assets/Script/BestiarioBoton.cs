using UnityEngine;
using UnityEngine.UI;

public class BestiarioBoton : MonoBehaviour
{
    [Header("Configuración")]
    public string idEnemigoAsociado = "FuerteRojo"; // ¡Debe coincidir con el ID del enemigo!

    private const string clavePrefijo = "Bestiario_"; // Prefijo para las claves de PlayerPrefs

    private void Start()
    {
        // Verificar si el botón ya estaba activado en una sesión anterior
        if (PlayerPrefs.GetInt(clavePrefijo + idEnemigoAsociado, 0) == 1)
        {
            gameObject.SetActive(true); // Activar el botón si ya estaba desbloqueado
        }
        else
        {
            gameObject.SetActive(false); // Desactivar el botón si no estaba desbloqueado
        }

        // Verificar si el enemigo ya fue derrotado en esta sesión
        VerificarEstado();
    }

    public void VerificarEstado()
    {
        if (BestiarioManager.Instance.enemigosDerrotados.Contains(idEnemigoAsociado))
        {
            ActivarBoton();
        }
    }

    private void ActivarBoton()
    {
        gameObject.SetActive(true); // Activar el botón

        // Guardar el estado en PlayerPrefs
        PlayerPrefs.SetInt(clavePrefijo + idEnemigoAsociado, 1);
        PlayerPrefs.Save();

        Debug.Log($"Botón {idEnemigoAsociado} activado y guardado en PlayerPrefs.");
    }
}