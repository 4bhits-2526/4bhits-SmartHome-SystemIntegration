using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Opc.UaFx;
using Opc.UaFx.Client;
using TMPro;
using Unity.VisualScripting;

public class OpcUaClientBehaviour : MonoBehaviour
{
    private OpcClient client;
    private OpcSubscription subscription;

    // Boolwerte für die Lampen
    private bool room1Lamp1;
    private bool room1Lamp2;
    private bool room2Lamp1;
    private bool room3Lamp1;


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

            OpcSubscription subscription = GetSubscription();

            // .Log("Subscription erstellt für alle Lampen, RTs, und SwitchCounts in allen Räumen!");


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

            subscription = client.SubscribeNodes();

            for (int index = 0; index < nodeIds.Length; index++)
            {
                // Create an OpcMonitoredItem for the NodeId.
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;

                // You can set your own values on the "Tag" property
                // that allows you to identify the source later.
                item.Tag = index;

                // Set a custom sampling interval on the 
                // monitored item.
                item.SamplingInterval = 200;

                // Add the item to the subscription.
                this.subscription.AddMonitoredItem(item);
            }

            // After adding the items (or configuring the subscription), apply the changes.
            this.subscription.ApplyChanges();
        }
        catch (Exception ex)
        {
            if (ex is TypeInitializationException tiex)
                ex = tiex.InnerException;

            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }

    public OpcClient GetClient()
    {
        return this.client;
    }

    public OpcSubscription GetSubscription()
    {
        return this.subscription;
    }

    public void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        OpcMonitoredItem item = (OpcMonitoredItem)sender;

        Debug.Log("Data Change from Index : " + 
        item.Tag + " : " + item.NodeId.ToString() + " : " + e.Item.Value + ":" + e.Item.Value.DataType);

        if (item.NodeId.ToString().Contains("room1:Lampe"))
        {
            
        }

    }

    // --------------------------------------------------------------------------------

    /*   public void OnPointerDown(PointerEventData eventData)

        {

            Switch.transform.localRotation = Quaternion.Euler(0, 0, 5);

            try
            {
                if (this.client != null)
                    this.client.WriteNode("ns=6;s=::room" + roomNumber + ":SwitchValueT", true);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }


        public void OnPointerUp(PointerEventData eventData)
        {

            Switch.transform.localRotation = Quaternion.Euler(0, 0, 0);

            try
            {
                if (this.client != null)
                    this.client.WriteNode("ns=6;s=::room" + roomNumber + ":SwitchValueT", false);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    */
}