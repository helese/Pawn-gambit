using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteAnim : MonoBehaviour
{
    [Header("Sprites para la Animación")]
    public List<Sprite> sprites; // Lista de sprites para la animación

    [Header("Configuración de la Animación")]
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
            Debug.LogWarning("No se han asignado sprites para la animación.");
            return;
        }

        // Iniciar la animación
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