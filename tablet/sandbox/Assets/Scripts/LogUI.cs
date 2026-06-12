using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogUI : MonoBehaviour
{
    [Header("UI")]
    public ScrollRect scrollRect;
    public Transform content;
    public GameObject textPrefab;

    public void AddLog(string message)
    {
        // Neuen Eintrag erzeugen
        GameObject newEntry = Instantiate(textPrefab, content);

        TMP_Text text = newEntry.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = message;
        }

        // Layout aktualisieren
        Canvas.ForceUpdateCanvases();

        // Nach unten scrollen
        scrollRect.verticalNormalizedPosition = 0f;
    }
}