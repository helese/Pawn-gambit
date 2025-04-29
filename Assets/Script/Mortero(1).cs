using UnityEngine;

public class Mortero : MonoBehaviour
{
    [Header("Configuraci�n del Mortero")]
    public float cadenciaDisparo = 2f; // Tiempo entre cada disparo
    public GameObject proyectilPrefab; // Prefab del proyectil
    public float alturaMaxima = 10f; // Altura m�xima de la trayectoria parab�lica
    public float gravedad = -9.81f; // Gravedad para el movimiento parab�lico

    [Header("�reas de Detecci�n")]
    public Vector3[] tamanosCajas; // Tama�os de las cajas de detecci�n
    public Vector3[] rotacionesCajas; // Rotaciones de las cajas de detecci�n
    public Vector3[] desplazamientosCajas; // Desplazamientos de las cajas de detecci�n respecto al mortero
    public string tagEnemigo = "Enemigo"; // Tag del enemigo

    [Header("Material Requerido")]
    public Material materialRequerido; // Material que debe tener la torreta para disparar

    private float tiempoUltimoDisparo; // Tiempo del �ltimo disparo
    private Transform objetivo; // Objetivo actual
    private Vector3 posicionObjetivo; // Posici�n del objetivo (centro de la caja de detecci�n)

    void Update()
    {
        // Verificar si hay enemigos dentro de las �reas de detecci�n
        VerificarEnemigosEnRango();

        if (TieneMaterialCorrecto() == true )
        {
            // Disparar si hay un objetivo y es momento de disparar
            if (objetivo != null && Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time; // Actualizar el tiempo del �ltimo disparo
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
            // Verificar si el enemigo est� dentro de alguna de las cajas de detecci�n
            for (int i = 0; i < tamanosCajas.Length; i++)
            {
                Vector3 centroCaja = transform.position + desplazamientosCajas[i];
                if (EstaEnCaja(enemigo.transform.position, centroCaja, tamanosCajas[i], rotacionesCajas[i]))
                {
                    objetivo = enemigo.transform; // Asignar el enemigo como objetivo
                    posicionObjetivo = centroCaja; // Guardar la posici�n del centro de la caja de detecci�n
                    break; // Salir del bucle si se encuentra un enemigo en rango
                }
            }

            // Si se encontr� un objetivo, salir del bucle principal
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
        // Crear una matriz de rotaci�n para la caja
        Quaternion rotacion = Quaternion.Euler(rotacionCaja);

        // Convertir la posici�n del enemigo al espacio local de la caja
        Vector3 posicionLocal = Quaternion.Inverse(rotacion) * (posicionEnemigo - centroCaja);

        // Calcular los l�mites de la caja en su espacio local
        Vector3 min = -tamanoCaja / 2;
        Vector3 max = tamanoCaja / 2;

        // Verificar si la posici�n local del enemigo est� dentro de los l�mites de la caja
        return posicionLocal.x >= min.x && posicionLocal.x <= max.x &&
               posicionLocal.y >= min.y && posicionLocal.y <= max.y &&
               posicionLocal.z >= min.z && posicionLocal.z <= max.z;
    }

    private void Disparar()
    {
        if (objetivo != null)
        {
            // Instanciar el proyectil en la posici�n del mortero
            GameObject proyectil = Instantiate(proyectilPrefab, transform.position, Quaternion.identity);

            // Obtener el componente ProyectilParabolico del proyectil
            ProyectilParabolico proyectilScript = proyectil.GetComponent<ProyectilParabolico>();

            if (proyectilScript != null)
            {
                // Configurar el movimiento parab�lico del proyectil hacia el centro de la caja de detecci�n
                proyectilScript.Configurar(transform.position, posicionObjetivo, alturaMaxima, gravedad);
            }
        }
    }

    // Dibujar las �reas de detecci�n en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Dibujar todas las cajas de detecci�n
        for (int i = 0; i < tamanosCajas.Length; i++)
        {
            Vector3 centroCaja = transform.position + desplazamientosCajas[i];
            DibujarCajaGizmo(centroCaja, tamanosCajas[i], rotacionesCajas[i]);
        }
    }

    private void DibujarCajaGizmo(Vector3 centro, Vector3 tamano, Vector3 rotacion)
    {
        // Guardar la matriz de transformaci�n actual
        Matrix4x4 matrizOriginal = Gizmos.matrix;

        // Aplicar la rotaci�n a la matriz de transformaci�n de Gizmos
        Gizmos.matrix = Matrix4x4.TRS(centro, Quaternion.Euler(rotacion), Vector3.one);

        // Dibujar la caja
        Gizmos.DrawWireCube(Vector3.zero, tamano);

        // Restaurar la matriz de transformaci�n original
        Gizmos.matrix = matrizOriginal;
    }
}