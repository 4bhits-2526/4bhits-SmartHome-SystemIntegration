using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Opc.UaFx;
using Opc.UaFx.Client;
using TMPro;

public class OpcUaClientBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private OpcClient client;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI statusText4;
    private TextMeshProUGUI statusText3;
    private OpcSubscription subscription;

    // Boolwerte für die Lampen
    private bool room1Lamp1;
    private bool room1Lamp2;
    private bool room2Lamp1;
    private bool room3Lamp1;

    // Public Variablen
    public GameObject Switch;
    public int SwitchCode;

    public Color LampColor = Color.yellow;
    public Light[] Lamps;


    void Start()
    {
        /*
        this.statusText = GameObject.Find("statusText").GetComponent<TextMeshProUGUI>();
        this.statusText4 = GameObject.Find("statusText4").GetComponent<TextMeshProUGUI>();
        this.statusText3 = GameObject.Find("statusText3").GetComponent<TextMeshProUGUI>();

        this.statusText.text = "Connecting...";
        this.statusText4.text = "Connecting4...";
        this.statusText3.text = "Info3...";
        this.buttonLight.color = normalColor;
        */

        try
        {
            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();
            this.statusText.text = "Connected!";


            string[] nodeIds = {

            /*
            "ns=6;s=::opctest:mySinValue",
            "ns=6;s=::AsGlobalPV:gSchweibsChange",
            "ns=6;s=::AsGlobalPV:gSchweibsWrite",
            */

            // Room 1
            "ns=6;s=::room1:Lampe",
            "ns=6;s=::room1:SwitchValueT",
            "ns=6;s=::room1:SwitchValue",
            // Room 2
            "ns=6;s=::room2:Lampe",
            "ns=6;s=::room2:SwitchValueT",
            "ns=6;s=::room2:SwitchValue",
            // Room 3
            "ns=6;s=::room3:Lampe",
            "ns=6;s=::room3:SwitchValueT",
            "ns=6;s=::room3:SwitchValue"
        };


            this.subscription = this.client.SubscribeNodes();

            for (int i = 0; i < nodeIds.Length; i++)
            {
                var item = new OpcMonitoredItem(nodeIds[i], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;
                item.Tag = i;
                item.SamplingInterval = 200;

                this.subscription.AddMonitoredItem(item);
            }

            this.subscription.ApplyChanges();
            this.statusText3.text = "Subscribed!";
        }
        catch (Exception ex)
        {
            if (ex is TypeInitializationException tiex)
                ex = tiex.InnerException;

            this.statusText.text += "\n" + ex.Message;
        }
    }

    void Update()
    {
        room1Lamp1 = (bool)this.client.ReadNode("ns=6;s=::room1:Lampe").Value;
        room1Lamp2 = (bool)this.client.ReadNode("ns=6;s=::room1:Lampe").Value;
        room2Lamp1 = (bool)this.client.ReadNode("ns=6;s=::room2:Lampe").Value;
        room3Lamp1 = (bool)this.client.ReadNode("ns=6;s=::room3:Lampe").Value;

        if (room1Lamp1 == true)
        {
            if(SwitchCode == 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Lamps[i].enabled = true;
                }
            }
        } else
        {
            if (SwitchCode == 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Lamps[i].enabled = false;
                }
            }
        }

        if(room1Lamp2 == true)
        {
            if (SwitchCode == 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Lamps[i].enabled = true;
                }
            }
        } else
        {
            if (SwitchCode == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Lamps[i].enabled = false;
                }
            }
        }

        if (room2Lamp1 == true)
        {
            if (SwitchCode == 2)
            {
                for (int i = 0; i < 1; i++)
                {
                    Lamps[i].enabled = true;
                }
            }
        } else
        {
            if (SwitchCode == 2)
            {
                for (int i = 0; i < 1; i++)
                {
                    Lamps[i].enabled = false;
                }
            }
        }

        if (room3Lamp1 == true)
        {
            if (SwitchCode == 3)
            {
                for (int i = 0; i < 1; i++)
                {
                    Lamps[i].enabled = true;
                }
            }
        } else
        {
            if (SwitchCode == 3)
            {
                for (int i = 0; i < 1; i++)
                {
                    Lamps[i].enabled = false;
                }
            }
        }

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Licht AN");
        // Rotation des Lichtschalters in Unity anpassen

        Switch.transform.localRotation = Quaternion.Euler(0, 0, 5); // Beispielrotation, anpassen je nach Bedarf
        // buttonLight.color = pressedColor;

        try
        {
            this.client.WriteNode("ns=6;s=::room" + SwitchCode + ":SwitchValueT", true);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Licht AUS");

        Switch.transform.localRotation = Quaternion.Euler(0, 0, 0); // Zurück zur ursprünglichen Rotation
        // buttonLight.color = normalColor;

        try
        {
            this.client.WriteNode("ns=6;s=::room" + SwitchCode + ":SwitchValueT", false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        OpcMonitoredItem item = (OpcMonitoredItem)sender;

        if (item.NodeId.ToString().Contains("gSchweibsChange"))
        {
            this.statusText.text = e.Item.Value.Value?.ToString() ?? "null";
        }
        else if (item.NodeId.ToString().Contains("mySinValue"))
        {
            this.statusText4.text = e.Item.Value.Value?.ToString() ?? "null";
        }
        else
        {
            Debug.Log("Data Change: " + item.NodeId + " = " + e.Item.Value);
        }
    }

}
