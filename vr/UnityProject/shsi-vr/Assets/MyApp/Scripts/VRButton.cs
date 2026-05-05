using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRButton : MonoBehaviour
{
    public TcpClientUnity tcpClient;
    public int lampIndex; // 1, 2, or 3

    XRSimpleInteractable m_Interactable;

    void Awake()
    {
        m_Interactable = GetComponent<XRSimpleInteractable>();

        if (m_Interactable == null)
        {
            Debug.LogError($"{nameof(VRButton)} requires an {nameof(XRSimpleInteractable)} on the same GameObject.", this);
        }
    }

    void OnEnable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        OnPress();
    }

    public void OnPress()
    {
        if (tcpClient != null)
            tcpClient.ToggleLamp(lampIndex);
    }
}