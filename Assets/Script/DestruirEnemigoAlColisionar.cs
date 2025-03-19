using UnityEngine;

public class DestruirEnemigoAlColisionar : MonoBehaviour
{
    public int daño = 10; // Cantidad de daño que se aplica al enemigo (ajustable desde el Inspector)

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró en el trigger tiene el tag "Enemigo"
        if (other.CompareTag("Enemigo"))
        {
            // Obtener el componente Enemigo del objeto colisionado
            Enemigo enemigo = other.GetComponent<Enemigo>();

            // Verificar si el componente Enemigo existe
            if (enemigo != null)
            {
                // Aplicar daño al enemigo
                enemigo.RecibirDaño(daño);
            }
        }
    }
}