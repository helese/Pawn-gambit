using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [System.Serializable]
    public class ScoreEntry
    {
        public GameObject panel; // Panel que contiene los textos
        public TextMeshProUGUI nameText; // Texto para el nombre
        public TextMeshProUGUI scoreText; // Texto para la puntuaci�n
    }

    [Header("Configuraci�n de la tabla")]
    public ScoreEntry[] scoreEntries; // Array de entradas de puntuaci�n

    void Start()
    {
        // Verificar que el ScoreManager est� disponible
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManager no est� inicializado.");
            return;
        }

        // Mostrar las puntuaciones en los paneles
        DisplayScores();
    }

    void DisplayScores()
    {
        // Obtener las puntuaciones del ScoreManager
        var highScores = ScoreManager.Instance.highScores;

        // Recorrer las entradas de la tabla
        for (int i = 0; i < scoreEntries.Length; i++)
        {
            // Verificar si hay una puntuaci�n para esta posici�n
            if (i < highScores.Count)
            {
                // Mostrar el nombre y la puntuaci�n
                scoreEntries[i].nameText.text = highScores[i].initials;
                scoreEntries[i].scoreText.text = highScores[i].score.ToString();
            }
            else
            {
                // Si no hay puntuaci�n, mostrar valores por defecto
                scoreEntries[i].nameText.text = "---";
                scoreEntries[i].scoreText.text = "0";
            }

            // Activar el panel (por si estaba desactivado)
            if (scoreEntries[i].panel != null)
            {
                scoreEntries[i].panel.SetActive(true);
            }
        }
    }
}