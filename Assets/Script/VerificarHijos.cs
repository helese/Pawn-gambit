using UnityEngine;

public class VerificarHijos : MonoBehaviour
{
    public void OnTransformChildrenChanged()
    {
        // Verificar la cantidad de hijos en la jerarqu�a
        int cantidadHijos = transform.childCount;

        // Cambiar el tag seg�n la cantidad de hijos
        if (cantidadHijos >= 2)
        {
            gameObject.tag = "ObjetoInteractuado";
        }
        else
        {
            gameObject.tag = "ObjetoInteractuable";
        }
    }
}