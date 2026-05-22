using UnityEngine;
using System;
using Opc.UaFx;
using Opc.UaFx.Client;
using UnityEngine.Events;


public class Lamp : MonoBehaviour
{
    private OpcUaClientBehaviour opcUaClient;

    public int roomNumber;

    public GameObject lampVisual;

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
        if (this.opcUaClient.GetClient() != null)
        {
            this.opcUaClient.GetClient().Disconnect();
        }
    }
}