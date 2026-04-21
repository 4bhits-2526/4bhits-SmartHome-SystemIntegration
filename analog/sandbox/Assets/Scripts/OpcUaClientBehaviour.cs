// Copyright (c) Traeger Industry Components GmbH. All Rights Reserved.
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Opc.UaFx;
using Opc.UaFx.Client;
public class OpcUaClientBehaviour : MonoBehaviour
{
    private OpcClient client;
    private Text statusText;
    private Text statusText4;
    private Text statusText3;
    private OpcSubscription subscription;
    // 👉 LIGHT aus Unity (im Inspector setzen!)
    public Light zielLicht;
 
    // 👉 HIER im Inspector pro Taster einstellen: "room1", "room2" oder "room3"
    public string roomName = "room1";
    // 👉 Taster Positionen (JETZT DYNAMISCH)
    private Vector3 startPosition;
    private Vector3 gedruecktPosition;
    // 👉 Wie weit der Knopf runter geht
    private float klickOffset = 0.00077f;
    private bool istGedrueckt = false;
    // 👉 NEU: Toggle für Unity-Licht
    private bool lichtAn = false;
    void Start()
    {
        // licht soll zu Beginn AUS sein
        if (zielLicht != null)
        {
            zielLicht.enabled = false;
        }
        this.statusText = GameObject.Find("statusText").GetComponent<Text>();
        this.statusText4 = GameObject.Find("statusText4").GetComponent<Text>();
        this.statusText3 = GameObject.Find("statusText3").GetComponent<Text>();
        this.statusText.text = "Connecting...";
        this.statusText4.text = "Connecting4...";
        this.statusText3.text = "Info3...";
        // 👉 Startposition dynamisch setzen
        startPosition = transform.localPosition;
        gedruecktPosition = startPosition - new Vector3(0f, klickOffset, 0f);
        transform.localPosition = startPosition;
        SendFalse();
        try
        {
            this.client = new OpcClient("opc.tcp://127.0.0.1:4840");
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");
            this.client.Connect();
            this.statusText.text = "Connected!";
            // 👉 Nur die NodeIds für den eigenen Room abonnieren
            string[] nodeIds = {
             //   "ns=6;s=::opctest:mySinValue",
                "ns=6;s=::AsGlobalPV:gSchweibsChange",
                "ns=6;s=::AsGlobalPV:gSchweibsWrite",
                $"ns=6;s=::{roomName}:Lampe",
                $"ns=6;s=::{roomName}:SwitchValueW",
                $"ns=6;s=::{roomName}:SwitchValue"
            };
            this.subscription = this.client.SubscribeNodes();
            for (int index = 0; index < nodeIds.Length; index++)
            {
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;
                item.Tag = index;
                item.SamplingInterval = 200;
                this.subscription.AddMonitoredItem(item);
            }
            this.subscription.ApplyChanges();
            this.statusText3.text = "Subscribed! Room: " + roomName;
        }
        catch (Exception ex)
        {
            if (ex is TypeInitializationException tiex)
                ex = tiex.InnerException;
            this.statusText.text += Environment.NewLine
                + ex.GetType().Name + ": " + ex.Message
                + Environment.NewLine + ex.StackTrace;
        }
    }
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckClick(true);
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CheckClick(false);
        }
    }
    void CheckClick(bool pressed)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                if (pressed)
                    KnopfRunter();
                else
                    KnopfHoch();
            }
        }
    }
    void KnopfRunter()
    {
        transform.localPosition = gedruecktPosition;
        if (!istGedrueckt)
        {
            istGedrueckt = true;
            // 👉 Unity Licht toggeln
            lichtAn = !lichtAn;
            if (zielLicht != null)
            {
                zielLicht.enabled = lichtAn;
            }
            SendTrue();
        }
    }
    void KnopfHoch()
    {
        transform.localPosition = startPosition;
        if (istGedrueckt)
        {
            istGedrueckt = false;
            SendFalse();
        }
    }
    void SendTrue()
    {
        Debug.Log($"Taster GEDRÜCKT ({roomName})");
        if (client != null)
        {
            // 👉 Schreibt nur in den eigenen Room
            client.WriteNode($"ns=6;s=::{roomName}:SwitchValueW", (Boolean)true);
        }
    }
    void SendFalse()
    {
        Debug.Log($"Taster LOSGELASSEN ({roomName})");
        if (client != null)
        {
            // 👉 Schreibt nur in den eigenen Room
            client.WriteNode($"ns=6;s=::{roomName}:SwitchValueW", (Boolean)false);
        }
    }
    void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        OpcMonitoredItem item = (OpcMonitoredItem)sender;
        if (item.NodeId.ToString().Contains("ns=6;s=::AsGlobalPV:gSchweibsChange"))
        {
            this.statusText.text = e.Item.Value.Value?.ToString() ?? "null";
        }
        else if (item.NodeId.ToString().Contains($"::{roomName}:Lampe"))
        {
            bool lampState = false;
            if (e.Item.Value.Value != null)
            {
                lampState = (bool)e.Item.Value.Value;
            }
            Debug.Log($"Lampe ({roomName}) OPC: " + lampState);
            this.statusText4.text = roomName + " Lampe: " + lampState.ToString();
        }
        else
        {
            Debug.Log("Data Change: " + item.NodeId + " = " + e.Item.Value);
        }
    }
}