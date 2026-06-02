using UnityEngine;
using UnityEngine.UI;
using Opc.UaFx.Client;
using System;
using TMPro;

public class ErrorManager : MonoBehaviour
{
    public OpcUaClientBehaviour opcClient;

    [Header("Connection Status UI")]
    [SerializeField] private Image statusImage;
    [SerializeField] private TMP_Text statusText;

    [Header("Status Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color connectingColor = Color.yellow;
    [SerializeField] private Color disconnectedColor = Color.red;

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
                SetStatus(connectedColor, "Connected");
                break;
            case OpcClientState.Reconnected:
                SetStatus(connectedColor, "Connected");
                break;
            case OpcClientState.Connecting:
                SetStatus(connectingColor, "Connecting...");
                break;
            case OpcClientState.Reconnecting:
                SetStatus(connectingColor, "Reconnecting...");
                break;
            case OpcClientState.Disconnected:
                SetStatus(disconnectedColor, "Disconnected");
                break;
        }
    }

    private void SetStatus(Color color, string text)
    {
        if (statusImage != null && statusText != null)
        {
            statusImage.color = color;
            statusText.text = text;
        }
    }
}