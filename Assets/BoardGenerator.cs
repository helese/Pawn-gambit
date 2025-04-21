using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [Header("Configuración del Tablero")]
    public GameObject cellPrefab;
    public int boardWidth = 5;
    public int boardHeight = 5;
    public float cellSpacing = 1.2f;

    [ContextMenu("Generar Tablero")]
    public void GenerateBoard()
    {
        // Eliminar celdas existentes
        ClearBoard();

        // Crear nuevas celdas
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                // Calcular posición
                Vector3 position = new Vector3(
                    x * cellSpacing,
                    0f,
                    y * cellSpacing
                );

                // Instanciar celda
                GameObject newCell = Instantiate(
                    cellPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );

                newCell.name = $"Cell_{x}_{y}";
            }
        }
    }

    [ContextMenu("Limpiar Tablero")]
    public void ClearBoard()
    {
        // Destruir todos los hijos mientras haya
        while (transform.childCount > 0)
        {
            // Necesario usar DestroyImmediate para editor
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}