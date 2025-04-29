using UnityEngine;

public class DesactivarTrasSegundo : MonoBehaviour
{
    void Start()
    {
        Invoke("DesactivarObjeto", 1f); // Llama a la funci�n despu�s de 1 segundo
    }

    void DesactivarObjeto()
    {
        gameObject.SetActive(false); // Desactiva este GameObject
    }
}