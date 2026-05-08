using UnityEngine;
using TMPro;

public class EnergyManager : MonoBehaviour
{
    [System.Serializable]
    public class Lamp
    {
        public GameObject lampObject;
        public float watt = 3f;
        public float costPerKWh = 0.30f;

        [HideInInspector]
        public float totalCost;
    }

    public Lamp[] lamps;

    public TextMeshProUGUI totalCostText;

    void Update()
    {
        float total = 0f;

        foreach (Lamp lamp in lamps)
        {
            if (lamp.lampObject != null && lamp.lampObject.activeSelf)
            {
                float kW = lamp.watt / 1000f;
                float kWhPerSecond = kW / 3600f;

                lamp.totalCost += kWhPerSecond * lamp.costPerKWh * Time.deltaTime;
            }

            total += lamp.totalCost;
        }

        UpdateUI(total);
    }

    void UpdateUI(float total)
    {
        if (totalCostText != null)
        {
            totalCostText.text = total.ToString("F6") + " €";
        }
    }
}