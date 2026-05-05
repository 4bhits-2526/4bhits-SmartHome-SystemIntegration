using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRButton : MonoBehaviour
{
    XRSimpleInteractable m_Interactable;

    public GameObject SwitchOn;
    public GameObject SwitchOff;

    bool isOn = false;

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
        isOn = !isOn;

        FlipSwitchVisual();

        Debug.Log("VR Button Pressed!");
    }

    void FlipSwitchVisual()
    {
        if (isOn) {
            SwitchOff.SetActive(false);
            SwitchOn.SetActive(true);
        } else
        {
            SwitchOn.SetActive(false);
            SwitchOff.SetActive(true);
        }
    }
}
