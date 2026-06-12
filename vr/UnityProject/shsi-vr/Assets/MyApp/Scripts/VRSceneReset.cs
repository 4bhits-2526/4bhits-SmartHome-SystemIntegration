using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class VRSceneReset : MonoBehaviour
{
    private bool wasUnfocused = false;

    void Start()
    {
        Debug.Log("VR Reset System läuft");
    }

    void Update()
    {
        // 🧪 TEST RESET (R)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Manual Reset (R)");
            ReloadScene();
        }

        // 🧪 TEST Idle Trigger (E)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Simulate Idle (E)");
            ReloadScene();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            wasUnfocused = true;
            return;
        }

        if (wasUnfocused && hasFocus)
        {
            Debug.Log("Focus Reset");
            ReloadScene();
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}