using UnityEngine;

public class DestruirSiNoHayCasilla : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float radioDeteccion = 1f; // Radio de detecci�n para los objetos
    public string[] tagsPermitidos = { "CasillaInteractuable" }; // Tags que evitan la destrucci�n

    void Start()
    {
        // Verificar si hay alg�n objeto con los tags permitidos en el radio de detecci�n
        if (!HayObjetosPermitidos())
        {
            // Si no hay objetos permitidos, destruir este objeto
            Destroy(gameObject);
        }
    }

    private bool HayObjetosPermitidos()
    {
        // Usar Physics.OverlapSphere para detectar objetos en el radio de detecci�n
        Collider[] colliders = Physics.OverlapSphere(transform.position, radioDeteccion);

        // Recorrer todos los colliders detectados
        foreach (Collider collider in colliders)
        {
            // Verificar si el collider tiene alguno de los tags permitidos
            foreach (string tag in tagsPermitidos)
            {
                if (collider.CompareTag(tag))
                {
                    return true; // Hay un objeto con un tag permitido
                }
            }
        }

        return false; // No hay objetos con tags permitidos
    }

    // Dibujar el radio de detecci�n en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}