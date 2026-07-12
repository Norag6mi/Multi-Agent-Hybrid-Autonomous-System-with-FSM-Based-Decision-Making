using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    public Camera playerCamera;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 10f; // smoothing
    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.6f;
    public float gravity = -24f;
    public float groundedOffset = -0.05f;
    public float groundedRadius = 0.25f;
    public LayerMask groundLayers = ~0;
    bool grounded;
    Vector3 velocity;

    [Header("Crouch")]
    public float standHeight = 2.0f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 8f;
    public KeyCode crouchKey = KeyCode.LeftControl;
    bool isCrouched = false;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool allowSprint = true;

    [Header("FOV Kick")]
    public float normalFOV = 60f;
    public float sprintFOV = 72f;
    public float fovTransitionSpeed = 6f;

    [Header("Head Bob (optional)")]
    public bool enableHeadBob = false;
    public float headBobFrequency = 1.5f;
    public float headBobAmplitude = 0.04f;
    float headBobTimer = 0f;
    Vector3 cameraInitialLocalPos;

    void Reset()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        cameraInitialLocalPos = playerCamera.transform.localPosition;
        // set initial CharacterController height to standHeight
        controller.height = standHeight;
        controller.center = new Vector3(0, standHeight / 2f, 0);
    }

    void Update()
    {
        GroundCheck();
        HandleInput();
        ApplyGravity();
        MoveCharacter();
        HandleCrouchHeight();
        HandleFOV();
        HandleHeadBob();
    }

    void GroundCheck()
    {
        Vector3 spherePos = transform.position + Vector3.up * groundedOffset;
        grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        if (grounded && velocity.y < 0f)
            velocity.y = -2f; // small downward stick to ground
    }

    void HandleInput()
    {
        // Smooth movement input
        Vector2 targetDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        targetDir = Vector2.ClampMagnitude(targetDir, 1f);
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, 1f / acceleration, Mathf.Infinity, Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (grounded && Input.GetButtonDown("Jump"))
        {
            // v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
        velocity.y += gravity * Time.deltaTime;
    }

    void MoveCharacter()
    {
        // Determine speed
        float targetSpeed = walkSpeed;
        if (allowSprint && Input.GetKey(sprintKey) && !isCrouched && currentDir.sqrMagnitude > 0.01f)
            targetSpeed = sprintSpeed;
        if (isCrouched) targetSpeed = crouchSpeed;

        // Build movement vector in world space
        Vector3 move = transform.right * currentDir.x + transform.forward * currentDir.y;
        Vector3 horizontalVelocity = move * targetSpeed;

        // apply movement and gravity via CharacterController
        Vector3 finalVelocity = horizontalVelocity + new Vector3(0f, velocity.y, 0f);
        controller.Move(finalVelocity * Time.deltaTime);
    }

    void HandleCrouchHeight()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            isCrouched = !isCrouched;
        }

        float targetHeight = isCrouched ? crouchHeight : standHeight;
        float currentHeight = controller.height;
        float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // attempt to prevent standing up into a low ceiling
        if (!isCrouched && currentHeight < targetHeight)
        {
            // cast above player to see if we can stand
            float castDist = (targetHeight - currentHeight);
            Vector3 castOrigin = transform.position + Vector3.up * currentHeight;
            if (Physics.SphereCast(castOrigin, controller.radius, Vector3.up, out RaycastHit hit, castDist, groundLayers, QueryTriggerInteraction.Ignore))
            {
                // hit something above; keep crouched
                newHeight = currentHeight;
                isCrouched = true;
            }
        }

        // adjust center so feet stay on ground
        float prevHeight = controller.height;
        controller.height = Mathf.Max(0.1f, newHeight);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        // Move camera down a little when crouching (local)
        Vector3 camLocal = playerCamera.transform.localPosition;
        float camTargetY = controller.height - 0.1f; // slight offset so camera is not at top of capsule
        float camY = Mathf.Lerp(camLocal.y, camTargetY, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.transform.localPosition = new Vector3(camLocal.x, camY, camLocal.z);
    }

    void HandleFOV()
    {
        if (playerCamera == null) return;

        float targetFOV = (allowSprint && Input.GetKey(sprintKey) && !isCrouched && currentDir.sqrMagnitude > 0.01f) ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;

        if (controller.velocity.magnitude > 0.1f && grounded)
        {
            headBobTimer += Time.deltaTime * (controller.velocity.magnitude / walkSpeed) * headBobFrequency;
            float bobX = Mathf.Sin(headBobTimer) * headBobAmplitude;
            float bobY = Mathf.Cos(headBobTimer * 2f) * headBobAmplitude;
            playerCamera.transform.localPosition = cameraInitialLocalPos + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            // smooth return to initial
            headBobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraInitialLocalPos, Time.deltaTime * 6f);
        }
    }

    // Debug visualize ground-check
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position + Vector3.up * groundedOffset;
        Gizmos.DrawWireSphere(pos, groundedRadius);
    }
}
