using UnityEngine;
using UnityEngine.SceneManagement;

public class ColorCycleOnReset : MonoBehaviour
{
    private static int resetCount = 0;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // Zähler erhöhen bei jedem Scene-Start
        resetCount++;

        ApplyColor();
    }

    void ApplyColor()
    {
        Color color;

        if (resetCount % 3 == 1)
        {
            color = Color.blue;
        }
        else if (resetCount % 3 == 2)
        {
            color = Color.green;
        }
        else
        {
            color = Color.red;
        }

        rend.material.color = color;

        Debug.Log("Reset: " + resetCount + " | Color: " + color);
    }

    // OPTIONAL: zum Testen ohne VR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}