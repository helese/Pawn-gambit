using UnityEngine;

public class Torreta : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public float cadenciaDisparo = 1f; // Tiempo entre cada disparo
    public GameObject proyectilPrefab; // Prefab del proyectil

    [Header("Puntos de Disparo")]
    public Transform[] puntosDeDisparo; // Lista de puntos de disparo (direcciones y posiciones)

    [Header("Áreas de Detección")]
    public Vector3 tamanoCaja1 = new Vector3(10f, 5f, 10f); // Tamaño de la primera caja
    public Vector3 rotacionCaja1 = Vector3.zero; // Rotación de la primera caja
    public Vector3 tamanoCaja2 = new Vector3(10f, 5f, 10f); // Tamaño de la segunda caja
    public Vector3 rotacionCaja2 = Vector3.zero; // Rotación de la segunda caja
    public string tagEnemigo = "Enemigo"; // Tag del enemigo

    [Header("Material Requerido")]
    public Material materialRequerido; // Material que debe tener la torreta para disparar

    private float tiempoUltimoDisparo; // Tiempo del último disparo
    private bool enemigoEnRango = false; // Indica si hay un enemigo dentro del área de detección

    public int puntosARecuperar;
    private GameManager gameManager;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // Verificar si el material de la torreta es el correcto
        if (TieneMaterialCorrecto())
        {
            // Verificar si hay enemigos dentro de las áreas de detección
            VerificarEnemigosEnRango();

            // Disparar solo si hay un enemigo en rango y es momento de disparar
            if (enemigoEnRango && Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
            {
                Disparar();
                tiempoUltimoDisparo = Time.time; // Actualizar el tiempo del último disparo
            }
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

    private void VerificarEnemigosEnRango()
    {
        // Buscar todos los objetos con el tag "Enemigo" en la escena
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

        // Inicialmente asumir que no hay enemigos en rango
        enemigoEnRango = false;

        // Recorrer todos los enemigos
        foreach (GameObject enemigo in enemigos)
        {
            // Verificar si el enemigo está dentro de alguna de las cajas
            if (EstaEnCaja(enemigo.transform.position, transform.position, tamanoCaja1, rotacionCaja1) ||
                EstaEnCaja(enemigo.transform.position, transform.position, tamanoCaja2, rotacionCaja2))
            {
                enemigoEnRango = true;
                break; // Salir del bucle si se encuentra al menos un enemigo en rango
            }
        }
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
        // Recorrer todos los puntos de disparo
        foreach (Transform punto in puntosDeDisparo)
        {
            // Instanciar el proyectil en la posición y rotación del punto de disparo
            GameObject proyectil = Instantiate(proyectilPrefab, punto.position, punto.rotation);

            // Obtener el componente Rigidbody del proyectil (si lo tiene)
            Rigidbody rb = proyectil.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Aplicar fuerza al proyectil en la dirección hacia adelante del punto de disparo
                rb.linearVelocity = punto.forward * 10f; // Ajusta la velocidad según sea necesario
            }
        }
    }

    // Dibujar las áreas de detección en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Dibujar la primera caja con rotación
        DibujarCajaGizmo(transform.position, tamanoCaja1, rotacionCaja1);

        // Dibujar la segunda caja con rotación
        DibujarCajaGizmo(transform.position, tamanoCaja2, rotacionCaja2);
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

    private void OnDestroy()
    {
        // Verificar si la torreta tiene el material correcto antes de sumar puntos
        if (TieneMaterialCorrecto())
        {
            gameManager.SumarUnidades(puntosARecuperar); // Sumar unidades al slider
        }
    }
}