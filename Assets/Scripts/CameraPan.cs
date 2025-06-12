using UnityEngine;

public class CameraPanLimited : MonoBehaviour
{
    Camera camera;
    public Vector2 minPosition = new Vector2(-10f, -10f);
    public Vector2 maxPosition = new Vector2(10f, 10f);
    public float panSpeed = 0.01f; // Adjust for sensitivity

    private Vector3 lastMousePosition;
    private bool isPanning;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        if(!GameManager.instance.inBoardView) return;

        if (Input.GetMouseButtonDown(2)) // Middle mouse pressed
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2)) // Middle mouse released
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (camera.orthographic)
            {
                // Convert pixel delta to world units (based on orthographic size and screen height)
                float pixelsPerUnit = Screen.height / (2f * camera.orthographicSize);
                Vector3 move = new Vector3(-delta.x / pixelsPerUnit, -delta.y / pixelsPerUnit, 0f);

                transform.position += move;
            }
            else
            {
                // Optional: 3D camera panning logic
                Ray lastRay = camera.ScreenPointToRay(lastMousePosition);
                Ray currentRay = camera.ScreenPointToRay(Input.mousePosition);
                Vector3 lastPoint = lastRay.origin + lastRay.direction * 10f;
                Vector3 currentPoint = currentRay.origin + currentRay.direction * 10f;
                transform.position += (lastPoint - currentPoint);
            }

            lastMousePosition = Input.mousePosition;
            ClampPosition();
        }
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        transform.position = pos;
    }
}
