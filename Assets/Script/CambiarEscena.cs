using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using System.Collections;
using System.Drawing;

public class CambiarEscena : MonoBehaviour
{
    public GameObject panel;
    public float delaySeconds = 1f;
    public void CambiarNivel(string nombreEscena)
    {
        StartCoroutine(CambioEscena(nombreEscena));
    }
    public void FadeIn()
    {
        panel.SetActive(true);
    }
    IEnumerator CambioEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
        yield return new WaitForSeconds(delaySeconds);
    }
}