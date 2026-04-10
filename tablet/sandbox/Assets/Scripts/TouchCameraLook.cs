using UnityEngine;

public class TouchCameraLook : MonoBehaviour
{
    public float sensitivity = 0.2f;
 
    float rotationX = 0f;
    float rotationY = 0f;
 
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
 
            if (touch.phase == TouchPhase.Moved)
            {
                float touchX = touch.deltaPosition.x * sensitivity;
                float touchY = touch.deltaPosition.y * sensitivity;
 
                rotationY += touchX;
                rotationX -= touchY;
 
                // Optional: Limit nach oben/unten
                rotationX = Mathf.Clamp(rotationX, -80f, 80f);
 
                transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }
    }
}