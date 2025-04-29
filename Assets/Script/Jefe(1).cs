using UnityEngine;

public class Jefe : MonoBehaviour
{
    private GameManager gameManager;

    // Método para inicializar el jefe
    public void Inicializar(GameManager manager)
    {
        gameManager = manager;
    }

    // Método llamado cuando el jefe es destruido
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.NotificarJefeDestruido();
        }
    }
}