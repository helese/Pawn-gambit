using System.Collections.Generic;
using UnityEngine;

public class BestiarioManager : MonoBehaviour
{
    public static BestiarioManager Instance;
    public HashSet<string> enemigosDerrotados = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LimitarFPS(); // Aplicar límite de FPS al inicializar
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para limitar FPS (60 por defecto)
    public void LimitarFPS(int fps = 60)
    {
        Application.targetFrameRate = fps;
        Debug.Log($"FPS limitados a {fps}");
    }

    public void RegistrarEnemigoDerrotado(string idEnemigo)
    {
        if (!enemigosDerrotados.Contains(idEnemigo))
        {
            enemigosDerrotados.Add(idEnemigo);
            Debug.Log($"Enemigo {idEnemigo} registrado en el bestiario!");
        }
    }
}