using System;
using System.IO;
using UnityEngine;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Collections.Concurrent; // WICHTIG für Thread-Sicherheit

public class OpcUaClientBehaviour : MonoBehaviour
{
    private bool connection_status = false;
    private bool isConnecting = true;
    private OpcClient client;
    private OpcSubscription subscription;

    // Event, das von den Lamp-Skripten abonniert wird (Gibt Raumnummer und Zustand weiter)
    public event Action<int, bool> OnLampStateChanged;

    // Queue, um OPC-Events sicher in den Unity Main-Thread zu leiten
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

#region OpcUaClient Setup

    void Start()
    {
        try
        {

            // Nur fürs builden auskommentieren! Sonst fatal error auf PC
            // string certFolder = Path.Combine(Application.persistentDataPath, "OPC");
            // Directory.CreateDirectory(certFolder);
            // Environment.CurrentDirectory = certFolder;

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

        // Prüfen, ob es sich um den reinen "Lampe" Knoten handelt (und nicht RT oder SwitchCnt)
        if (nodeId.Contains(":Lampe") && !nodeId.Contains("RT") && !nodeId.Contains("SwitchCnt"))
        {
            bool newState = (bool)e.Item.Value.Value;
            int roomNumber = 0;

            // Raumnummer aus der NodeId extrahieren
            if (nodeId.Contains("room1")) roomNumber = 1;
            else if (nodeId.Contains("room2")) roomNumber = 2;
            else if (nodeId.Contains("room3")) roomNumber = 3;

            if (roomNumber != 0)
            {
                // In die Queue für den Main Thread legen!
                mainThreadActions.Enqueue(() =>
                {
                    // Alle Abonnenten (Lampen) benachrichtigen
                    OnLampStateChanged?.Invoke(roomNumber, newState);
                });
            }
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
#region Static Function
    private static void Client_StateChanged(object sender, OpcClientStateChangedEventArgs e)
        {
            // The tag property contains the previously set value.
            OpcClient item = (OpcClient)sender;
 
           
 
            Console.WriteLine(
                        " Client_StateChange from Index {0}: {1}",

 
                        item.ToString(),
                        e.NewState.ToString(),
                        e.OldState.ToString(),
                        e.ToString());
        }

#endregion
}