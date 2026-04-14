using UnityEngine;
using System;
using UnityEngine.EventSystems;

using Opc.UaFx;
using Opc.UaFx.Client;
using TMPro;

public class Lamp : MonoBehaviour
{
    private OpcClient client;
    public int roomNumber;


    void Start()
    {
        try
        {
            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();
            Debug.Log("Connected to OPC UA server!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            bool roomLampState = (bool)this.client.ReadNode("ns=6;s=::room " + this.roomNumber + ":Lampe").Value;
            enabled = roomLampState;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error reading lamp states from OPC UA server: " + ex.Message);
        }
    }
}
