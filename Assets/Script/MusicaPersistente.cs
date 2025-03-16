using UnityEngine;

public class MusicaPersistente : MonoBehaviour
{
    private static MusicaPersistente instance; // Singleton para evitar duplicados
    private AudioSource audioSource; // Referencia al AudioSource

    private const string claveVolumen = "Volumen"; // Misma clave que en ControladorVolumen

    // No destruye este objeto
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Obtener el componente AudioSource
        audioSource = GetComponent<AudioSource>();

        // Cargar el volumen guardado en PlayerPrefs
        float volumenGuardado = PlayerPrefs.GetFloat(claveVolumen, 1.0f); // Valor por defecto: 1.0 (100%)
        audioSource.volume = volumenGuardado;

        // Establecer el pitch a 0.5 al iniciarse
        audioSource.pitch = 0.5f;
    }

    // Método para actualizar el volumen desde otro script
    public void ActualizarVolumen(float nuevoVolumen)
    {
        if (audioSource != null)
        {
            audioSource.volume = nuevoVolumen;
        }
    }
}