using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRButton : MonoBehaviour
{
    public TcpClientUnity tcpClient;
    public int lampIndex;

    public GameObject SwitchOn;
    public GameObject SwitchOff;

    private XRSimpleInteractable m_Interactable;

    void Awake()
    {
        m_Interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        m_Interactable.selectEntered.AddListener(OnPressed);
        m_Interactable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        m_Interactable.selectEntered.RemoveListener(OnPressed);
        m_Interactable.selectExited.RemoveListener(OnReleased);
    }

    void OnPressed(SelectEnterEventArgs args)
    {
        if (SwitchOn != null)
            SwitchOn.SetActive(true);

        if (SwitchOff != null)
            SwitchOff.SetActive(false);

        tcpClient?.SendSwitchValue(lampIndex, true);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (SwitchOn != null)
            SwitchOn.SetActive(false);

        if (SwitchOff != null)
            SwitchOff.SetActive(true);

        tcpClient?.SendSwitchValue(lampIndex, false);
    }
}