using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    public TextMeshProUGUI logText;
    public ScrollRect scrollRect;

    public void AddLog(string message)
    {
        logText.text += message + "\n";
        StartCoroutine(ScrollDownNextFrame());
    }

    private IEnumerator ScrollDownNextFrame()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}