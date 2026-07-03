using UnityEngine;

public class TabletLookController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Look Settings")]
    public float sensitivity = 0.2f;
    public float maxLookAngle = 80f;
    public bool invertY = false;

    [Header("Debug")]
    [Tooltip("Ignoriert die erste Bewegung nach dem Aufsetzen des Fingers.")]
    public bool ignoreFirstMove = true;

    private float yaw;
    private float pitch;

    private int activeFingerId = -1;
    private bool skipNextMove;

    private void Start()
    {
        // Für Tablet / Touch
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Aktuelle Rotation übernehmen, damit es keinen Sprung beim Start gibt
        yaw = transform.localEulerAngles.y;

        pitch = playerCamera.transform.localEulerAngles.x;

        // Unity speichert Winkel von 0–360°
        if (pitch > 180f)
            pitch -= 360f;
    }

    private void Update()
    {
        HandleTouchLook();
    }

    private void HandleTouchLook()
    {
        if (Input.touchCount != 1)
        {
            activeFingerId = -1;
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                activeFingerId = touch.fingerId;
                skipNextMove = ignoreFirstMove;
                break;

            case TouchPhase.Moved:

                if (touch.fingerId != activeFingerId)
                    return;

                // Erstes Moved-Event ignorieren
                if (skipNextMove)
                {
                    skipNextMove = false;
                    return;
                }

                float touchX = touch.deltaPosition.x * sensitivity;
                float touchY = touch.deltaPosition.y * sensitivity;

                yaw += touchX;

                if (invertY)
                    pitch += touchY;
                else
                    pitch -= touchY;

                pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

                transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                activeFingerId = -1;
                break;
        }
    }
}