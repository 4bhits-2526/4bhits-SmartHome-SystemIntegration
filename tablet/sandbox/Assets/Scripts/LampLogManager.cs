using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LampLogManager : MonoBehaviour
{
    [SerializeField] private OpcUaClientBehaviour opcClient;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;

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
        string message =
            $"[{System.DateTime.Now:HH:mm:ss}] Raum {roomNumber}: Lampe {state}\n";

        logText.text += message;

        StartCoroutine(ScrollToBottom());
    }

    private void LogLampSwitchCount(int roomNumber, int switchCount)
    {
        string message =
            $"[{System.DateTime.Now:HH:mm:ss}] Raum {roomNumber}: SwitchCnt = {switchCount}\n";

        logText.text += message;

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
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