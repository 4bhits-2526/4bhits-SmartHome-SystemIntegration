using UnityEngine;
using TMPro;
using Opc.UaFx.Client;

public class StromVerbrauch : MonoBehaviour
{
    [Header("OPC")]
    public OpcUaClientBehaviour opcBehaviour;

    [Header("Live Laufzeit Texte")]
    public TMP_Text lampText1;
    public TMP_Text lampText2;
    public TMP_Text lampText3;

    [Header("Kosten Texte")]
    public TMP_Text costText1;
    public TMP_Text costText2;
    public TMP_Text costText3;

    [Header("Preis")]
    public float pricePerKWh = 0.30f;

    [Header("Lampen pro Raum")]
    public int room1Lamps = 2;
    public int room2Lamps = 1;
    public int room3Lamps = 1;

    private OpcClient client;

    void Update()
    {
        // Client erst holen wenn verfügbar
        if (client == null)
        {
            if (opcBehaviour == null)
            {
                Debug.LogError("StromVerbrauch: OpcUaClientBehaviour fehlt!");
                return;
            }

            client = opcBehaviour.GetClient();

            if (client == null)
            {
                Debug.Log("StromVerbrauch: OPC Client noch nicht bereit...");
                return;
            }

            Debug.Log("StromVerbrauch: OPC Client verbunden.");
        }

        // Räume aktualisieren
        UpdateRoom(
            "ns=6;s=::room1:LampeRT",
            lampText1,
            costText1,
            room1Lamps);

        UpdateRoom(
            "ns=6;s=::room2:LampeRT",
            lampText2,
            costText2,
            room2Lamps);

        UpdateRoom(
            "ns=6;s=::room3:LampeRT",
            lampText3,
            costText3,
            room3Lamps);
    }

    void UpdateRoom(
        string nodeId,
        TMP_Text lampText,
        TMP_Text costText,
        int lampCount)
    {
        try
        {
            // TMP prüfen
            if (lampText == null || costText == null)
            {
                Debug.LogError("TMP_Text Referenz fehlt für " + nodeId);
                return;
            }

            // Node lesen
            var value = client.ReadNode(nodeId);

            if (value == null || value.Value == null)
            {
                Debug.LogWarning("Keine Daten von Node: " + nodeId);
                return;
            }

            // Typ Debug
            //Debug.Log(
            //  "Node: " + nodeId +
            //  " Value: " + value.Value +
            //  " Type: " + value.Value.GetType());

            // Sekunden umwandeln
            float seconds = System.Convert.ToSingle(value.Value);

            // Live Anzeige
            lampText.text = "LIVE: " + seconds.ToString("F2") + " s";

            // Verbrauchsberechnung
            float powerKW = 3f / 1000f; // 3W Lampe
            float costPerSecond =
                (powerKW / 3600f) *
                pricePerKWh;

            float totalCost =
                costPerSecond *
                seconds *
                lampCount;

            // Kostenanzeige
            costText.text =
                totalCost.ToString("F6") + " €";
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                "Fehler bei Node " +
                nodeId +
                "\n" +
                ex.Message);
        }
    }
}