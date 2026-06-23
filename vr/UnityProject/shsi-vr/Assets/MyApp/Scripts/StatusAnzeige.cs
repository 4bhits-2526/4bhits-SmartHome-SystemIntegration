using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StatusAnzeige : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text statusText;

    [Header("Server")]
    [SerializeField] string serverHost = "192.168.1.61";
    [SerializeField] int serverPort = 8000;

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;

    private bool runClient = true;
    private bool connecting;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    void Start()
    {
        SetStatus("nicht verbunden");
        _ = ConnectionLoop();
    }

    async Task ConnectionLoop()
    {
        while (runClient)
        {
            if (!IsConnected() && !connecting)
                await ConnectToServer();

            await Task.Delay(2000);
        }
    }

    async Task ConnectToServer()
    {
        connecting = true;
        CloseConnection();

        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverHost, serverPort);
            stream = client.GetStream();
            reader = new StreamReader(stream);

            SetStatus("verbunden");
            _ = ReadLoop();
            RequestStatus();
        }
        catch
        {
            SetStatus("nicht verbunden");
            CloseConnection();
        }
        finally
        {
            connecting = false;
        }
    }

    async Task ReadLoop()
    {
        try
        {
            while (runClient)
            {
                if (reader == null) break;

                string data = await reader.ReadLineAsync();

                if (data == null)
                {
                    SetStatus("nicht verbunden");
                    CloseConnection();
                    break;
                }

                messageQueue.Enqueue(data);
            }
        }
        catch
        {
            SetStatus("nicht verbunden");
            CloseConnection();
        }
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string msg))
            HandleMessage(msg);
    }

    void HandleMessage(string msg)
    {
        msg = msg.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        HandleState(msg);
    }

    void HandleState(string msg)
    {
        if (msg.Equals("True", StringComparison.OrdinalIgnoreCase) ||
            msg.Equals("False", StringComparison.OrdinalIgnoreCase))
            return;

        if (!TryParseStateMessage(msg, out int lampIndex, out bool value))
        {
            Debug.LogWarning("[TCP] Unbekannte Nachricht: " + msg);
            return;
        }
    }

    bool TryParseStateMessage(string msg, out int lampIndex, out bool value)
    {
        lampIndex = 0;
        value = false;

        string[] parts = msg.Split('=');
        if (parts.Length != 2) return false;

        string path = parts[0].Trim();
        string val = parts[1].Trim();

        if (path.Contains("room1", StringComparison.OrdinalIgnoreCase))
            lampIndex = 1;
        else if (path.Contains("room2", StringComparison.OrdinalIgnoreCase))
            lampIndex = 2;
        else if (path.Contains("room3", StringComparison.OrdinalIgnoreCase))
            lampIndex = 3;
        else
            return false;

        if (val.Equals("True", StringComparison.OrdinalIgnoreCase))
            value = true;
        else if (val.Equals("False", StringComparison.OrdinalIgnoreCase))
            value = false;
        else
            return false;

        return true;
    }

    public void RequestStatus() => SendMessageToServer("R");

    public void SendMessageToServer(string msg)
    {
        if (!IsConnected()) return;

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
            stream.Write(data, 0, data.Length);
        }
        catch
        {
            SetStatus("nicht verbunden");
            CloseConnection();
        }
    }

    void SetStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    bool IsConnected()
    {
        try
        {
            if (client == null || client.Client == null || !client.Connected)
                return false;

            Socket socket = client.Client;
            return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch { return false; }
    }

    void CloseConnection()
    {
        reader?.Close();
        stream?.Close();
        client?.Close();
        reader = null;
        stream = null;
        client = null;
    }

    void OnApplicationQuit() { runClient = false; CloseConnection(); }
    void OnDestroy() { runClient = false; CloseConnection(); }
}