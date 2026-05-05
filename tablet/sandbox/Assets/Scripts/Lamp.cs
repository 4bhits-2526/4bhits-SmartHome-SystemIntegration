using UnityEngine;
using System;
using Opc.UaFx;
using Opc.UaFx.Client;


public class Lamp : MonoBehaviour
{
    private OpcClient client;
    private OpcSubscription subscription;

    public int roomNumber;

    // ZIEHE HIER IM INSPECTOR DIE GRAFIK/DAS LICHT DEINER LAMPE REIN
    public GameObject lampVisual;

    void Start()
    {
        try
        {
            this.client = new OpcClient("opc.tcp://192.168.1.61:4840/");
            // Opc.UaFx.OpcSecurityPolicy myOPCUASecurityPolicy = new Opc.UaFx.OpcSecurityPolicy(Opc.UaFx.OpcSecurityMode.None);
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");

            this.client.Connect();
            Debug.Log("Connected to OPC UA server!");

            this.subscription = this.client.SubscribeDataChange(
                "ns=6;s=::room" + roomNumber + ":Lampe",
                OnLampValueChanged
            );
            Debug.Log("Subscription erstellt für Raum " + roomNumber);


        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }

    private void OnLampValueChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        try
        {
            bool newState = (bool)e.Item.Value.Value;

            Debug.Log("OPC Update für Raum " + roomNumber + ": " + newState);

            SetLampState(newState);
        }
        catch (Exception ex)
        {
            Debug.LogError("Fehler im Callback: " + ex.Message);
        }
    }




    public void SetLampState(bool state)
    {
        Debug.Log("Lamp " + roomNumber + " set to: " + state);

        if (lampVisual != null)
        {
            lampVisual.SetActive(state);
        }
    }

    // GANZ WICHTIG: Verbindung sauber trennen, wenn das Spiel beendet wird!
    void OnApplicationQuit()
    {
        if (this.client != null)
        {
            this.client.Disconnect();
        }
    }
}