using UnityEngine;

public class DestruirObjeto : MonoBehaviour
{
    [SerializeField] private LayerMask layerCasillas; // Asigna la layer en el Inspector

    void Start()
    {
        VerificarYDestruir();
    }

    private void VerificarYDestruir()
    {
        Collider[] objetosCercanos = Physics.OverlapSphere(
            transform.position,
            0.01f,
            layerCasillas
        );

        foreach (Collider col in objetosCercanos)
        {
            // Verificar que no sea el mismo objeto
            if (col.gameObject != gameObject)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}