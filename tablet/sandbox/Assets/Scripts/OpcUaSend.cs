using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class OpcUaSend : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject Switch;
    public int roomNumber;

    public OpcUaClientBehaviour opcUaClient;

    public void OnPointerDown(PointerEventData eventData)
    {

        Switch.transform.localRotation = Quaternion.Euler(0, 0, 5);

        try
        {
            if (this.opcUaClient.GetClient() != null)
                this.opcUaClient.GetClient().WriteNode("ns=6;s=::room" + roomNumber + ":SwitchValueT", true);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {

        Switch.transform.localRotation = Quaternion.Euler(0, 0, 0);

        try
        {
            if (this.opcUaClient.GetClient() != null)
                this.opcUaClient.GetClient().WriteNode("ns=6;s=::room" + roomNumber + ":SwitchValueT", false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
