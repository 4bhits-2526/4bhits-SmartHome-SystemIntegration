using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
