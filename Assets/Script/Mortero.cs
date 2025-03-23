using UnityEngine;

public class Mortero : MonoBehaviour
{
    [Header("Configuración del Mortero")]
    public float cadenciaDisparo = 2f; // Tiempo entre cada disparo
    public GameObject proyectilPrefab; // Prefab del proyectil
    public float alturaMaxima = 10f; // Altura máxima de la trayectoria parabólica
    public float gravedad = -9.81f; // Gravedad para el movimiento parabólico

    [Header("Áreas de Detección")]
    public Vector3[] tamanosCajas; // Tamaños de las cajas de detección
    public Vector3[] rotacionesCajas; // Rotaciones de las cajas de detección
    public Vector3[] desplazamientosCajas; // Desplazamientos de las cajas de detección respecto al mortero
    public string tagEnemigo = "Enemigo"; // Tag del enemigo

    [Header("Material Requerido")]
    public Material materialRequerido; // Material que debe tener la torreta para disparar

    private float tiempoUltimoDisparo; // Tiempo del último disparo
    private Transform objetivo; // Objetivo actual
    private Vector3 posicionObjetivo; // Posición del objetivo (centro de la caja de detección)

    void Update()
    {
        // Verificar si hay enemigos dentro de las áreas de detección
        VerificarEnemigosEnRango();

        if (TieneMaterialCorrecto() == true )
        {
            // Disparar si hay un objetivo y es momento de disparar
            if (objetivo != null && Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time; // Actualizar el tiempo del último disparo
            }
        }

    }

    private void VerificarEnemigosEnRango()
    {
        // Buscar todos los objetos con el tag "Enemigo" en la escena
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

        // Inicialmente no hay objetivo
        objetivo = null;

        // Recorrer todos los enemigos
        foreach (GameObject enemigo in enemigos)
        {
            // Verificar si el enemigo está dentro de alguna de las cajas de detección
            for (int i = 0; i < tamanosCajas.Length; i++)
            {
                Vector3 centroCaja = transform.position + desplazamientosCajas[i];
                if (EstaEnCaja(enemigo.transform.position, centroCaja, tamanosCajas[i], rotacionesCajas[i]))
                {
                    objetivo = enemigo.transform; // Asignar el enemigo como objetivo
                    posicionObjetivo = centroCaja; // Guardar la posición del centro de la caja de detección
                    break; // Salir del bucle si se encuentra un enemigo en rango
                }
            }

            // Si se encontró un objetivo, salir del bucle principal
            if (objetivo != null) break;
        }
    }

    private bool TieneMaterialCorrecto()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.material.name.Replace(" (Instance)", "") == materialRequerido.name;
        }
        return false;
    }

    private bool EstaEnCaja(Vector3 posicionEnemigo, Vector3 centroCaja, Vector3 tamanoCaja, Vector3 rotacionCaja)
    {
        // Crear una matriz de rotación para la caja
        Quaternion rotacion = Quaternion.Euler(rotacionCaja);

        // Convertir la posición del enemigo al espacio local de la caja
        Vector3 posicionLocal = Quaternion.Inverse(rotacion) * (posicionEnemigo - centroCaja);

        // Calcular los límites de la caja en su espacio local
        Vector3 min = -tamanoCaja / 2;
        Vector3 max = tamanoCaja / 2;

        // Verificar si la posición local del enemigo está dentro de los límites de la caja
        return posicionLocal.x >= min.x && posicionLocal.x <= max.x &&
               posicionLocal.y >= min.y && posicionLocal.y <= max.y &&
               posicionLocal.z >= min.z && posicionLocal.z <= max.z;
    }

    private void Disparar()
    {
        if (objetivo != null)
        {
            // Instanciar el proyectil en la posición del mortero
            GameObject proyectil = Instantiate(proyectilPrefab, transform.position, Quaternion.identity);

            // Obtener el componente ProyectilParabolico del proyectil
            ProyectilParabolico proyectilScript = proyectil.GetComponent<ProyectilParabolico>();

            if (proyectilScript != null)
            {
                // Configurar el movimiento parabólico del proyectil hacia el centro de la caja de detección
                proyectilScript.Configurar(transform.position, posicionObjetivo, alturaMaxima, gravedad);
            }
        }
    }

    // Dibujar las áreas de detección en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Dibujar todas las cajas de detección
        for (int i = 0; i < tamanosCajas.Length; i++)
        {
            Vector3 centroCaja = transform.position + desplazamientosCajas[i];
            DibujarCajaGizmo(centroCaja, tamanosCajas[i], rotacionesCajas[i]);
        }
    }

    private void DibujarCajaGizmo(Vector3 centro, Vector3 tamano, Vector3 rotacion)
    {
        // Guardar la matriz de transformación actual
        Matrix4x4 matrizOriginal = Gizmos.matrix;

        // Aplicar la rotación a la matriz de transformación de Gizmos
        Gizmos.matrix = Matrix4x4.TRS(centro, Quaternion.Euler(rotacion), Vector3.one);

        // Dibujar la caja
        Gizmos.DrawWireCube(Vector3.zero, tamano);

        // Restaurar la matriz de transformación original
        Gizmos.matrix = matrizOriginal;
    }
}