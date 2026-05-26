using System;
using System.IO;
using UnityEngine;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Collections.Concurrent; // WICHTIG für Thread-Sicherheit

public class OpcUaClientBehaviour : MonoBehaviour
{
    private OpcClient client;
    private OpcSubscription subscription;

    // Event, das von den Lamp-Skripten abonniert wird (Gibt Raumnummer und Zustand weiter)
    public event Action<int, bool> OnLampStateChanged;
    public event Action<int, int> OnLampSwitchCountChanged;
    // Queue, um OPC-Events sicher in den Unity Main-Thread zu leiten
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        try
        {

            // Nur fürs builden auskommentieren! Sonst fatal error auf PC
            // string certFolder = Path.Combine(Application.persistentDataPath, "OPC");
            // Directory.CreateDirectory(certFolder);
            // Environment.CurrentDirectory = certFolder;

            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            Opc.UaFx.OpcSecurityPolicy myOPCUASecurityPolicy = new Opc.UaFx.OpcSecurityPolicy(Opc.UaFx.OpcSecurityMode.None);
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();
            this.subscription = client.SubscribeNodes();

            string[] nodeIds = {
                // Room 1
                "ns=6;s=::room1:Lampe",
                "ns=6;s=::room1:LampeRT",
                "ns=6;s=::room1:LampeSwitchCnt",
                // Room 2
                "ns=6;s=::room2:Lampe",
                "ns=6;s=::room2:LampeRT",
                "ns=6;s=::room2:LampeSwitchCnt",
                // Room 3
                "ns=6;s=::room3:Lampe",
                "ns=6;s=::room3:LampeRT",
                "ns=6;s=::room3:LampeSwitchCnt",
            };

            for (int index = 0; index < nodeIds.Length; index++)
            {
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;
                item.Tag = index;
                item.SamplingInterval = 200;
                this.subscription.AddMonitoredItem(item);
            }

            this.subscription.ApplyChanges();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }

    // Führt die gesammelten Aktionen sicher im Main Thread aus
    void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    public OpcClient GetClient() { return this.client; }

    public void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        OpcMonitoredItem item = (OpcMonitoredItem)sender;
        string nodeId = item.NodeId.ToString();

        Debug.Log($"Data Change from Node: {nodeId} Value: {e.Item.Value}");

        int roomNumber = 0;

        if (nodeId.Contains("room1")) roomNumber = 1;
        else if (nodeId.Contains("room2")) roomNumber = 2;
        else if (nodeId.Contains("room3")) roomNumber = 3;

        if (roomNumber == 0) return;

        if (nodeId.Contains(":Lampe") && !nodeId.Contains("RT") && !nodeId.Contains("SwitchCnt"))
        {
            bool newState = Convert.ToBoolean(e.Item.Value.Value);

            mainThreadActions.Enqueue(() =>
            {
                OnLampStateChanged?.Invoke(roomNumber, newState);
            });
        }

        if (nodeId.Contains("LampeSwitchCnt"))
        {
            int switchCnt = Convert.ToInt32(e.Item.Value.Value);

            mainThreadActions.Enqueue(() =>
            {
                OnLampSwitchCountChanged?.Invoke(roomNumber, switchCnt);
            });
        }
    }

    // Die Trennung gehört ins Client-Skript, nicht in die Lampe!
    void OnApplicationQuit()
    {
        if (this.client != null)
        {
            this.client.Disconnect();
            Debug.Log("OPC Client disconnected.");
        }
    }
}