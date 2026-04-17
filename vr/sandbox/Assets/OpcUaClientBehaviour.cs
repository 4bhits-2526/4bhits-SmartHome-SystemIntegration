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
    public Text outputText; // im Inspector zuweisen

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;

    private bool runClient = true;

    // Thread-safe Queue für Messages
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    async void Start()
    {
        try
        {
            Debug.Log("Connecting...");

            client = new TcpClient();
            await client.ConnectAsync("192.168.1.61", 8000);

            Debug.Log("Connected");

            stream = client.GetStream();
            reader = new StreamReader(stream);

            _ = ReadLoop();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    async Task ReadLoop()
    {
        try
        {
            while (runClient)
            {
                string data = await reader.ReadLineAsync();

                if (data == null) break;

                if (data.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                // In Queue statt direkt UI
                messageQueue.Enqueue(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void Update()
    {
        // läuft im Unity Main Thread
        while (messageQueue.TryDequeue(out string msg))
        {
            if (outputText != null)
            {
                outputText.text += "\n" + msg;
            }

            Debug.Log("DataIn: " + msg);
        }
    }

    public void SendMessageToServer(string msg)
    {
        if (client == null || !client.Connected) return;

        byte[] data = Encoding.ASCII.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }

    void OnApplicationQuit()
    {
        runClient = false;

        reader?.Close();
        stream?.Close();
        client?.Close();
    }
}