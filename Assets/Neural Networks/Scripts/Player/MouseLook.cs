using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 200f;
    public float smoothTime = 0.03f; // lower is snappier

    float xRotation = 0f;
    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;
    }

    void Update()
    {
        Vector2 rawMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity * Time.deltaTime;

        // smooth the input
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, rawMouse, ref currentMouseDeltaVelocity, smoothTime);

        float mouseX = currentMouseDelta.x;
        float mouseY = currentMouseDelta.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
