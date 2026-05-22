using UnityEngine;

public class Lamp : MonoBehaviour
{
    // Die Zuweisung kann im Unity Editor erfolgen (via Drag & Drop)
    public OpcUaClientBehaviour opcUaClient; 
    public int roomNumber;
    public GameObject lampVisual;

    void Start()
    {
        // Wir abonnieren das Event des Central Clients
        if (opcUaClient != null)
        {
            opcUaClient.OnLampStateChanged += HandleLampChange;
        }
        else
        {
            Debug.LogError($"Lamp in Room {roomNumber} hat keine Referenz zum OpcUaClientBehaviour!");
        }
    }

    // WICHTIG: Event abbestellen, wenn das Objekt zerstört wird, um Memory Leaks zu vermeiden
    void OnDestroy()
    {
        if (opcUaClient != null)
        {
            opcUaClient.OnLampStateChanged -= HandleLampChange;
        }
    }

    private void HandleLampChange(int changedRoom, bool newState)
    {
        // Nur reagieren, wenn MEIN Raum gemeint ist
        if (changedRoom == this.roomNumber)
        {
            SetLampState(newState);
        }
    }

    public void SetLampState(bool state)
    {
        Debug.Log($"Lamp {roomNumber} set to: {state}");
        if (lampVisual != null)
        {
            lampVisual.SetActive(state);
        }
    }
}