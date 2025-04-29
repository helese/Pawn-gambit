using UnityEngine;

public class DetectorColision : MonoBehaviour
{
    public DetectorCasillas padre;
    public string direccion;

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto está en la layer CasillasAjedrez
        if (other.gameObject.layer == LayerMask.NameToLayer("CasillasAjedrez"))
        {
            Debug.Log("Objeto en layer CasillasAjedrez entró en collider " + direccion);
            if (padre != null)
            {
                padre.RegistrarColision(direccion, true);
            }
            else
            {
                Debug.LogError("Padre no asignado en DetectorColision " + direccion);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto está en la layer CasillasAjedrez
        if (other.gameObject.layer == LayerMask.NameToLayer("CasillasAjedrez"))
        {
            Debug.Log("Objeto en layer CasillasAjedrez salió de collider " + direccion);
            if (padre != null)
            {
                padre.RegistrarColision(direccion, false);
            }
            else
            {
                Debug.LogError("Padre no asignado en DetectorColision " + direccion);
            }
        }
    }
}