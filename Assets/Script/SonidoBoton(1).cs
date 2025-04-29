using UnityEngine;
using UnityEngine.UI;

public class SonidoBoton : MonoBehaviour
{
    [Header("Configuraci�n de Sonido")]
    public AudioClip soundClip;
    [Range(0f, 1f)] // Crea un slider en el Inspector
    public float volume = 1f; // Volumen ajustable (default: 1 = 100%)

    [Header("Configuraci�n de Pitch")]
    [Range(0.1f, 3f)]
    public float pitchMin = 0.6f; // Pitch m�nimo
    [Range(0.1f, 3f)]
    public float pitchMax = 1.2f; // Pitch m�ximo

    [Header("Referencias UI (Opcional)")]
    public Slider volumeSlider; // Arrastra un Slider de UI aqu� si quieres control visual

    private AudioSource audioSource;

    void Start()
    {
        // Configura el AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Si hay un Slider asignado, configura su valor inicial y su evento
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = volume; // Sincroniza con el valor del Inspector
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }

        // Asigna el sonido al bot�n
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    public void PlaySound()
    {
        if (soundClip != null)
        {
            // Genera un pitch aleatorio entre pitchMin y pitchMax
            float randomPitch = Random.Range(pitchMin, pitchMax);

            // Guarda el pitch original
            float originalPitch = audioSource.pitch;

            // Aplica el nuevo pitch
            audioSource.pitch = randomPitch;

            // Reproduce el sonido con el volumen actual
            audioSource.PlayOneShot(soundClip, volume);

            // Si quieres restaurar el pitch original despu�s de reproducir
            // (�til si usas el mismo AudioSource para otros sonidos)
            // Necesitar�as una corrutina para esto, o simplemente dejarlo como est�
            // si solo se usa para este bot�n
        }
    }

    // M�todo para cambiar el volumen (desde Slider o c�digo)
    public void ChangeVolume(float newVolume)
    {
        volume = newVolume; // Actualiza la variable
        Debug.Log("Volumen cambiado a: " + (volume * 100f) + "%");
    }
}