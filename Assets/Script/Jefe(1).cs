using UnityEngine;

public class Jefe : MonoBehaviour
{
    private GameManager gameManager;

    // M�todo para inicializar el jefe
    public void Inicializar(GameManager manager)
    {
        gameManager = manager;
    }

    // M�todo llamado cuando el jefe es destruido
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.NotificarJefeDestruido();
        }
    }
}