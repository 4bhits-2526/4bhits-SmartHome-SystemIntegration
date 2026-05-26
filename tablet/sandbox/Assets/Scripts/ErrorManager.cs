using UnityEngine;
using UnityEngine.UI;
using Opc.UaFx.Client;
using System;

public class ErrorManager : MonoBehaviour
{
    private bool connection_status = false;
    private bool isConnecting = true;

    private OpcClient client;
    private OpcSubscription subscription;

    [Header("Connection Status UI")]
    [SerializeField] private Image statusImage;

    [Header("Status Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color connectingColor = Color.yellow;
    [SerializeField] private Color disconnectedColor = Color.red;

    void Start()
    {
        // Beim Start erstmal gelb anzeigen
        SetStatusColor(connectingColor);

        try
        {
            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");

            this.client.Security.UserIdentity =
                new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();

            Debug.Log("Connected to OPC UA server!");

            connection_status = true;
            isConnecting = false;

            // Grün = connected
            SetStatusColor(connectedColor);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);

            connection_status = false;
            isConnecting = false;

            // Rot = disconnected
            SetStatusColor(disconnectedColor);
        }
    }

    void Update()
    {
        if (isConnecting)
        {
            SetStatusColor(connectingColor);
        }
        else if (connection_status)
        {
            SetStatusColor(connectedColor);
        }
        else
        {
            SetStatusColor(disconnectedColor);
            Debug.LogError("Not connected to OPC UA server!");
        }
    }

    private void SetStatusColor(Color color)
    {
        if (statusImage != null)
        {
            statusImage.color = color;
        }
    }
}