using UnityEngine;
using UnityEngine.UI;

public class BestiarioBoton : MonoBehaviour
{
    [Header("Configuraci�n")]
    public string idEnemigoAsociado = "FuerteRojo"; // �Debe coincidir con el ID del enemigo!

    private const string clavePrefijo = "Bestiario_"; // Prefijo para las claves de PlayerPrefs

    private void Start()
    {
        // Verificar si el bot�n ya estaba activado en una sesi�n anterior
        if (PlayerPrefs.GetInt(clavePrefijo + idEnemigoAsociado, 0) == 1)
        {
            gameObject.SetActive(true); // Activar el bot�n si ya estaba desbloqueado
        }
        else
        {
            gameObject.SetActive(false); // Desactivar el bot�n si no estaba desbloqueado
        }

        // Verificar si el enemigo ya fue derrotado en esta sesi�n
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
        gameObject.SetActive(true); // Activar el bot�n

        // Guardar el estado en PlayerPrefs
        PlayerPrefs.SetInt(clavePrefijo + idEnemigoAsociado, 1);
        PlayerPrefs.Save();

        Debug.Log($"Bot�n {idEnemigoAsociado} activado y guardado en PlayerPrefs.");
    }
}