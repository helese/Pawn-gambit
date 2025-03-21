using UnityEngine;
using UnityEngine.UI;

public class ControladorBotones : MonoBehaviour
{
    [Header("Botones y Costes")]
    public Button[] botones; // Array de botones
    public int[] costes; // Array de costes correspondientes a cada bot�n

    private GameManager gameManager; // Referencia al GameManager

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No se encontr� el GameManager en la escena.");
            return;
        }

        // Verificar que el n�mero de botones y costes coincida
        if (botones.Length != costes.Length)
        {
            Debug.LogError("El n�mero de botones y costes no coincide.");
            return;
        }

        // Asignar eventos a los botones
        for (int i = 0; i < botones.Length; i++)
        {
            int index = i; // Capturar el �ndice para el evento
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

    // M�todo para manejar el clic en un bot�n
    private void OnBotonClic(int indiceBoton)
    {
        // Obtener el coste del bot�n
        int coste = costes[indiceBoton];

        // Verificar si el coste es menor o igual al valor del Slider en el GameManager
        if (gameManager != null && coste <= gameManager.ObtenerValorSlider())
        {
            // Restar el coste al valor del Slider
            gameManager.RestarUnidades(coste);
            Debug.Log($"Bot�n {indiceBoton + 1} pulsado. Coste: {coste}");
        }
        else
        {
            Debug.LogWarning($"Coste del bot�n {indiceBoton + 1} ({coste}) es mayor que el valor actual del Slider.");
        }
    }

    // M�todo para actualizar el estado de los botones
    private void ActualizarEstadoBotones()
    {
        if (gameManager == null) return;

        // Obtener el valor actual del Slider del GameManager
        int valorSlider = gameManager.ObtenerValorSlider();

        // Recorrer todos los botones
        for (int i = 0; i < botones.Length; i++)
        {
            // Deshabilitar el bot�n si su coste es mayor que el valor del Slider
            botones[i].interactable = (costes[i] <= valorSlider);
        }
    }
}