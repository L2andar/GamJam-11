using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSPlayer : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float smoothTime = 0.08f;

    // head-bob settings
    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSmooth = 0.08f;

    private CharacterController controller;
    private Transform cameraTransform;
    private EventSystem eventSystem;
    private Vector3 currentMoveVelocity = Vector3.zero;
    private Vector3 moveVelocityRef = Vector3.zero;
    private float pitch = 0f;

    private const float PitchMin = -80f;
    private const float PitchMax = 80f;

    // head-bob state
    private Vector3 cameraStartLocalPos = Vector3.zero;
    private float bobTimer = 0f;
    private Vector3 bobVelocity = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        cameraTransform = playerCamera != null ? playerCamera.transform : null;
        eventSystem = EventSystem.current;

        // store camera start local position for bobbing baseline
        if (cameraTransform != null) cameraStartLocalPos = cameraTransform.localPosition;

<<<<<<< HEAD
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure a Rigidbody exists so trigger events reliably fire when using CharacterController.
        // We make it kinematic so it won't interfere with CharacterController movement.
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            Debug.Log("SimpleFPSPlayer: Added kinematic Rigidbody to ensure trigger events.");
        }
        else
        {
            // if a Rigidbody already exists, ensure it's configured to work with CharacterController
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
=======
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
>>>>>>> 9e148df (coding ui)
    }

    void Update()
    {
        HandleCursorUnlock();
        HandleLook();
        HandleMove();
        HandleBobbing(); // apply head-bob after movement updates
    }

    void HandleCursorUnlock()
    {
        // Press ESC to unlock and show cursor
<<<<<<< HEAD
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // If cursor is visible and the player clicks the game window (left mouse)
        // re-lock and hide the cursor, but only if the click was NOT over UI.
        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            if (eventSystem == null || !eventSystem.IsPointerOverGameObject())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
=======
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //    return;
        //}

        //// If cursor is visible and the player clicks the game window (left mouse)
        //// re-lock and hide the cursor, but only if the click was NOT over UI.
        //if (Cursor.visible && Input.GetMouseButtonDown(0))
        //{
        //    //if (eventSystem == null || !eventSystem.IsPointerOverGameObject())
        //    //{
        //    //    Cursor.lockState = CursorLockMode.Locked;
        //    //    Cursor.visible = false;
        //    //}
        //}
>>>>>>> 9e148df (coding ui)
    }

    void HandleLook()
    {
        // Respect UI interaction: when cursor visible and pointer over UI, do not rotate camera
<<<<<<< HEAD
        if (Cursor.visible && eventSystem != null && eventSystem.IsPointerOverGameObject())
            return;
=======
        //if (Cursor.visible && eventSystem != null && eventSystem.IsPointerOverGameObject())
        //    return;
>>>>>>> 9e148df (coding ui)

        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;

        // yaw on player
        transform.Rotate(0f, mx, 0f);

        // pitch on camera (invert mouse Y), clamped
        pitch = Mathf.Clamp(pitch + my, PitchMin, PitchMax);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(-pitch, 0f, 0f);
    }

    void HandleMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 desired = transform.right * x + transform.forward * z;
        if (desired.sqrMagnitude > 1f) desired.Normalize();

        // SmoothDamp with explicit deltaTime for consistent smoothing
        currentMoveVelocity = Vector3.SmoothDamp(currentMoveVelocity, desired, ref moveVelocityRef, smoothTime, Mathf.Infinity, Time.deltaTime);
        controller.Move(currentMoveVelocity * moveSpeed * Time.deltaTime);
    }

    // new: procedural head-bob using sine wave, affects only camera localPosition
    void HandleBobbing()
    {
        if (cameraTransform == null) return;

        // movement intensity (0..1). currentMoveVelocity is a direction vector smoothed; use its magnitude.
        float moveIntensity = currentMoveVelocity.magnitude;

        Vector3 targetLocalPos = cameraStartLocalPos;

        if (moveIntensity > 0.01f)
        {
            // advance timer based on frequency and movement intensity
            bobTimer += Time.deltaTime * bobFrequency * Mathf.Clamp01(moveIntensity);
            // sine wave for vertical bob; multiply by intensity so partial movement reduces bob
            float bobOffsetY = Mathf.Sin(bobTimer * Mathf.PI * 2f) * bobAmplitude * Mathf.Clamp01(moveIntensity);
            targetLocalPos += Vector3.up * bobOffsetY;
        }
        else
        {
            // when stopped, slowly reset timer to avoid large jumps on restart (optional)
            bobTimer = 0f;
        }

        // smooth the camera local position toward target
        cameraTransform.localPosition = Vector3.SmoothDamp(cameraTransform.localPosition, targetLocalPos, ref bobVelocity, bobSmooth);
    }
}
