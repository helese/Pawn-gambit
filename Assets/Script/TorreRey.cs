using UnityEngine;
using UnityEngine.UI; // Necesario para usar el Slider

public class TorreRey : MonoBehaviour
{
    [Header("Configuraci�n")]
    public int vidaMaxima = 100; // Vida m�xima de la TorreRey (ajustable desde el Inspector)
    private int vidaActual; // Vida actual de la TorreRey

    [Header("UI")]
    public Slider sliderVida; // Referencia al Slider que muestra la vida

    private bool estaVivo = true;

    // Referencia al GameManager
    private GameManager gameManager;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Inicializar la vida de la TorreRey
        vidaActual = vidaMaxima;

        estaVivo = true;

        // Configurar el Slider
        if (sliderVida != null)
        {
            sliderVida.maxValue = vidaMaxima; // Establecer el valor m�ximo del Slider
            sliderVida.value = vidaActual; // Establecer el valor actual del Slider
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si la colisi�n es con un enemigo
        if (collision.gameObject.CompareTag("Enemigo"))
        {
            // Obtener el componente Enemigo
            Enemigo enemigo = collision.gameObject.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                // Recibir da�o de acuerdo al da�o del enemigo
                RecibirDa�o(enemigo.ObtenerDa�oATorreRey());
            }

            // Destruir el enemigo
            Destroy(collision.gameObject);
        }
    }

    // M�todo para recibir da�o
    public void RecibirDa�o(int da�o)
    {
        vidaActual -= da�o; // Reducir la vida actual

        // Asegurarse de que la vida no sea menor que 0
        vidaActual = Mathf.Max(vidaActual, 0);

        // Actualizar el Slider
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual;
        }

        // Verificar si la TorreRey ha muerto
        if (vidaActual <= 0)
        {
            estaVivo = false;
            Morir();
        }
    }

    // M�todo para manejar la muerte de la TorreRey
    private void Morir()
    {
        // Notificar al GameManager que la TorreRey ha sido destruida
        if (gameManager != null && estaVivo == false)
        {
            gameManager.NotificarTorreReyDestruida();
        }

        // Destruir todos los objetos con el tag "Enemigo"
        DestruirTodosLosEnemigos();

        // Destruir la TorreRey
        Destroy(gameObject);
        Debug.Log("�La TorreRey ha sido destruida!");
    }

    // M�todo para destruir todos los objetos con el tag "Enemigo"
    private void DestruirTodosLosEnemigos()
    {
        // Buscar todos los objetos con el tag "Enemigo"
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");

        // Destruir cada uno de los enemigos
        foreach (GameObject enemigo in enemigos)
        {
            Destroy(enemigo);
        }

        Debug.Log($"Se han destruido {enemigos.Length} enemigos.");
    }

    // M�todo para recuperar toda la vida de la TorreRey
    public void RecuperarVidaCompleta()
    {
        vidaActual = vidaMaxima;

        // Actualizar el Slider
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual;
        }

        Debug.Log("Vida de la TorreRey recuperada al 100%.");
    }
}