using UnityEngine;

public class VerificarHijos : MonoBehaviour
{
    public void OnTransformChildrenChanged()
    {
        // Verificar la cantidad de hijos en la jerarquía
        int cantidadHijos = transform.childCount;

        // Cambiar el tag según la cantidad de hijos
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