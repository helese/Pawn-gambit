using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteAnim : MonoBehaviour
{
    [Header("Sprites para la Animaci�n")]
    public List<Sprite> sprites; // Lista de sprites para la animaci�n

    [Header("Configuraci�n de la Animaci�n")]
    public float frameRate = 0.1f; // Tiempo entre cada frame
    private int currentFrame = 0; // Frame actual
    private Image imageComponent; // Componente Image de la UI

    void Start()
    {
        // Obtener el componente Image
        imageComponent = GetComponent<Image>();

        // Verificar si hay sprites asignados
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning("No se han asignado sprites para la animaci�n.");
            return;
        }

        // Iniciar la animaci�n
        StartCoroutine(PlayAnimation());
    }

    System.Collections.IEnumerator PlayAnimation()
    {
        while (true)
        {
            // Cambiar el sprite actual
            imageComponent.sprite = sprites[currentFrame];

            // Avanzar al siguiente frame
            currentFrame = (currentFrame + 1) % sprites.Count;

            // Esperar antes de cambiar al siguiente frame
            yield return new WaitForSeconds(frameRate);
        }
    }
}