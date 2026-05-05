using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
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

    [Header("Lamp GameObjects")]
    public GameObject[] lampObjects; // assign size 3 in Inspector

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;

    private bool runClient = true;

    // Lamp states
    private bool lamp1State;
    private bool lamp2State;
    private bool lamp3State;

    private bool suppressToggleEvent = false;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    async void Start()
    {
        if (lamp1Text != null) lamp1Text.text = "Connecting...";
        if (lamp2Text != null) lamp2Text.text = "Connecting...";
        if (lamp3Text != null) lamp3Text.text = "Connecting...";

        if (getStatusButton != null)
            getStatusButton.onClick.AddListener(RequestStatus);

        // Toggle Events
        if (lamp1Toggle != null)
            lamp1Toggle.onValueChanged.AddListener((val) => SendLampState(1, val));
        if (lamp2Toggle != null)
            lamp2Toggle.onValueChanged.AddListener((val) => SendLampState(2, val));
        if (lamp3Toggle != null)
            lamp3Toggle.onValueChanged.AddListener((val) => SendLampState(3, val));

        try
        {
            client = new TcpClient();
            await client.ConnectAsync("192.168.1.61", 8000);

            stream = client.GetStream();
            reader = new StreamReader(stream);

            _ = ReadLoop();

            RequestStatus();
        }
        catch (Exception e)
        {
            Debug.LogError("CONNECT ERROR: " + e.Message);
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
                    break;

                Debug.Log("RAW: " + data);
                messageQueue.Enqueue(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("READ ERROR: " + e.Message);
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

        Debug.Log("MSG: " + msg);

        if (msg.Contains("/Lampe="))
            HandleState(msg);
    }

    void HandleState(string msg)
    {
        string[] parts = msg.Split('=');
        if (parts.Length < 2) return;

        string path = parts[0].Trim();
        string val = parts[1].Trim();

        bool value = val.Equals("True", StringComparison.OrdinalIgnoreCase);

        Debug.Log($"STATE → {path} = {value}");

        if (path.Contains("room1"))
            lamp1State = value;
        else if (path.Contains("room2"))
            lamp2State = value;
        else if (path.Contains("room3"))
            lamp3State = value;

        UpdateUI();
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

        if (lamp1Toggle != null)
            lamp1Toggle.SetIsOnWithoutNotify(lamp1State);
        if (lamp2Toggle != null)
            lamp2Toggle.SetIsOnWithoutNotify(lamp2State);
        if (lamp3Toggle != null)
            lamp3Toggle.SetIsOnWithoutNotify(lamp3State);

        suppressToggleEvent = false;

        // Sync GameObjects with lamp states
        if (lampObjects != null && lampObjects.Length >= 3)
        {
            if (lampObjects[0] != null)
                lampObjects[0].SetActive(lamp1State);

            if (lampObjects[1] != null)
                lampObjects[1].SetActive(lamp2State);

            if (lampObjects[2] != null)
                lampObjects[2].SetActive(lamp3State);
        }
    }

    public void RequestStatus()
    {
        Debug.Log("STATUS REQUEST");
        SendMessageToServer("R");
    }

    void SendLampState(int lamp, bool state)
    {
        if (suppressToggleEvent) return;

        string value = state ? "True" : "False";
        string msg = $"::room{lamp}:SwitchValueGL={value}";

        Debug.Log("TOGGLE SEND: " + msg);
        SendMessageToServer(msg);
    }

    public void SendMessageToServer(string msg)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogWarning("Not connected");
            return;
        }

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
            stream.Write(data, 0, data.Length);

            Debug.Log("SEND: " + msg);
        }
        catch (Exception e)
        {
            Debug.LogError("SEND ERROR: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        runClient = false;

        reader?.Close();
        stream?.Close();
        client?.Close();
    }
}