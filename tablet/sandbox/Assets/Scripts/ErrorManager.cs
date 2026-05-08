using UnityEditor.MemoryProfiler;
using UnityEngine;
using Opc.UaFx.Client;
using System;

public class ErrorManager : MonoBehaviour
{
    private bool connection_status;

    private OpcClient client;
    private OpcSubscription subscription;

    public 

    void Start()
    {
        try
        {
            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            // Opc.UaFx.OpcSecurityPolicy myOPCUASecurityPolicy = new Opc.UaFx.OpcSecurityPolicy(Opc.UaFx.OpcSecurityMode.None);
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();
            Debug.Log("Connected to OPC UA server!");
            connection_status = true;


        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
            connection_status = false;
        }
    }

    void Update()
    {
        if (!connection_status)
        {
            Debug.LogError("Not connected to OPC UA server!");
        }

    }
}
