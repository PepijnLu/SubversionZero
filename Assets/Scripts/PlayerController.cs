using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundDistance = 0.4f;

    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    float xRotation = 0f;

    Rigidbody rb;
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        LookAround();

        // // Check if grounded
        // isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // // Jump
        // if (Input.GetButtonDown("Jump") && isGrounded)
        // {
        //     rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        // }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 targetVelocity = move * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
