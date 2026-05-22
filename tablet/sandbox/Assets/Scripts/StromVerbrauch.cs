using UnityEngine;
using TMPro;
using Opc.UaFx.Client;

public class StromVerbrauch : MonoBehaviour
{
    public OpcUaClientBehaviour opcBehaviour;

    public TMP_Text lampText1;
    public TMP_Text lampText2;
    public TMP_Text lampText3;

    public TMP_Text costText1;
    public TMP_Text costText2;
    public TMP_Text costText3;

    public float pricePerKWh = 0.30f;

    // Lampen pro Raum
    public int room1Lamps = 2;
    public int room2Lamps = 1;
    public int room3Lamps = 1;

    private OpcClient client;

    void Start()
    {
        if (opcBehaviour != null)
        {
            client = opcBehaviour.GetClient();
        }
    }

    void Update()
    {
        if (client == null) return;

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

    void UpdateRoom(string nodeId, TMP_Text lampText, TMP_Text costText, int lampCount)
{
    try
    {
        var value = client.ReadNode(nodeId);

        float seconds =
            System.Convert.ToSingle(value.Value);

        Debug.Log("ROOM UPDATE " + nodeId + " = " + seconds);

        lampText.text = "LIVE: " + seconds.ToString("F2");

        Debug.Log("TMP gesetzt: " + lampText.text);

        float powerKW = 3f / 1000f;
        float costPerSecond =
            (powerKW / 3600f) *
            pricePerKWh;

        float totalCost =
            costPerSecond *
            seconds *
            lampCount;

        costText.text =
            totalCost.ToString("F6") + " €";
    }
    catch (System.Exception ex)
    {
        Debug.LogError(ex);
    }
}
}