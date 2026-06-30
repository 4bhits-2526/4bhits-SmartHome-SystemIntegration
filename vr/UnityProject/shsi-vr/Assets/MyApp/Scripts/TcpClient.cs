using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientUnity : MonoBehaviour
{
    [Header("UI")]
    public Text lamp1Text;
    public Text lamp2Text;
    public Text lamp3Text;

    public Toggle lamp1Toggle;
    public Toggle lamp2Toggle;
    public Toggle lamp3Toggle;

    public Button getStatusButton;

    [Header("Error Debug")]
    [SerializeField] TMP_Text errorStatus;

    [Header("Lamp GameObjects")]
    public GameObject[] lampObjects;

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;

    private bool runClient = true;

    private bool lamp1State;
    private bool lamp2State;
    private bool lamp3State;

    private bool suppressToggleEvent = false;

    private ConcurrentQueue<string> messageQueue =
        new ConcurrentQueue<string>();

    async void Start()
    {
        SyncLampObjects();
        UpdateUI();

        if (getStatusButton != null)
            getStatusButton.onClick.AddListener(RequestStatus);

        if (lamp1Toggle != null)
        {
            lamp1Toggle.onValueChanged.AddListener((val) =>
            {
                if (suppressToggleEvent) return;
                SendLampState(1, val);
            });
        }

        if (lamp2Toggle != null)
        {
            lamp2Toggle.onValueChanged.AddListener((val) =>
            {
                if (suppressToggleEvent) return;
                SendLampState(2, val);
            });
        }

        if (lamp3Toggle != null)
        {
            lamp3Toggle.onValueChanged.AddListener((val) =>
            {
                if (suppressToggleEvent) return;
                SendLampState(3, val);
            });
        }

        try
        {
            client = new TcpClient();
            await client.ConnectAsync("192.168.1.61", 8000);
            stream = client.GetStream();
            reader = new StreamReader(stream);

            Debug.Log("[TCP] Verbunden mit 192.168.1.61:8000");

            _ = ReadLoop();
            RequestStatus();
        }
        catch (Exception e)
        {
            errorStatus.text = GetError(e);
            Debug.LogError("[TCP] Verbindung fehlgeschlagen: " + e.Message);
        }
    }

    async Task ReadLoop()
    {
        try
        {
            while (runClient)
            {
                string data = await reader.ReadLineAsync();

                if (data == null)
                {
                    Debug.LogWarning("[TCP] Verbindung vom Server getrennt.");
                    break;
                }

                messageQueue.Enqueue(data);
            }
        }
        catch (Exception e)
        {
            errorStatus.text = GetError(e);
            Debug.LogError("[TCP] Lesefehler: " + e.Message);
        }
    }


    void Update()
    {
        while (messageQueue.TryDequeue(out string msg))
        {
            HandleMessage(msg);
        }
    }

    void HandleMessage(string msg)
    {
        msg = msg.Trim();

        if (string.IsNullOrEmpty(msg))
            return;

        HandleState(msg);
    }

    void HandleState(string msg)
    {
        // Server-Broadcasts ohne Raum-Info (z.B. "True", "False") → ignorieren
        if (msg.Equals("True", StringComparison.OrdinalIgnoreCase) ||
            msg.Equals("False", StringComparison.OrdinalIgnoreCase))
            return;

        if (IsIgnoredStateMessage(msg))
            return;

        if (!TryParseStateMessage(msg, out int lampIndex, out bool value))
        {
            Debug.LogWarning("[TCP] Unbekannte Nachricht: " + msg);
            return;
        }

        Debug.Log($"[STATE] Lampe {lampIndex} → {(value ? "EIN" : "AUS")}");
        ApplyLampState(lampIndex, value);
        UpdateUI();
    }

    bool IsIgnoredStateMessage(string msg)
    {
        return msg.Contains("LampeRT", StringComparison.OrdinalIgnoreCase) ||
               msg.Contains("LampeSwCnt", StringComparison.OrdinalIgnoreCase);
    }

    bool TryParseStateMessage(string msg, out int lampIndex, out bool value)
    {
        lampIndex = 0;
        value = false;

        string[] parts = msg.Split('=');

        if (parts.Length != 2)
            return false;

        string path = parts[0].Trim();
        string val = parts[1].Trim();

        if (!IsLampStatePath(path))
            return false;

        if (path.Contains("room1", StringComparison.OrdinalIgnoreCase))
            lampIndex = 1;
        else if (path.Contains("room2", StringComparison.OrdinalIgnoreCase))
            lampIndex = 2;
        else if (path.Contains("room3", StringComparison.OrdinalIgnoreCase))
            lampIndex = 3;
        else {
            Debug.LogError("DU BIST GUT GENUG❤️");
            return false;
        }

        if (val.Equals("True", StringComparison.OrdinalIgnoreCase))
            value = true;
        else if (val.Equals("False", StringComparison.OrdinalIgnoreCase))
            value = false;
        else {
            Debug.LogError("DU BIST GUT GENUG❤️");
            return false;
        }

        return true;
    }

    bool IsLampStatePath(string path)
    {
        string normalizedPath = path.Replace(':', '/').Trim('/');
        string[] segments = normalizedPath.Split('/');

        return segments.Length > 0 &&
               segments[segments.Length - 1].Equals("Lampe", StringComparison.OrdinalIgnoreCase);
    }

    void ApplyLampState(int lampIndex, bool value)
    {
        switch (lampIndex)
        {
            case 1: lamp1State = value; break;
            case 2: lamp2State = value; break;
            case 3: lamp3State = value; break;
            default: return;
        }

        SyncLampObjects();
        UpdateUI();
    }

    void SyncLampObjects()
    {
        SetLampObjectState(0, lamp1State);
        SetLampObjectState(1, lamp2State);
        SetLampObjectState(2, lamp3State);
    }

    void SetLampObjectState(int arrayIndex, bool state)
    {
        if (lampObjects == null ||
            lampObjects.Length <= arrayIndex ||
            lampObjects[arrayIndex] == null)
            return;

        lampObjects[arrayIndex].SetActive(state);
    }

    void UpdateUI()
    {
        if (lamp1Text != null)
            lamp1Text.text = lamp1State ? "Lampe1: EIN" : "Lampe1: AUS";

        if (lamp2Text != null)
            lamp2Text.text = lamp2State ? "Lampe2: EIN" : "Lampe2: AUS";

        if (lamp3Text != null)
            lamp3Text.text = lamp3State ? "Lampe3: EIN" : "Lampe3: AUS";

        suppressToggleEvent = true;

        if (lamp1Toggle != null) lamp1Toggle.SetIsOnWithoutNotify(lamp1State);
        if (lamp2Toggle != null) lamp2Toggle.SetIsOnWithoutNotify(lamp2State);
        if (lamp3Toggle != null) lamp3Toggle.SetIsOnWithoutNotify(lamp3State);

        suppressToggleEvent = false;
    }

    public void SendSwitchValue(int lamp, bool state)
    {
        string value = state ? "True" : "False";

        string msg = $"::room{lamp}:SwitchValueGL={value}";

        Debug.Log($"[SEND] {msg}");

        SendMessageToServer(msg);
        RequestStatus();
        Invoke(nameof(SyncLampObjects), 0.1f);
    }


    public void RequestStatus()
    {
        SendMessageToServer("R");

    }

    void SendLampState(int lamp, bool state)
    {
        string value = state ? "True" : "False";

        string msg =
            $"::room{lamp}:SwitchValueGL={value}";

        Debug.Log(
            $"[SEND] Lampe {lamp} -> {(state ? "EIN" : "AUS")}");

        SendMessageToServer(msg);

        // Update local state immediately so toggling/UI/visuals work
        ApplyLampState(lamp, state);

        // Optional: confirm with server shortly after
        Invoke(nameof(RequestStatus), 0.2f);
    }

    public void SendMessageToServer(string msg)
    {
        SyncLampObjects();
        if (client == null || !client.Connected)
        {
            Debug.LogError("[TCP] Nicht verbunden – Nachricht verworfen: " + msg);
            return;
        }

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("[TCP] Sendefehler: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        runClient = false;
        reader?.Close();
        stream?.Close();
        client?.Close();
    }

    string GetError(Exception e)
    {
        if (e is SocketException se)
        {
            return se.SocketErrorCode switch
            {
                SocketError.ConnectionRefused => "Verbindung abgelehnt – läuft der Server?",
                SocketError.HostNotFound => "Host nicht gefunden – IP-Adresse prüfen.",
                SocketError.TimedOut => "Verbindung abgelaufen – Netzwerk prüfen.",
                SocketError.NetworkUnreachable => "Netzwerk nicht erreichbar.",
                SocketError.AddressAlreadyInUse => "Port wird bereits verwendet.",
                _ => $"Netzwerkfehler ({(int)se.SocketErrorCode}): {se.Message}"
            };
        }

        return e switch
        {
            IOException => "Fehler im Netzwerkstream.",
            TimeoutException => "Zeitüberschreitung bei der Verbindung.",
            _ => "Unbekannter Fehler: " + e.Message
        };
    }
}
