using UnityEngine;
using TMPro;
using System.Collections;

public class MaquinaDeEscribirTMP : MonoBehaviour
{
    [Header("Configuración Básica")]
    public float retrasoEntreLetras = 0.1f;
    public float pausaAntesDeBorrar = 1f;
    public float velocidadBorrado = 0.05f;

    [Header("Configuración de Sonido")]
    public AudioClip sonidoEscritura;
    public AudioClip sonidoBorrado;
    [Range(0.1f, 3f)] public float pitchEscritura = 1.2f;
    [Range(0.1f, 3f)] public float pitchBorrado = 0.8f;
    [Range(0f, 1f)] public float volumen = 0.5f;

    [Header("Referencias")]
    public TextMeshProUGUI textoUI;

    private string textoActual = "";
    private bool escribiendo = false;
    private AudioSource audioSource;

    void Start()
    {
        // Inicializar componentes
        textoUI.text = "";
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volumen;
    }

    public void EscribirTextoDesdeBoton(string textoCompleto)
    {
        StopAllCoroutines();
        StartCoroutine(EscribirYBorrarTexto(textoCompleto));
    }

    IEnumerator EscribirYBorrarTexto(string textoCompleto)
    {
        // Fase de Escritura
        escribiendo = true;
        textoActual = "";

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            textoActual += textoCompleto[i];
            textoUI.text = textoActual;
            ReproducirSonido(sonidoEscritura, pitchEscritura);
            yield return new WaitForSeconds(retrasoEntreLetras);
        }

        escribiendo = false;
        yield return new WaitForSeconds(pausaAntesDeBorrar);

        // Fase de Borrado
        while (textoActual.Length > 0)
        {
            textoActual = textoActual.Remove(textoActual.Length - 1);
            textoUI.text = textoActual;
            ReproducirSonido(sonidoBorrado, pitchBorrado);
            yield return new WaitForSeconds(velocidadBorrado);
        }
    }

    void ReproducirSonido(AudioClip clip, float pitch)
    {
        if (clip != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }

    // Método para detener todo inmediatamente
    public void DetenerEfecto()
    {
        StopAllCoroutines();
        textoActual = "";
        textoUI.text = "";
        escribiendo = false;
    }
}