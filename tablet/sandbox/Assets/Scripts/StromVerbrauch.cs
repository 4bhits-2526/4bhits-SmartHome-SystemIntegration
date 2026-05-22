using UnityEngine;
using TMPro;

public class StromVerbrauch : MonoBehaviour
{
    public TMP_Text lampText1;
    public TMP_Text lampText2;
    public TMP_Text lampText3;

    public TMP_Text costText1;
    public TMP_Text costText2;
    public TMP_Text costText3;

    public float pricePerKWh = 0.30f;

    // 👉 Lampen pro Raum
    public int room1Lamps = 2;
    public int room2Lamps = 1;
    public int room3Lamps = 1;

    private string lastText1;
    private string lastText2;
    private string lastText3;

    void Update()
    {
        if (HasChanged())
        {
            UpdateCosts();
        }
    }

    bool HasChanged()
    {
        return lampText1.text != lastText1 ||
               lampText2.text != lastText2 ||
               lampText3.text != lastText3;
    }

    public void UpdateCosts()
    {
        Calculate(lampText1, costText1, room1Lamps, ref lastText1);
        Calculate(lampText2, costText2, room2Lamps, ref lastText2);
        Calculate(lampText3, costText3, room3Lamps, ref lastText3);
    }

    void Calculate(TMP_Text input, TMP_Text output, int lampCount, ref string lastValue)
    {
        if (input == null || output == null) return;

        string text = input.text;

        if (text == lastValue) return;
        lastValue = text;

        try
        {
            float seconds = float.Parse(text.Split(':')[1].Trim());

            float powerKW = 3f / 1000f;

            float costPerSecond = (powerKW / 3600f) * pricePerKWh;

            // 👉 HIER kommt der Multiplikator rein
            float totalCost = costPerSecond * seconds * lampCount;

            output.text = totalCost.ToString("F6") + " €";
        }
        catch
        {
            Debug.LogWarning("Parse Fehler: " + text);
        }
    }
}