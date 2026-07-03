using UnityEngine;

public class RoomDropdownButton : MonoBehaviour
{
    [Header("Arrow Transforms")]
    public RectTransform arrow1;
    public RectTransform arrow2;
    public RectTransform arrow3;

    private bool isOpen1 = false;
    private bool isOpen2 = false;
    private bool isOpen3 = false;

    public void ToggleDropdown1()
    {
        isOpen1 = !isOpen1;
        RotateArrow(arrow1, isOpen1);
    }

    public void ToggleDropdown2()
    {
        isOpen2 = !isOpen2;
        RotateArrow(arrow2, isOpen2);
    }

    public void ToggleDropdown3()
    {
        isOpen3 = !isOpen3;
        RotateArrow(arrow3, isOpen3);
    }

    private void RotateArrow(RectTransform arrow, bool isOpen)
    {
        if (arrow == null) return;

        // geschlossen = 0°, offen = 180°
        float zRotation = isOpen ? 180f : 0f;
        arrow.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }
}