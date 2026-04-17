using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Opc.UaFx;
using Opc.UaFx.Client;
using TMPro;

public class OpcUaClientBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private OpcClient client;
    private OpcSubscription subscription;

    // Boolwerte für die Lampen
    private bool room1Lamp1;
    private bool room1Lamp2;
    private bool room2Lamp1;
    private bool room3Lamp1;

    // Public Variablen
    public GameObject Switch;
    public int roomNumber;


    void Start()
    {
        try
        {
            string certFolder = Path.Combine(Application.persistentDataPath, "OPC");
            Directory.CreateDirectory(certFolder);
            Environment.CurrentDirectory = certFolder;

            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            Opc.UaFx.OpcSecurityPolicy myOPCUASecurityPolicy = new Opc.UaFx.OpcSecurityPolicy(Opc.UaFx.OpcSecurityMode.None);
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();


            string[] nodeIds = {

            /*
            "ns=6;s=::opctest:mySinValue",
            "ns=6;s=::AsGlobalPV:gSchweibsChange",
            "ns=6;s=::AsGlobalPV:gSchweibsWrite",
            */

            // Room 1
            "ns=6;s=::room1:SwitchValueT",

            // Room 2
            "ns=6;s=::room2:SwitchValueT",

            // Room 3
            "ns=6;s=::room3:SwitchValueT",

            };
        }
        catch (Exception ex)
        {
            if (ex is TypeInitializationException tiex)
                ex = tiex.InnerException;

            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
<<<<<<< HEAD
        Debug.Log("Licht in Room " + roomNumber +" AN");
        // Rotation des Lichtschalters in Unity anpassen
=======
        Debug.Log("Licht AN");
>>>>>>> a0c1c57ef5e2d8e81b09fead6146913130a139dd

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
        Debug.Log("Licht in Room " + roomNumber +" AUS");

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

}