using TMPro;
using UnityEngine;

public class RoomDropdownButton : MonoBehaviour
{
    public TMP_Text buttonText;
    public TMP_Text buttonText2;
    public TMP_Text buttonText3;

    private bool isOpen1 = false;
    private bool isOpen2 = false;
    private bool isOpen3 = false;

    public void ToggleDropdown1()
    {
        isOpen1 = !isOpen1;

        if (isOpen1)
        {
            buttonText.text = "\nClick me! ▲";

        }
        else
        {
            buttonText.text = "\nClick me! ▼";

        }
    }

    public void ToggleDropdown2()
    {
        isOpen2 = !isOpen2;
        if (isOpen2)
        {
             buttonText2.text = "\nClick me! ▲";
        }
        else
        {
            buttonText2.text = "\nClick me! ▼";
        }
    }

    public void ToggleDropdown3()
    {
        isOpen3 = !isOpen3;
        if (isOpen3)
        {
            buttonText3.text = "\nClick me! ▲";
        }
        else
        {
            buttonText3.text = "\nClick me! ▼";
        }
    }
}

           
            

            
            