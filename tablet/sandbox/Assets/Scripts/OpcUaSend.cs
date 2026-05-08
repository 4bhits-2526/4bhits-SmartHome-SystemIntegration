using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class OpcUaSend : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public OpcUaClientBehaviour opcUaClient;


    void Start()
    {
        try
        {
            opcUaClient.GetClient().Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error connecting to OPC UA server: " + ex.Message);
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {

        opcUaClient.Switch.transform.localRotation = Quaternion.Euler(0, 0, 5);

        try
        {
            if (this.opcUaClient.GetClient() != null)
                this.opcUaClient.GetClient().WriteNode("ns=6;s=::room" + opcUaClient.roomNumber + ":SwitchValueT", true);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {

        opcUaClient.Switch.transform.localRotation = Quaternion.Euler(0, 0, 0);

        try
        {
            if (this.opcUaClient.GetClient() != null)
                this.opcUaClient.GetClient().WriteNode("ns=6;s=::room" + opcUaClient.roomNumber + ":SwitchValueT", false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
