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
        mainCamera.SetActive(false);

        room1.SetActive(false);
        room2.SetActive(false);
        room3.SetActive(false);

        targetRoom.SetActive(true);
        currentRoom = targetRoom;

        canvas.SetActive(false);
    }

    // 👉 DAS IST DEIN "ZURÜCK BUTTON"
    public void BackToMain()
    {
        mainCamera.SetActive(true);

        room1.SetActive(false);
        room2.SetActive(false);
        room3.SetActive(false);

        currentRoom = null;

        canvas.SetActive(true);
    }
}