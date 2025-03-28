using UnityEngine;
using System.Collections.Generic;

public class ControladorEscenario : MonoBehaviour
{
    public enum ModificadorEscenario
    {
        AumentarCadencia,
        ReducirCadencia,
        ReducirCoste,
        AumentarVida,
        AumentarVelocidad,
        ReducirTiempoEspera,
        ActivarRegeneracion
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

    void Start()
    {
        colliderEscenario.isTrigger = true;
        modificadorActual = (ModificadorEscenario)Random.Range(0, System.Enum.GetValues(typeof(ModificadorEscenario)).Length);
        Debug.Log($"Modificador activado: {modificadorActual}");
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

            case ModificadorEscenario.ActivarRegeneracion:
                if (!enemigo.regeneraVida) enemigo.regeneraVida = true;
                enemigo.vidaPorSegundo += vidaPorSegundoExtra;
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
}