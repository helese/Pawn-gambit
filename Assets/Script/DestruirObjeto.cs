using UnityEngine;

public class DestruirObjeto : MonoBehaviour
{
    void Start()
    {
        // Verificar si hay otro objeto en la misma posici�n
        VerificarYDestruir();
    }

    private void VerificarYDestruir()
    {
        // Obtener todos los objetos en la escena
        GameObject[] todosLosObjetos = GameObject.FindObjectsOfType<GameObject>();

        // Recorrer todos los objetos
        foreach (GameObject otroObjeto in todosLosObjetos)
        {
            // Ignorarse a s� mismo
            if (otroObjeto != gameObject)
            {
                // Verificar si el otro objeto est� en la misma posici�n
                if (Vector3.Distance(transform.position, otroObjeto.transform.position) < 0.01f)
                {
                    // Destruir este objeto
                    Destroy(gameObject);
                    return; // Salir del m�todo despu�s de destruir el objeto
                }
            }
        }
    }
}