using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControladorBotones : MonoBehaviour
{
    [Header("Botones y Costes")]
    public Button[] botones; // Array de botones
    public int[] costes; // Array de costes correspondientes a cada bot�n
    public TextMeshProUGUI[] textosCostes; // Array de TextMeshProUGUI para mostrar costes

    private GameManager gameManager;

    void Start()
    {
        // Obtener la referencia al GameManager
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No se encontr� el GameManager en la escena.");
            return;
        }

        // Verificar coincidencia de arrays
        if (botones.Length != costes.Length || botones.Length != textosCostes.Length)
        {
            Debug.LogError("El n�mero de botones, costes y textos no coincide.");
            return;
        }

        // Asignar texto de costes y eventos
        for (int i = 0; i < botones.Length; i++)
        {
            // Mostrar el coste en el TextMeshPro correspondiente
            textosCostes[i].text = costes[i].ToString();

            // Asignar evento al bot�n
            int index = i;
            botones[i].onClick.AddListener(() => OnBotonClic(index));
        }

        ActualizarEstadoBotones();
    }

    void Update()
    {
        ActualizarEstadoBotones();
    }

    private void OnBotonClic(int indiceBoton)
    {
        int coste = costes[indiceBoton];

        if (gameManager != null && coste <= gameManager.ObtenerValorSlider())
        {
            gameManager.RestarUnidades(coste);
            Debug.Log($"Bot�n {indiceBoton + 1} pulsado. Coste: {coste}");

            // Actualizar visualizaci�n (opcional)
            textosCostes[indiceBoton].text = coste.ToString();
        }
        else
        {
            Debug.LogWarning($"Coste del bot�n {indiceBoton + 1} ({coste}) es mayor que el valor actual del Slider.");
        }
    }

    private void ActualizarEstadoBotones()
    {
        if (gameManager == null) return;

        int valorSlider = gameManager.ObtenerValorSlider();

        for (int i = 0; i < botones.Length; i++)
        {
            bool puedeComprar = costes[i] <= valorSlider;
            botones[i].interactable = puedeComprar;

            // Cambiar color del texto seg�n disponibilidad (opcional)
            textosCostes[i].color = puedeComprar ? Color.white : Color.red;
        }
    }
}