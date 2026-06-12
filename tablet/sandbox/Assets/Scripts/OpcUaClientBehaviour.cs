using System;
using System.IO;
using UnityEngine;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Collections.Concurrent;
using Unity.VisualScripting; // WICHTIG für Thread-Sicherheit

public class OpcUaClientBehaviour : MonoBehaviour
{
    private bool connection_status = false;
    private bool isConnecting = true;
    private OpcClient client;
    private OpcSubscription subscription;

    // Event, das von den Lamp-Skripten abonniert wird (Gibt Raumnummer und Zustand weiter)
    public event Action<int, bool> OnLampStateChanged;
    public event Action<int, int> OnLampSwitchCountChanged;

    public event Action<OpcClientState> OnConnectionStatusChanged;
    // Queue, um OPC-Events sicher in den Unity Main-Thread zu leiten
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();


    #region OpcUaClient Setup

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {

            // Nur fürs builden auskommentieren! Sonst fatal error auf PC
            string certFolder = Path.Combine(Application.persistentDataPath, "OPC");
            Directory.CreateDirectory(certFolder);
            Environment.CurrentDirectory = certFolder;

            this.isConnecting = true;

            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            this.client.StateChanged += Client_StateChanged;
            Opc.UaFx.OpcSecurityPolicy myOPCUASecurityPolicy = new Opc.UaFx.OpcSecurityPolicy(Opc.UaFx.OpcSecurityMode.None);
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();

            this.connection_status = true;
            this.isConnecting = false;

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
            // Variablen auf false setzten um den Connection Status richtig anzuzeigen
            this.connection_status = false;
            this.isConnecting = false;
            Debug.Log(connection_status);
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

    #endregion
    #region Event Handler
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


        // Function for Connection Status
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

    #endregion
    private void Client_StateChanged(object sender, OpcClientStateChangedEventArgs e)
    {
        // The tag property contains the previously set value.
        OpcClient item = (OpcClient)sender;

        Debug.Log((
                    " Client_StateChange from Index {0}: {1}",


                    item.ToString(),
                    e.NewState.ToString(),
                    e.OldState.ToString(),
                    e.ToString()));

        if (e.NewState == OpcClientState.Connecting)
        {
            Debug.Log("OPC UA Client is connecting...");
        }
        if (e.NewState == OpcClientState.Reconnected)
        {
            Debug.Log("OPC UA Client is reconnected!");
        }
        if (e.NewState == OpcClientState.Connected)
        {
            Debug.Log("OPC UA Client connected.");
        }
        else if (e.NewState == OpcClientState.Disconnected)
        {
            Debug.LogWarning("OPC UA Client disconnected.");
        }
        else if (e.NewState == OpcClientState.Reconnecting)
        {
            Debug.Log("OPC UA Client reconnecting...");
        }

        mainThreadActions.Enqueue(() =>
        {
            OnConnectionStatusChanged?.Invoke(e.NewState);
        }); 
    }
}