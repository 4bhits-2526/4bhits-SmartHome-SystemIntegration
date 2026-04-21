using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject mainCamera;

    public GameObject room1;
    public GameObject room2;
    public GameObject room3;

    private GameObject currentRoom;

    public GameObject canvas;

    public void OpenRoom1()
    {
        SwitchRoom(room1);
    }

    public void OpenRoom2()
    {
        SwitchRoom(room2);
    }

    public void OpenRoom3()
    {
        SwitchRoom(room3);
    }

    private void SwitchRoom(GameObject targetRoom)
    {
        // Kamera aus
        mainCamera.SetActive(false);

        // alle Räume aus
        room1.SetActive(false);
        room2.SetActive(false);
        room3.SetActive(false);

        // gewünschten Raum an
        targetRoom.SetActive(true);
        currentRoom = targetRoom;

        // Canvas aus
        canvas.SetActive(false);

        // 🔒 Cursor sperren (optional, falls du im Raum frei schauen willst)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            mainCamera.SetActive(true);

            room1.SetActive(false);
            room2.SetActive(false);
            room3.SetActive(false);

            currentRoom = null;

            canvas.SetActive(true);

            // ✅ FIX: Cursor wieder freigeben
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}