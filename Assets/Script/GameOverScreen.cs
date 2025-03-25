using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    public TMP_InputField initialsInput; // Campo de entrada para las iniciales
    public TextMeshProUGUI scoreText; // Texto para mostrar la puntuaci�n

    void Start()
    {
        // Limitar el campo de iniciales a 3 caracteres y solo letras may�sculas
        initialsInput.characterLimit = 3;
        initialsInput.contentType = TMP_InputField.ContentType.Alphanumeric;

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

    public void SavePlayerScore()
    {
        // Obtener las iniciales (asegurarse de que sean 3 caracteres)
        string initials = initialsInput.text.ToUpper().Substring(0, Mathf.Min(3, initialsInput.text.Length));

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