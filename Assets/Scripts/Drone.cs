using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Drone : MonoBehaviour
{
    public RedNeuronal brain;
    public Rigidbody rb;
    public float sensorRange = 15f;
    public LayerMask sensorMask;
    public float thrustPower = 15f;
    public float torquePower = 5f;
    public float maxSpeed = 20f;
    public bool isAlive = true;
    public float fitness = 0f;
    private Vector3 lastPosition;
    private GameManager manager;
    public LayerMask deathMask;
    private Renderer rend;
    private Color originalColor;

    public void Initialize(RedNeuronal assignedBrain, GameManager gm)
    {
        
        brain = assignedBrain;
        manager = gm;

       
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        
        rb.useGravity = false;                
        rb.isKinematic = false;               
        rb.velocity = Vector3.zero;            
        rb.angularVelocity = Vector3.zero;

        if (rend == null) rend = GetComponent<Renderer>();
        if (rend != null) originalColor = rend.material.color;

        // Reset
        isAlive = true;
        fitness = 0f;
        lastPosition = transform.position;
    }

    private void FixedUpdate()  //fisicas del juego
    {
        //if se murio ya ni modo
        if (!isAlive) { return; }

        //velocidad limitada
        float speed = rb.velocity.magnitude;
        if (speed > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        // 1) Recolectar inputs de sensores con los rayos y la velocidad
        float[] inputs = GatherInputs();

        // 2) feedback de velocidad pesos etc
        float[] outputs = brain.FeedForward(inputs);

        // 3) checa como debe salir el dron 
        float yaw = Mathf.Clamp(outputs[0], -1f, 1f);
        float pitch = Mathf.Clamp(outputs[1], -1f, 1f);
        float thrust01 = (Mathf.Clamp(outputs[2], -1f, 1f) + 1f) * 0.5f; // 0..1

        // 4) Aplicar fuerzas y giros 
        Vector3 localTorque = new Vector3(pitch * torquePower, yaw * torquePower, 0f);
        rb.AddRelativeTorque(localTorque, ForceMode.Acceleration);

        // 5) Aplicar empuje 
        Vector3 localForce = new Vector3(0f, 0f, thrust01 * thrustPower);
        rb.AddRelativeForce(localForce, ForceMode.Acceleration);

        // 6) Actualizar fitness basado en supervivencia y avance
        UpdateFitness();
    }


    private float[] GatherInputs()
    {
        // Usamos 6 entradas:
        // 0: sensor adelante
        // 1: sensor izquierda
        // 2: sensor derecha
        // 3: sensor arriba
        // 4: sensor abajo
        // 5: velocidad normalizada (0..1)
        float[] inputs = new float[6];

        // Lanzas rayos desde la posición del dron en ejes locales
        inputs[0] = RaySensor(transform.forward);   // adelante
        inputs[1] = RaySensor(-transform.right);    // izquierda
        inputs[2] = RaySensor(transform.right);     // derecha
        inputs[3] = RaySensor(transform.up);        // arriba
        inputs[4] = RaySensor(-transform.up);       // abajo

        // Velocidad normalizada respecto al tope maxSpeed
        float speed = rb != null ? rb.velocity.magnitude : 0f;
        float speedNorm = Mathf.Clamp01(speed / maxSpeed);
        inputs[5] = speedNorm;

        return inputs;
    }

    private float RaySensor(Vector3 direction)
    {
        RaycastHit hitInfo;
        bool didHit = Physics.Raycast(
            origin: transform.position,
            direction: direction,
            hitInfo: out hitInfo,
            maxDistance: sensorRange,
            layerMask: sensorMask
        );

        // Debug visual del rayo (solo editor/juego con Gizmos)
        Debug.DrawRay(transform.position, direction * sensorRange, didHit ? Color.red : Color.green);

        if (didHit)
        {
            // Normalizamos: 0 (muy cerca) .. 1 (lejos, igual a sensorRange)
            float normalized = hitInfo.distance / sensorRange;
            return normalized;
        }
        else
        {
            // Nada delante: consideramos espacio libre
            return 1f;
        }
    }

    private void UpdateFitness()
    {
        // Recompensa por sobrevivir 
        fitness += Time.fixedDeltaTime * 0.1f;

        
        // Calculamos cuánto avanzó el dron en su eje forward desde el último frame
        Vector3 delta = transform.position - lastPosition;
        float forwardProgress = Vector3.Dot(transform.forward, delta);

        // Solo sumamos progreso positivo 
        if (forwardProgress > 0f)
        {
            fitness += forwardProgress;
        }

        // Guardamos posición para el siguiente cálculo
        lastPosition = transform.position;
    }

    public void Die()
    {
        if (!isAlive) { return; }

        isAlive = false;

        //Stop fisicas para que ya no se mueva
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        //avisamos al GameManager 
        if (manager != null)
        {
            manager.NotifyDroneDeath(this);
        }

        // Desactivamos el objeto (opcional Destroy con retardo si prefieres)
        gameObject.SetActive(false);
        // Destroy(gameObject, 0.25f);
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (!isAlive) return;

        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive) return;

        // Si entramos a una zona con el Tag "KillZone", morimos
        if (other.CompareTag("KillZone"))
        {
            Die();
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }

    public void HighlightChampion()
    {
        if (rend != null) rend.material.color = Color.magenta;
    }

    public void ResetHighlight()
    {
        if (rend != null) rend.material.color = originalColor;
    }

}