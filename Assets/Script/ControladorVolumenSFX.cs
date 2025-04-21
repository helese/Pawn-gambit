using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorVolumenSFX : MonoBehaviour
{
    [Header("Configuración de Volumen SFX")]
    public Slider sliderVolumenSFX;
    public TextMeshProUGUI textoVolumenSFX;

    private Dictionary<SonidoBoton, float> volumenesOriginales = new Dictionary<SonidoBoton, float>();
    private float volumenSFX = 1.0f;
    private const string claveVolumenSFX = "VolumenSFX";

    void Start()
    {
        // Cargar el volumen guardado
        volumenSFX = PlayerPrefs.GetFloat(claveVolumenSFX, 1.0f);

        // Configurar el slider
        sliderVolumenSFX.value = volumenSFX;
        sliderVolumenSFX.onValueChanged.AddListener(CambiarVolumenSFX);

        // Actualizar texto
        ActualizarTextoVolumen();

        // Buscar todos los scripts SonidoBoton al inicio
        BuscarSonidosBoton();
    }

    private void BuscarSonidosBoton()
    {
        // Obtener TODOS los SonidoBoton en la escena
        SonidoBoton[] todosSonidosBoton = GameObject.FindObjectsOfType<SonidoBoton>();

        foreach (SonidoBoton sonido in todosSonidosBoton)
        {
            // Registrar el sonido si aún no está en nuestro diccionario
            RegistrarNuevoSonidoBoton(sonido);
        }

        Debug.Log($"Encontrados {volumenesOriginales.Count} SonidoBoton para control de volumen");
    }

    void CambiarVolumenSFX(float nuevoVolumen)
    {
        volumenSFX = nuevoVolumen;

        // Actualizar texto
        ActualizarTextoVolumen();

        // Aplicar a todos los SonidoBoton
        AplicarVolumenATodos();

        // Guardar en PlayerPrefs
        PlayerPrefs.SetFloat(claveVolumenSFX, volumenSFX);
        PlayerPrefs.Save();
    }

    private void ActualizarTextoVolumen()
    {
        if (textoVolumenSFX != null)
        {
            textoVolumenSFX.text = $"{volumenSFX * 100:F0}%";
        }
    }

    private void AplicarVolumenATodos()
    {
        foreach (var entrada in volumenesOriginales)
        {
            SonidoBoton sonido = entrada.Key;
            float volumenOriginal = entrada.Value;

            if (sonido != null)
            {
                // Aplicar volumen como un porcentaje del volumen original
                sonido.volume = volumenOriginal * volumenSFX;

                // Si el botón tiene un slider, actualizarlo también
                if (sonido.volumeSlider != null)
                {
                    sonido.volumeSlider.value = sonido.volume;
                }
            }
        }
    }

    // Método público para registrar nuevos SonidoBoton en tiempo de ejecución
    public void RegistrarNuevoSonidoBoton(SonidoBoton nuevoSonido)
    {
        if (nuevoSonido != null && !volumenesOriginales.ContainsKey(nuevoSonido))
        {
            // Guardar el volumen original
            volumenesOriginales[nuevoSonido] = nuevoSonido.volume;

            // Aplicar configuración actual
            nuevoSonido.volume = nuevoSonido.volume * volumenSFX;

            // Si el sonido tiene un slider, actualizarlo también
            if (nuevoSonido.volumeSlider != null)
            {
                nuevoSonido.volumeSlider.value = nuevoSonido.volume;
            }
        }
    }

    // Método para escanear la escena por nuevos SonidoBoton
    public void BuscarNuevosSonidos()
    {
        BuscarSonidosBoton();
    }
}