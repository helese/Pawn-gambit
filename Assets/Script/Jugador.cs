using UnityEngine;
using System.Collections;

public class Jugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f; // Velocidad de movimiento normal
    public float velocidadDash = 15f; // Velocidad del dash
    public float duracionDash = 0.2f; // Duración del dash
    public float cooldownDash = 1f; // Tiempo de espera entre dashes
    public float velocidadRotacion = 10f; // Velocidad de rotación del jugador

    private Rigidbody rb; // Referencia al Rigidbody
    private Vector3 direccionMovimiento; // Dirección de movimiento
    private bool puedeDash = true; // Indica si el jugador puede hacer dash
    private bool estaDashing = false; // Indica si el jugador está haciendo dash
    private Transform camara; // Referencia a la cámara
    private bool puedeMoverse = true; // Indica si el jugador puede moverse
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtener el componente Rigidbody
        camara = Camera.main.transform; // Obtener la referencia a la cámara principal

        // Suscribirse al evento de destrucción de la TorreRey
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            // Usar el nombre de la clase para acceder al evento estático
            GameManager.OnTorreReyDestruida += DesactivarMovimiento;
        }
    }

    void Update()
    {
        // Solo procesar la entrada si el jugador puede moverse
        if (puedeMoverse)
        {
            // Obtener la entrada del jugador (WASD o flechas)
            float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
            float movimientoVertical = Input.GetAxisRaw("Vertical");

            // Calcular la dirección de movimiento en relación con la cámara
            direccionMovimiento = (camara.forward * movimientoVertical + camara.right * movimientoHorizontal).normalized;
            direccionMovimiento.y = 0; // Ignorar la componente Y para evitar movimientos verticales

            // Verificar si el jugador presiona Shift y puede hacer dash
            if (Input.GetMouseButtonDown(1) && puedeDash)
            {
                StartCoroutine(Dash());
            }
        }
    }

    void FixedUpdate()
    {
        // Mover al jugador solo si no está haciendo dash y puede moverse
        if (!estaDashing && puedeMoverse)
        {
            rb.linearVelocity = direccionMovimiento * velocidadMovimiento;

            // Rotar el jugador hacia la dirección del movimiento
            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }
    }

    // Método para rotar el jugador hacia la dirección del movimiento
    private void RotarHaciaDireccion(Vector3 direccion)
    {
        // Calcular la rotación deseada
        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);

        // Suavizar la rotación hacia la dirección deseada
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
    }

    // Corrutina para realizar el dash
    private IEnumerator Dash()
    {
        puedeDash = false; // Desactivar el dash
        estaDashing = true; // Indicar que el jugador está haciendo dash

        // Aplicar la velocidad del dash en la dirección actual
        rb.linearVelocity = direccionMovimiento * velocidadDash;

        // Esperar la duración del dash
        yield return new WaitForSeconds(duracionDash);

        // Detener el dash y restaurar la velocidad normal
        rb.linearVelocity = Vector3.zero;
        estaDashing = false;

        // Esperar el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(cooldownDash);
        puedeDash = true; // Reactivar el dash
    }

    // Método para desactivar el movimiento del jugador
    private void DesactivarMovimiento()
    {
        puedeMoverse = false;
    }
}