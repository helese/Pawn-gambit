using UnityEngine;
using Unity.Cinemachine;

public class MoverCamara : MonoBehaviour
{
    [Header("Configuración de la cámara")]
    public Vector3 posicionObjetivo; 
    public Vector3 rotacionObjetivo; 
    public float velocidadMovimiento = 5f; 
    public float velocidadRotacion = 2f; 

    private bool moverCamara = false; 
    private CinemachineBrain cinemachineBrain; 

    void Start()
    {
        cinemachineBrain = GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (moverCamara)
        {
            transform.position = Vector3.Lerp(transform.position, posicionObjetivo, velocidadMovimiento * Time.deltaTime);
            Quaternion rotacionDeseada = Quaternion.Euler(rotacionObjetivo);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
        }
    }
    public void IniciarMovimiento()
    {
        // Desactivar el componente CinemachineBrain
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        moverCamara = true;
    }
}