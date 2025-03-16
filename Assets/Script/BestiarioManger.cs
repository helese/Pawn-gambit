using System.Collections.Generic;
using UnityEngine;

public class BestiarioManager : MonoBehaviour
{
    public static BestiarioManager Instance;

    // Lista de IDs de enemigos derrotados
    public HashSet<string> enemigosDerrotados = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para registrar enemigos derrotados
    public void RegistrarEnemigoDerrotado(string idEnemigo)
    {
        if (!enemigosDerrotados.Contains(idEnemigo))
        {
            enemigosDerrotados.Add(idEnemigo);
            Debug.Log($"Enemigo {idEnemigo} registrado en el bestiario!");
        }
    }
}