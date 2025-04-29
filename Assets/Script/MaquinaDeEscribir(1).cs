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
    public AudioClip sonidoEscrituraContinuo;
    public AudioClip sonidoBorradoContinuo;
    [Range(0.1f, 3f)] public float pitchEscrituraMin = 0.9f;
    [Range(0.1f, 3f)] public float pitchEscrituraMax = 1.2f;
    [Range(0.1f, 3f)] public float pitchBorradoMin = 0.7f;
    [Range(0.1f, 3f)] public float pitchBorradoMax = 0.9f;
    [Range(0f, 1f)] public float volumen = 0.5f;

    [Header("Configuración de Desvanecimiento")]
    public int letrasParaDesvanecer = 5; // Número de letras antes del final para comenzar a desvanecer

    [Header("Referencias")]
    public TextMeshProUGUI textoUI;

    private string textoActual = "";
    private bool escribiendo = false;
    private AudioSource audioSource;
    private float volumenOriginal;

    void Start()
    {
        // Inicializar componentes
        textoUI.text = "";
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volumen;
        audioSource.loop = true; // Configurar para reproducción continua
        volumenOriginal = volumen;
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

        // Generar pitch aleatorio para esta sesión de escritura
        float pitchAleatorioEscritura = Random.Range(pitchEscrituraMin, pitchEscrituraMax);

        // Iniciar sonido continuo de escritura con pitch aleatorio
        IniciarSonidoContinuo(sonidoEscrituraContinuo, pitchAleatorioEscritura);

        int totalLetras = textoCompleto.Length;

        for (int i = 0; i < totalLetras; i++)
        {
            textoActual += textoCompleto[i];
            textoUI.text = textoActual;

            // Verificar si estamos cerca del final para reducir volumen
            if (i >= totalLetras - letrasParaDesvanecer && totalLetras > letrasParaDesvanecer)
            {
                // Calcular cuánto falta para terminar
                int letrasRestantes = totalLetras - i;
                // Reducir volumen gradualmente (desde volumenOriginal hasta 0)
                float factorVolumen = (float)letrasRestantes / letrasParaDesvanecer;
                audioSource.volume = volumenOriginal * factorVolumen;
            }

            yield return new WaitForSeconds(retrasoEntreLetras);
        }

        // Detener sonido al terminar de escribir
        DetenerSonidoContinuo();
        escribiendo = false;

        yield return new WaitForSeconds(pausaAntesDeBorrar);

        // Fase de Borrado
        // Generar pitch aleatorio para esta sesión de borrado
        float pitchAleatorioBorrado = Random.Range(pitchBorradoMin, pitchBorradoMax);

        // Iniciar sonido continuo de borrado con pitch aleatorio
        IniciarSonidoContinuo(sonidoBorradoContinuo, pitchAleatorioBorrado);

        int totalLetrasParaBorrar = textoActual.Length;

        while (textoActual.Length > 0)
        {
            // Verificar si estamos cerca del final para reducir volumen
            if (textoActual.Length <= letrasParaDesvanecer && totalLetrasParaBorrar > letrasParaDesvanecer)
            {
                // Calcular factor de volumen (desde volumenOriginal hasta 0)
                float factorVolumen = (float)textoActual.Length / letrasParaDesvanecer;
                audioSource.volume = volumenOriginal * factorVolumen;
            }

            textoActual = textoActual.Remove(textoActual.Length - 1);
            textoUI.text = textoActual;
            yield return new WaitForSeconds(velocidadBorrado);
        }

        // Detener sonido al terminar de borrar
        DetenerSonidoContinuo();
    }

    void IniciarSonidoContinuo(AudioClip clip, float pitch)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.volume = volumenOriginal; // Restaurar el volumen original
            audioSource.Play();
        }
    }

    void DetenerSonidoContinuo()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Método para detener todo inmediatamente
    public void DetenerEfecto()
    {
        StopAllCoroutines();
        DetenerSonidoContinuo();
        textoActual = "";
        textoUI.text = "";
        escribiendo = false;
    }
}