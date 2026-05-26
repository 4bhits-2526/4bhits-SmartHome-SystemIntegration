using System.Linq;
using UnityEngine;
using TMPro;

public class LampLogManager : MonoBehaviour
{
    [SerializeField]
    private OpcUaClientBehaviour opcClient;

    public TMP_Text logText;

    void Start()
    {
        if (opcClient == null)
        {
            // Suche zuerst nach aktiven Objekten
            opcClient = FindObjectOfType<OpcUaClientBehaviour>(true);
        }

        if (opcClient == null)
        {
            // Fallback: Suche auch inaktive Objekte
            opcClient = Resources.FindObjectsOfTypeAll<OpcUaClientBehaviour>().FirstOrDefault();
        }

        if (opcClient == null)
        {
            Debug.LogError("OpcUaClientBehaviour nicht gefunden! Bitte das GameObject mit dem Skript in der Szene hinzufügen oder den Referenzwert im Inspector setzen.");
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
    }

    private void LogLampSwitchCount(int roomNumber, int switchCount)
    {
        string message =
            $"[{System.DateTime.Now:HH:mm:ss}] Raum {roomNumber}: SwitchCnt = {switchCount}\n";

        logText.text += message;
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