using UnityEngine;
using UnityEngine.SceneManagement;

public class VRSceneReset : MonoBehaviour {
    private bool wasUnfocused = false;

    void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus) {
            wasUnfocused = true;
            return;
        }

        if (wasUnfocused && hasFocus) {
            ReloadScene();
        }
    }

    void ReloadScene() {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}