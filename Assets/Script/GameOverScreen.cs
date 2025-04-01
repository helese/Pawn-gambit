using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public TMP_InputField initialsInput; // Campo de entrada para las iniciales
    public TextMeshProUGUI scoreText; // Texto para mostrar la puntuaci�n

    void Start()
    {
        // Configurar el campo de iniciales
        initialsInput.characterLimit = 3;
        initialsInput.contentType = TMP_InputField.ContentType.Alphanumeric;

        // Suscribir eventos
        initialsInput.onValueChanged.AddListener(OnInitialsChanged);
        initialsInput.onEndEdit.AddListener(OnInitialsEndEdit);

        // Mostrar la puntuaci�n actual
        if (ScoreManager.Instance != null)
        {
            scoreText.text = $"PUNTUACI�N: {ScoreManager.Instance.currentScore}";
        }
        else
        {
            Debug.LogError("ScoreManager no est� inicializado.");
        }
    }

    private void OnInitialsChanged(string value)
    {
        // Convertir a may�sculas en tiempo real
        if (value != value.ToUpper())
        {
            initialsInput.text = value.ToUpper();
            // Mover el cursor al final
            initialsInput.caretPosition = initialsInput.text.Length;
        }
    }

    private void OnInitialsEndEdit(string value)
    {
        // Rellenar con guiones si no hay 3 caracteres
        if (value.Length < 3)
        {
            string filledInitials = value.PadRight(3, '-');
            initialsInput.text = filledInitials;
        }
    }

    public void SavePlayerScore()
    {
        // Obtener las iniciales (asegurarse de que sean 3 caracteres)
        string initials = initialsInput.text.Length >= 3 ?
                         initialsInput.text :
                         initialsInput.text.PadRight(3, '-');

        // Guardar la puntuaci�n en el ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveScore(initials);
        }
        else
        {
            Debug.LogError("ScoreManager no est� inicializado.");
        }
    }
}