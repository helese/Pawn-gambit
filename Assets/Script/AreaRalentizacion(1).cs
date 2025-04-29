using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AreaRalentizacion : MonoBehaviour
{
    [Header("Configuración de Ralentización")]
    [Range(0, 100)]
    [Tooltip("Porcentaje de reducción de velocidad (0-100%)")]
    public float porcentajeReduccionVelocidad = 50f;

    [Range(0, 500)]
    [Tooltip("Porcentaje de aumento del tiempo de espera (0-500%)")]
    public float porcentajeAumentoTiempoEspera = 100f;

    [Header("Configuración Visual")]
    [Tooltip("Color del área para visualización")]
    public Color colorArea = new Color(0, 0.5f, 1f, 0.3f);

    [Tooltip("Mostrar efectos visuales en los enemigos afectados")]
    public bool mostrarEfectoVisual = true;

    [Tooltip("Color para el slider de vida de enemigos afectados")]
    public Color colorSliderAfectado = new Color(0, 0.5f, 1f);

    [Header("Depuración")]
    public bool mostrarMensajesDebug = true;

    // Diccionario para rastrear los enemigos que están en esta área específica
    private HashSet<int> enemigosEnEstaArea = new HashSet<int>();

    // Diccionario estático para rastrear todos los enemigos y sus valores originales
    private static Dictionary<int, EnemySlowdownData> enemigosAfectadosGlobal =
        new Dictionary<int, EnemySlowdownData>();

    // Clase para almacenar los datos de ralentización de un enemigo
    private class EnemySlowdownData
    {
        public float velocidadOriginal;
        public float tiempoEsperaOriginal;
        public Enemigo enemigo;
        public int contadorAreas; // Contador de áreas que afectan a este enemigo
        public float maxPorcentajeReduccion; // Mayor porcentaje de ralentización aplicado
        public float maxPorcentajeAumentoEspera; // Mayor porcentaje de aumento de espera aplicado

        public EnemySlowdownData(Enemigo enemigo)
        {
            this.enemigo = enemigo;
            this.velocidadOriginal = enemigo.velocidad;
            this.tiempoEsperaOriginal = enemigo.tiempoDeEspera;
            this.contadorAreas = 0;
            this.maxPorcentajeReduccion = 0f;
            this.maxPorcentajeAumentoEspera = 0f;
        }
    }

    private void Start()
    {
        // Asegurar que el collider sea trigger
        Collider collider = GetComponent<Collider>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log("El collider se ha configurado como trigger automáticamente");
        }

        // Iniciar la corrutina para verificar continuamente los enemigos dentro del área
        StartCoroutine(VerificarEnemigosEnArea());
    }

    // Corrutina para verificar continuamente los enemigos en el área
    private IEnumerator VerificarEnemigosEnArea()
    {
        while (true)
        {
            // Encontrar todos los colliders en el área
            Collider[] collidersEnArea = Physics.OverlapBox(
                transform.position + ((BoxCollider)GetComponent<Collider>()).center,
                ((BoxCollider)GetComponent<Collider>()).size / 2,
                transform.rotation);

            // Verificar cada collider
            HashSet<int> enemigosActuales = new HashSet<int>();

            foreach (Collider collider in collidersEnArea)
            {
                if (collider.CompareTag("Enemigo"))
                {
                    Enemigo enemigo = collider.GetComponent<Enemigo>();
                    if (enemigo != null)
                    {
                        int id = enemigo.GetInstanceID();
                        enemigosActuales.Add(id);

                        // Si es la primera vez que este enemigo entra en esta área específica
                        if (!enemigosEnEstaArea.Contains(id))
                        {
                            if (mostrarMensajesDebug)
                                Debug.Log($"Enemigo entró al área: {enemigo.name}, ID: {id}");

                            enemigosEnEstaArea.Add(id);

                            // Registrar el enemigo en el sistema global si es nuevo
                            if (!enemigosAfectadosGlobal.ContainsKey(id))
                            {
                                enemigosAfectadosGlobal[id] = new EnemySlowdownData(enemigo);
                            }

                            // Incrementar el contador de áreas que afectan a este enemigo
                            enemigosAfectadosGlobal[id].contadorAreas++;

                            // Actualizar el efecto máximo si este área tiene un efecto más fuerte
                            if (porcentajeReduccionVelocidad > enemigosAfectadosGlobal[id].maxPorcentajeReduccion)
                            {
                                enemigosAfectadosGlobal[id].maxPorcentajeReduccion = porcentajeReduccionVelocidad;
                            }

                            if (porcentajeAumentoTiempoEspera > enemigosAfectadosGlobal[id].maxPorcentajeAumentoEspera)
                            {
                                enemigosAfectadosGlobal[id].maxPorcentajeAumentoEspera = porcentajeAumentoTiempoEspera;
                            }

                            // Aplicar el efecto máximo
                            AplicarEfectoMaximo(enemigosAfectadosGlobal[id]);
                        }
                    }
                }
            }

            // Verificar si algún enemigo salió de esta área específica
            List<int> enemigosQueSalieron = new List<int>();
            foreach (int id in enemigosEnEstaArea)
            {
                if (!enemigosActuales.Contains(id))
                {
                    enemigosQueSalieron.Add(id);

                    if (enemigosAfectadosGlobal.TryGetValue(id, out EnemySlowdownData datos))
                    {
                        if (mostrarMensajesDebug && datos.enemigo != null)
                            Debug.Log($"Enemigo salió del área: {datos.enemigo.name}, ID: {id}");

                        // Decrementar el contador de áreas
                        datos.contadorAreas--;

                        // Si este era el área con el efecto más fuerte, necesitamos recalcular
                        if (porcentajeReduccionVelocidad >= datos.maxPorcentajeReduccion ||
                            porcentajeAumentoTiempoEspera >= datos.maxPorcentajeAumentoEspera)
                        {
                            // Recalcular el máximo entre todas las áreas restantes
                            RecalcularMaximosYAplicar(datos.enemigo);
                        }

                        // Si ya no está en ninguna área, restaurar valores originales
                        if (datos.contadorAreas <= 0)
                        {
                            RestaurarValoresOriginales(datos.enemigo);
                            enemigosAfectadosGlobal.Remove(id);
                        }
                    }
                }
            }

            // Eliminar referencias a enemigos que salieron de esta área
            foreach (int id in enemigosQueSalieron)
            {
                enemigosEnEstaArea.Remove(id);
            }

            // Esperar antes de la próxima verificación
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Recalcular los efectos máximos para un enemigo
    private void RecalcularMaximosYAplicar(Enemigo enemigo)
    {
        int id = enemigo.GetInstanceID();
        if (enemigosAfectadosGlobal.TryGetValue(id, out EnemySlowdownData datos))
        {
            // Resetear los máximos
            datos.maxPorcentajeReduccion = 0f;
            datos.maxPorcentajeAumentoEspera = 0f;

            // Buscar todas las áreas de ralentización activas en la escena
            AreaRalentizacion[] todasLasAreas = FindObjectsOfType<AreaRalentizacion>();

            foreach (AreaRalentizacion area in todasLasAreas)
            {
                // Si el enemigo está en esta área
                if (area.enemigosEnEstaArea.Contains(id))
                {
                    // Actualizar máximos
                    if (area.porcentajeReduccionVelocidad > datos.maxPorcentajeReduccion)
                    {
                        datos.maxPorcentajeReduccion = area.porcentajeReduccionVelocidad;
                    }

                    if (area.porcentajeAumentoTiempoEspera > datos.maxPorcentajeAumentoEspera)
                    {
                        datos.maxPorcentajeAumentoEspera = area.porcentajeAumentoTiempoEspera;
                    }
                }
            }

            // Aplicar el nuevo máximo
            AplicarEfectoMaximo(datos);
        }
    }

    // Aplicar el efecto máximo a un enemigo
    private void AplicarEfectoMaximo(EnemySlowdownData datos)
    {
        Enemigo enemigo = datos.enemigo;

        // Aplicar velocidad con el porcentaje máximo de reducción
        float nuevaVelocidad = datos.velocidadOriginal * (1 - datos.maxPorcentajeReduccion / 100f);
        enemigo.velocidad = nuevaVelocidad;

        // Aplicar tiempo de espera con el porcentaje máximo de aumento
        float nuevoTiempoEspera = datos.tiempoEsperaOriginal * (1 + datos.maxPorcentajeAumentoEspera / 100f);
        enemigo.tiempoDeEspera = nuevoTiempoEspera;

        if (mostrarMensajesDebug)
            Debug.Log($"Efecto máximo aplicado: velocidad {enemigo.velocidad:F2}, tiempo de espera {enemigo.tiempoDeEspera:F2}");

        // Efecto visual (opcional)
        if (mostrarEfectoVisual && enemigo.sliderVida != null)
        {
            enemigo.sliderVida.fillRect.GetComponent<UnityEngine.UI.Image>().color = colorSliderAfectado;
        }
    }

    // Restaurar valores originales
    private void RestaurarValoresOriginales(Enemigo enemigo)
    {
        int id = enemigo.GetInstanceID();
        if (enemigosAfectadosGlobal.TryGetValue(id, out EnemySlowdownData datos))
        {
            // Restaurar valores originales
            enemigo.velocidad = datos.velocidadOriginal;
            enemigo.tiempoDeEspera = datos.tiempoEsperaOriginal;

            if (mostrarMensajesDebug)
                Debug.Log($"Valores restaurados: velocidad {enemigo.velocidad:F2}, tiempo de espera {enemigo.tiempoDeEspera:F2}");

            // Restaurar color del slider (si corresponde)
            if (mostrarEfectoVisual && enemigo.sliderVida != null)
            {
                enemigo.sliderVida.fillRect.GetComponent<UnityEngine.UI.Image>().color = Color.red;
            }
        }
    }

    // Dibujar gizmos en el editor para visualizar el área
    private void OnDrawGizmos()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = colorArea;

            // Dibujar diferente según el tipo de collider
            if (collider is BoxCollider)
            {
                BoxCollider boxCollider = collider as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphereCollider = collider as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                // Para capsule simplemente mostramos una esfera aproximada
                Gizmos.DrawSphere(transform.position + capsuleCollider.center, capsuleCollider.radius);
            }
        }
    }

    // Limpiar recursos al destruir el objeto
    private void OnDestroy()
    {
        // Procesar los enemigos que estaban en esta área
        foreach (int id in new List<int>(enemigosEnEstaArea))
        {
            if (enemigosAfectadosGlobal.TryGetValue(id, out EnemySlowdownData datos))
            {
                // Decrementar el contador de áreas
                datos.contadorAreas--;

                // Si este era el área con el efecto más fuerte, necesitamos recalcular
                if (porcentajeReduccionVelocidad >= datos.maxPorcentajeReduccion ||
                    porcentajeAumentoTiempoEspera >= datos.maxPorcentajeAumentoEspera)
                {
                    // Recalcular el máximo entre todas las áreas restantes
                    RecalcularMaximosYAplicar(datos.enemigo);
                }

                // Si ya no está en ninguna área, restaurar valores originales
                if (datos.contadorAreas <= 0)
                {
                    RestaurarValoresOriginales(datos.enemigo);
                    enemigosAfectadosGlobal.Remove(id);
                }
            }
        }

        // Limpiar la lista de enemigos de esta área
        enemigosEnEstaArea.Clear();

        // Detener la corrutina
        StopAllCoroutines();
    }
}