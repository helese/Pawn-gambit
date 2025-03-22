using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MaquinaDeEscribirTMP : MonoBehaviour
{
    [Header("Configuración")]
    public float retrasoEntreLetras = 0.1f; // Tiempo entre cada letra

    [Header("Referencias")]
    public TextMeshProUGUI textoUI; // Referencia al componente TextMeshProUGUI

    private string textoActual = ""; // Texto que se muestra actualmente
    private bool escribiendo = false; // Indica si el texto se está escribiendo

    void Start()
    {
        // Inicializar el texto vacío
        textoUI.text = "";
    }

    // Método para asignar a los botones
    public void EscribirTextoDesdeBoton(string textoCompleto)
    {
        // Detener la corrutina si está en progreso
        StopAllCoroutines();

        // Borrar el texto actual
        textoActual = "";
        textoUI.text = textoActual;

        // Iniciar la escritura del nuevo texto
        StartCoroutine(EscribirTexto(textoCompleto));
    }

    IEnumerator EscribirTexto(string textoCompleto)
    {
        escribiendo = true; // Indicar que se está escribiendo

        // Recorrer el texto letra por letra
        for (int i = 0; i < textoCompleto.Length; i++)
        {
            textoActual += textoCompleto[i];
            textoUI.text = textoActual;
            yield return new WaitForSeconds(retrasoEntreLetras);
        }

        escribiendo = false; // Indicar que se terminó de escribir
    }
}