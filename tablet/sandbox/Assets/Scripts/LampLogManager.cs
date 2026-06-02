using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LampLogManager : MonoBehaviour
{
    [SerializeField] private OpcUaClientBehaviour opcClient;

    [Header("UI")]
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    void Start()
    {
        if (opcClient == null)
            opcClient = FindObjectOfType<OpcUaClientBehaviour>(true);

        if (opcClient == null)
            opcClient = Resources.FindObjectsOfTypeAll<OpcUaClientBehaviour>().FirstOrDefault();

        if (opcClient == null)
        {
            Debug.LogError("OpcUaClientBehaviour nicht gefunden!");
            return;
        }

        opcClient.OnLampStateChanged += LogLampState;
        opcClient.OnLampSwitchCountChanged += LogLampSwitchCount;
    }

    private void LogLampState(int roomNumber, bool isOn)
    {
        string state = isOn ? "AN" : "AUS";
        AddLog($"[{System.DateTime.Now:HH:mm:ss}] Raum {roomNumber}: Lampe {state}\n");
    }

    private void LogLampSwitchCount(int roomNumber, int switchCount)
    {
        AddLog($"[{System.DateTime.Now:HH:mm:ss}] Raum {roomNumber}: SwitchCnt = {switchCount}\n");
    }

    private void AddLog(string message)
    {
        logText.text += message;
        StartCoroutine(ScrollToRealBottom());
    }

    private IEnumerator ScrollToRealBottom()
    {
        yield return null;

        logText.ForceMeshUpdate(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(logText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Canvas.ForceUpdateCanvases();

        yield return null;

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRect.velocity = Vector2.zero;

        Canvas.ForceUpdateCanvases();
    }

    void OnDestroy()
    {
        if (opcClient != null)
        {
            opcClient.OnLampStateChanged -= LogLampState;
            opcClient.OnLampSwitchCountChanged -= LogLampSwitchCount;
        }
    }
}