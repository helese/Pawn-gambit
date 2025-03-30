using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorVolumen : MonoBehaviour
{
    [Header("Configuraci�n de Volumen")]
    public Slider sliderVolumen; 
    public TextMeshProUGUI textoVolumen; 

    [Header("Configuraci�n de Pitch")]
    public float pitchObjetivoGrave = 0.5f; 
    public float velocidadTransicion = 0.5f; 

    private AudioSource audioSource; 
    private float pitchOriginal = 1.0f;
    private float pitchObjetivoActual;
    private bool musicaGrave = false;

    private const string claveVolumen = "Volumen"; //PlayerPrefs

    void Start()
    {
        GameObject objMusica = GameObject.FindWithTag("ObjMusica");
        audioSource = objMusica.GetComponent<AudioSource>();

        // Cargar el volumen
        float volumenGuardado = PlayerPrefs.GetFloat(claveVolumen, 1.0f);
        audioSource.volume = volumenGuardado;
        sliderVolumen.value = volumenGuardado;

        // Poner el volumen al del pref
        sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        pitchObjetivoActual = pitchOriginal;

        textoVolumen.text = $"{volumenGuardado * 100:F0}%";

        if (SceneManager.GetActiveScene().name == "Inicio")
        {
            CambiarModoGrave();
        }
    }

    void Update()
    {

        // Transici�n gradual del pitch
        if (audioSource.pitch != pitchObjetivoActual)
        {
            audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, pitchObjetivoActual, velocidadTransicion * Time.unscaledDeltaTime);
        }
    }

    void CambiarVolumen(float nuevoVolumen)
    {
        audioSource.volume = nuevoVolumen;
        textoVolumen.text = $"{nuevoVolumen * 100:F0}%";
        PlayerPrefs.SetFloat(claveVolumen, nuevoVolumen);
        PlayerPrefs.Save();
    }

    // Alternar el pitch
    public void CambiarModoGrave()
    {

        if (musicaGrave)
        {
            pitchObjetivoActual = pitchOriginal;
            musicaGrave = false;
            Debug.Log("M�sica vuelve a tono normal.");
        }
        else
        {
            pitchObjetivoActual = pitchObjetivoGrave;
            musicaGrave = true;
            Debug.Log("M�sica en modo grave.");
        }
    }
}