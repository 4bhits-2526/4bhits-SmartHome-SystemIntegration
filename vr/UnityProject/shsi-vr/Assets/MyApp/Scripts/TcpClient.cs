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

    public Button getStatusButton;
    public Button toggleButton;

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;

    private bool runClient = true;

    // Lamp states
    private bool lamp1State;
    private bool lamp2State;
    private bool lamp3State;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    async void Start()
    {
        if (lamp1Text != null) lamp1Text.text = "Connecting...";
        if (lamp2Text != null) lamp2Text.text = "Connecting...";
        if (lamp3Text != null) lamp3Text.text = "Connecting...";

        if (getStatusButton != null)
            getStatusButton.onClick.AddListener(RequestStatus);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(SwitchLamp);

        try
        {
            client = new TcpClient();
            await client.ConnectAsync("192.168.1.61", 8000);

            stream = client.GetStream();
            reader = new StreamReader(stream);

            UpdateUI();

            _ = ReadLoop();
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

        // ignore menu text
        if (msg.Contains("SmartHome") || msg.Contains("Press") || msg.Contains("abort"))
            return;

        if (msg.Contains("="))
            HandleState(msg);
    }

    void HandleState(string msg)
    {
        // expected:
        // room1/Lampe1=True
        // room1/Lampe2=False
        // room1/Lampe3=True

        string[] parts = msg.Split('=');
        if (parts.Length < 2) return;

        string path = parts[0].Trim();
        string val = parts[1].Trim();

        bool value = val.Equals("True", StringComparison.OrdinalIgnoreCase);

        Debug.Log($"STATE → {path} = {value}");

        if (path.Contains("Lampe1"))
            lamp1State = value;
        else if (path.Contains("Lampe2"))
            lamp2State = value;
        else if (path.Contains("Lampe3"))
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
    }

    public void RequestStatus()
    {
        Debug.Log("STATUS REQUEST");
        SendMessageToServer("R");
    }

    public void SwitchLamp()
    {
        Debug.Log("SWITCH REQUEST");
        SendMessageToServer("2");
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