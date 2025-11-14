using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Drag your player here

    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Start position
    public float followSpeed = 5f;   // How snappy the follow is

    [Header("Rotation")]
    public bool lockTopDownRotation = true;
    public Vector3 topDownEulerAngles = new Vector3(45f, 0f, 0f);

    [Header("Zoom")]
    public float zoomSpeed = 100f;     // How fast zoom reacts
    public float minZoom = 3f;        // Min size / height
    public float maxZoom = 20f;       // Max size / height

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleFollow();
        HandleRotation();
        HandleZoom();
    }

    private void HandleFollow()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (lockTopDownRotation)
        {
            transform.rotation = Quaternion.Euler(topDownEulerAngles);
        }
    }

    private void HandleZoom()
    {
        if (Mouse.current == null || cam == null) return;

        // Scroll value: positive when scrolling up
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) < 0.01f) return;

        // Normalize so it's not crazy fast
        float zoomDelta = scroll * zoomSpeed * Time.deltaTime * 0.1f;

        if (cam.orthographic)
        {
            // Orthographic: use orthographicSize
            float newSize = cam.orthographicSize - zoomDelta;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
        else
        {
            // Perspective: change camera height (offset.y)
            float newHeight = offset.y - zoomDelta;
            newHeight = Mathf.Clamp(newHeight, minZoom, maxZoom);
            offset.y = newHeight;
        }
    }
}