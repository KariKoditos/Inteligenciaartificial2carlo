using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 10f;     
    public float lookSpeed = 2f;      
    public float fastMultiplier = 3f; 

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");   

       
        float upDown = 0f;
        if (Input.GetKey(KeyCode.E)) upDown = 1f;
        if (Input.GetKey(KeyCode.Q)) upDown = -1f;

        //rapidito
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= fastMultiplier;

        //movimiento
        Vector3 move = transform.right * h + transform.forward * v + transform.up * upDown;
        transform.position += move * speed * Time.deltaTime;

        //movimientomouse
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
