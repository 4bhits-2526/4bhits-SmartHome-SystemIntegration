using UnityEngine;
using UnityEngine.EventSystems;

public class TabletLookController : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Look Settings")]
    public float sensitivity = 0.2f;
    public float maxLookAngle = 80f;
    public bool invertY = false;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // Wichtig für UI + Touch
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleTouchLook();
    }

    void HandleTouchLook()
    {
        // Nur wenn genau ein Finger auf dem Bildschirm ist
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // ❗ Verhindert Kamera-Drehung wenn UI gedrückt wird
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(0))
                return;

            if (touch.phase == TouchPhase.Moved)
            {
                float touchX = touch.deltaPosition.x * sensitivity;
                float touchY = touch.deltaPosition.y * sensitivity;

                yaw += touchX;

                if (!invertY)
                    pitch -= touchY;
                else
                    pitch += touchY;

                pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

                // Rotation anwenden
                transform.localEulerAngles = new Vector3(0f, yaw, 0f);
                playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }
        }
    }
}