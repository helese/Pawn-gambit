using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ControladorEscenario : MonoBehaviour
{
    public enum ModificadorEscenario
    {
        AumentarCadencia,
        ReducirCadencia,
        ReducirCoste,
        AumentarVida,
        AumentarVelocidad,
        ReducirTiempoEspera
    }

    [Header("Configuración General")]
    public BoxCollider colliderEscenario;
    public ModificadorEscenario modificadorActual;

    [Header("Modificadores Enemigos")]
    [Range(0.1f, 2f)] public float porcentajeVidaExtra = 0.5f;
    [Range(0.1f, 2f)] public float porcentajeVelocidadExtra = 0.3f;
    [Range(0.1f, 0.9f)] public float reduccionTiempoEspera = 0.2f;
    public float vidaPorSegundoExtra = 2f;

    [Header("Modificadores Torretas")]
    [Range(0.1f, 0.9f)] public float reduccionCadencia = 0.2f;
    [Range(0.1f, 2f)] public float aumentoCadencia = 0.5f;

    private List<Enemigo> enemigosAfectados = new List<Enemigo>();
    private List<Torreta> torretasAfectadas = new List<Torreta>();

    [Header("Configuración de Texto")]
    public TextMeshProUGUI textoEscenario;
    public float duracionTexto = 3f; // Tiempo que se muestra el texto
    private Coroutine corutinaTexto;

    [Header("Efecto Máquina de Escribir")]
    public float velocidadEscritura = 0.05f; // Tiempo entre caracteres
    public float pausaEntreFrases = 1f; // Tiempo antes de borrar
    private bool textoEnProgreso = false;

    [Header("Configuración de Sonido")]
    public AudioClip sonidoTeclado;
    [Range(0.1f, 3f)] public float pitchEscritura = 1f;
    [Range(0.1f, 3f)] public float pitchBorrado = 0.7f; // Más grave
    private AudioSource audioSource;

    void Start()
    {
        colliderEscenario.isTrigger = true;
        modificadorActual = (ModificadorEscenario)Random.Range(0, System.Enum.GetValues(typeof(ModificadorEscenario)).Length);
        Debug.Log($"Modificador activado: {modificadorActual}");

        if (sonidoTeclado != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = sonidoTeclado;
            audioSource.volume = 0.1f;
        }

        MostrarTextoModificador();
    }

    public void AplicarModificadorANuevoObjeto(Component objeto)
    {
        if (objeto is Enemigo enemigo && EstaEnZona(enemigo.transform.position))
        {
            AplicarEfectoEnemigo(enemigo);
            enemigosAfectados.Add(enemigo);
        }
        else if (objeto is Torreta torreta && EstaEnZona(torreta.transform.position))
        {
            AplicarEfectoTorreta(torreta);
            torretasAfectadas.Add(torreta);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            var enemigo = other.GetComponent<Enemigo>();
            if (enemigo != null && !enemigosAfectados.Contains(enemigo))
            {
                AplicarEfectoEnemigo(enemigo);
                enemigosAfectados.Add(enemigo);
            }
        }
        else if (other.CompareTag("Torreta"))
        {
            var torreta = other.GetComponent<Torreta>();
            if (torreta != null && !torretasAfectadas.Contains(torreta))
            {
                AplicarEfectoTorreta(torreta);
                torretasAfectadas.Add(torreta);
            }
        }
    }

    bool EstaEnZona(Vector3 posicion)
    {
        return colliderEscenario.bounds.Contains(posicion);
    }

    void AplicarEfectoEnemigo(Enemigo enemigo)
    {
        switch (modificadorActual)
        {
            case ModificadorEscenario.AumentarVida:
                int vidaExtra = Mathf.RoundToInt(enemigo.vidaMaxima * porcentajeVidaExtra);
                enemigo.vidaMaxima += vidaExtra;
                enemigo.vidaActual += vidaExtra;
                if (enemigo.sliderVida != null)
                {
                    enemigo.sliderVida.maxValue = enemigo.vidaMaxima;
                    enemigo.sliderVida.value = enemigo.vidaActual;
                }
                break;

            case ModificadorEscenario.AumentarVelocidad:
                enemigo.velocidad *= (1 + porcentajeVelocidadExtra);
                break;

            case ModificadorEscenario.ReducirTiempoEspera:
                enemigo.tiempoDeEspera *= (1 - reduccionTiempoEspera);
                break;
        }
    }

    void AplicarEfectoTorreta(Torreta torreta)
    {
        switch (modificadorActual)
        {
            case ModificadorEscenario.AumentarCadencia:
                torreta.cadenciaDisparo *= (1 + aumentoCadencia);
                Debug.Log($"Torreta {torreta.name} - Cadencia aumentada: {torreta.cadenciaDisparo}");
                break;

            case ModificadorEscenario.ReducirCadencia:
                torreta.cadenciaDisparo *= (1 - reduccionCadencia);
                Debug.Log($"Torreta {torreta.name} - Cadencia reducida: {torreta.cadenciaDisparo}");
                break;

            case ModificadorEscenario.ReducirCoste:
                if (torreta.puntosARecuperar > 1)
                {
                    torreta.puntosARecuperar -= 1;
                    Debug.Log($"Torreta {torreta.name} - Coste reducido: {torreta.puntosARecuperar}");
                }
                break;
        }
    }

    void Update()
    {
        // Limpiar listas de objetos destruidos
        for (int i = enemigosAfectados.Count - 1; i >= 0; i--)
        {
            if (enemigosAfectados[i] == null)
            {
                enemigosAfectados.RemoveAt(i);
            }
        }

        for (int i = torretasAfectadas.Count - 1; i >= 0; i--)
        {
            if (torretasAfectadas[i] == null)
            {
                torretasAfectadas.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (colliderEscenario != null)
        {
            Gizmos.color = new Color(1, 0, 1, 0.3f);
            Gizmos.matrix = colliderEscenario.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, colliderEscenario.size);
        }
    }

    void MostrarTextoModificador()
    {
        if (textoEnProgreso) return;

        string mensaje = ObtenerMensajeModificador();
        StartCoroutine(EfectoMaquinaEscribir(mensaje));
    }
    string ObtenerMensajeModificador() // Cambiado de void a string
    {
        switch (modificadorActual)
        {
            case ModificadorEscenario.AumentarVida:
                return $"Vida enemigos +{porcentajeVidaExtra * 100:F0}%";

            case ModificadorEscenario.AumentarVelocidad:
                return $"Velocidad enemigos +{porcentajeVelocidadExtra * 100:F0}%";

            case ModificadorEscenario.ReducirTiempoEspera:
                return $"Tiempo entre pasos -{reduccionTiempoEspera * 100:F0}%";

            case ModificadorEscenario.AumentarCadencia:
                return $"Cadencia torretas +{aumentoCadencia * 100:F0}%";

            case ModificadorEscenario.ReducirCadencia:
                return $"Cadencia torretas -{reduccionCadencia * 100:F0}%";

            case ModificadorEscenario.ReducirCoste:
                return "Coste torretas -1";

            default:
                return "Modificador activado";
        }
    }

    IEnumerator EfectoMaquinaEscribir(string mensaje)
    {
        textoEnProgreso = true;
        textoEscenario.text = "";

        // ESCRITURA (tono normal)
        foreach (char letra in mensaje.ToCharArray())
        {
            textoEscenario.text += letra;
            ReproducirSonido(pitchEscritura);
            yield return new WaitForSeconds(velocidadEscritura);
        }

        yield return new WaitForSeconds(pausaEntreFrases);

        // BORRADO (tono grave)
        while (textoEscenario.text.Length > 0)
        {
            textoEscenario.text = textoEscenario.text.Substring(0, textoEscenario.text.Length - 1);
            ReproducirSonido(pitchBorrado);
            yield return new WaitForSeconds(velocidadEscritura / 2);
        }

        textoEnProgreso = false;
    }

    void ReproducirSonido(float pitch)
    {
        if (sonidoTeclado != null && audioSource != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(sonidoTeclado);
        }
    }
}