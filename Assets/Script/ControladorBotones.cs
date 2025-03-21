using UnityEngine;
using UnityEngine.UI;

public class ControladorBotones : MonoBehaviour
{
    [Header("Botones y Costes")]
    public Button[] botones; // Array de botones
    public int[] costes; // Array de costes correspondientes a cada botón

    private GameManager gameManager; // Referencia al GameManager

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No se encontró el GameManager en la escena.");
            return;
        }

        // Verificar que el número de botones y costes coincida
        if (botones.Length != costes.Length)
        {
            Debug.LogError("El número de botones y costes no coincide.");
            return;
        }

        // Asignar eventos a los botones
        for (int i = 0; i < botones.Length; i++)
        {
            int index = i; // Capturar el índice para el evento
            botones[i].onClick.AddListener(() => OnBotonClic(index));
        }

        // Actualizar el estado de los botones al inicio
        ActualizarEstadoBotones();
    }

    void Update()
    {
        // Actualizar el estado de los botones en cada frame
        ActualizarEstadoBotones();
    }

    // Método para manejar el clic en un botón
    private void OnBotonClic(int indiceBoton)
    {
        // Obtener el coste del botón
        int coste = costes[indiceBoton];

        // Verificar si el coste es menor o igual al valor del Slider en el GameManager
        if (gameManager != null && coste <= gameManager.ObtenerValorSlider())
        {
            // Restar el coste al valor del Slider
            gameManager.RestarUnidades(coste);
            Debug.Log($"Botón {indiceBoton + 1} pulsado. Coste: {coste}");
        }
        else
        {
            Debug.LogWarning($"Coste del botón {indiceBoton + 1} ({coste}) es mayor que el valor actual del Slider.");
        }
    }

    // Método para actualizar el estado de los botones
    private void ActualizarEstadoBotones()
    {
        if (gameManager == null) return;

        // Obtener el valor actual del Slider del GameManager
        int valorSlider = gameManager.ObtenerValorSlider();

        // Recorrer todos los botones
        for (int i = 0; i < botones.Length; i++)
        {
            // Deshabilitar el botón si su coste es mayor que el valor del Slider
            botones[i].interactable = (costes[i] <= valorSlider);
        }
    }
}