using UnityEngine;
using Unity.Cinemachine;

public class MoverCamara : MonoBehaviour
{
    [Header("Configuración de la cámara GameOver")]
    public Vector3 posicionObjetivo; 
    public Vector3 rotacionObjetivo; 
    public float velocidadMovimiento = 5f; 
    public float velocidadRotacion = 2f; 

    private bool moverCamara = false;

    [Header("Configuración de la cámara Mapa")]
    public Vector3 posicionObjetivo1;
    public Vector3 rotacionObjetivo1;
    public float velocidadMovimiento1 = 5f;
    public float velocidadRotacion1 = 2f;

    private bool moverCamara1 = false;

    [Header("Configuración de la cámara Reiniciar")]
    public Vector3 posicionObjetivo2;
    public Vector3 rotacionObjetivo2;
    public float velocidadMovimiento2 = 5f;
    public float velocidadRotacion2 = 2f;
    public GameObject jugador;

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
        if (moverCamara1)
        {
            transform.position = Vector3.Lerp(transform.position, posicionObjetivo1, velocidadMovimiento1 * Time.deltaTime);
            Quaternion rotacionDeseada = Quaternion.Euler(rotacionObjetivo1);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion1 * Time.deltaTime);
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

    public void IniciarMovimiento1()
    {
        // Desactivar el componente CinemachineBrain
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        moverCamara1 = true;
    }

    public void ReiniciarPosicion()
    {
        if (cinemachineBrain != null)
        {
            activarCinemachine();
            //Invoke("activarCinemachine", 2f);
        }
        //moverCamara2 = true;
        moverCamara1 = false;
    }

    void activarCinemachine()
    {
        cinemachineBrain.enabled = true;
    }
}