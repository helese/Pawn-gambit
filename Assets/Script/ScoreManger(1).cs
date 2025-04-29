using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [System.Serializable]
    public class HighScore
    {
        public string initials;
        public int score;
    }

    public List<HighScore> highScores = new List<HighScore>();
    public int currentScore;

    [Header("Configuración de puntuaciones")]
    public int maxHighScores = 10; // Número máximo de puntuaciones a guardar

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
    }

    public void SaveScore(string initials)
    {
        // Agregar la nueva puntuación a la lista
        highScores.Add(new HighScore { initials = initials, score = currentScore });

        // Ordenar la lista de mayor a menor
        highScores.Sort((a, b) => b.score.CompareTo(a.score));

        // Mantener solo las X puntuaciones más altas
        if (highScores.Count > maxHighScores)
        {
            highScores.RemoveRange(maxHighScores, highScores.Count - maxHighScores);
        }

        // Guardar las puntuaciones en PlayerPrefs
        SaveScores();

        // Reiniciar la puntuación actual
        currentScore = 0;
    }

    void SaveScores()
    {
        // Guardar cada puntuación en PlayerPrefs
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"score{i}_initials", highScores[i].initials);
            PlayerPrefs.SetInt($"score{i}_value", highScores[i].score);
        }
        PlayerPrefs.Save();
    }

    void LoadScores()
    {
        highScores.Clear();

        // Cargar las puntuaciones desde PlayerPrefs
        for (int i = 0; i < maxHighScores; i++)
        {
            if (PlayerPrefs.HasKey($"score{i}_value"))
            {
                highScores.Add(new HighScore
                {
                    initials = PlayerPrefs.GetString($"score{i}_initials", "AAA"),
                    score = PlayerPrefs.GetInt($"score{i}_value", 0)
                });
            }
        }

        // Asegurarse de que la lista esté ordenada al cargar
        highScores.Sort((a, b) => b.score.CompareTo(a.score));
    }
}