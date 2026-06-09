using UnityEngine;
using UnityEngine.UI;
using Opc.UaFx.Client;
using System;
using TMPro;

public class ErrorManager : MonoBehaviour
{
    public OpcUaClientBehaviour opcClient;

    [Header("Connection Status UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image connectionStatus;
    [SerializeField] private Sprite connectionStatusConnected;
    [SerializeField] private Sprite connectionStatusDisconnected;
    [SerializeField] private Sprite connectionStatusIsConnecting;
    

// Funktion zum Aktualisieren der Verbindungsstatus-Farbe

    void Start()
    {
        // Wir abonnieren das Event des Central Clients
        if (opcClient != null)
        {
            opcClient.OnConnectionStatusChanged += HandleConnection;
            Debug.Log("Handle Conncection wurde registriert");
        }
        else
        {
            Debug.LogError($"ErrorManager hat keine Referenz zum OpcUaClientBehaviour!");
        }
    }

    private void HandleConnection(OpcClientState state)
    {
        Debug.Log("ConnectionStatus Handler called");
        // Aktualisiere die Statusfarbe basierend auf dem Verbindungsstatus
        switch (state)
        {
            case OpcClientState.Connected:
                SetStatus(connectionStatusConnected, "Connected");
                break;
            case OpcClientState.Reconnected:
                SetStatus(connectionStatusConnected, "Connected");
                break;
            case OpcClientState.Connecting:
                SetStatus(connectionStatusIsConnecting, "Connecting...");
                break;
            case OpcClientState.Reconnecting:
                SetStatus(connectionStatusIsConnecting, "Reconnecting...");
                break;
            case OpcClientState.Disconnected:
                SetStatus(connectionStatusDisconnected, "Disconnected");
                break;
        }
    }

    private void SetStatus(Sprite connection, string text)
    {
        if (connection != null && statusText != null)
        {
            connectionStatus.sprite = connection;
            statusText.text = text;
        }
    }
}