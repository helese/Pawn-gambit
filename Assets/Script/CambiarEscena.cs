using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class CambiarEscena : MonoBehaviour
{
    public void CambiarNivel(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}