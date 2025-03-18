using UnityEngine;

public class DestruirObjeto : MonoBehaviour
{
    void Start()
    {
        // Verificar si hay otro objeto en la misma posición
        VerificarYDestruir();
    }

    private void VerificarYDestruir()
    {
        // Obtener todos los objetos en la escena
        GameObject[] todosLosObjetos = GameObject.FindObjectsOfType<GameObject>();

        // Recorrer todos los objetos
        foreach (GameObject otroObjeto in todosLosObjetos)
        {
            // Ignorarse a sí mismo
            if (otroObjeto != gameObject)
            {
                // Verificar si el otro objeto está en la misma posición
                if (Vector3.Distance(transform.position, otroObjeto.transform.position) < 0.01f)
                {
                    // Destruir este objeto
                    Destroy(gameObject);
                    return; // Salir del método después de destruir el objeto
                }
            }
        }
    }
}