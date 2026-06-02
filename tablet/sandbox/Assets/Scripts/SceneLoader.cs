using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelOne;
    public GameObject panelTwo;
    public GameObject panelThree;

    // private void LoadRoomOne()
    // {
    //     SceneManager.LoadScene("RoomOne");
    // }

    // private void LoadRoomTwo()
    // {
    //     SceneManager.LoadScene("RoomTwo");
    // }

    // private void LoadRoomThree()
    // {
    //     SceneManager.LoadScene("RoomThree");
    // }

    public void GoBackToOPCUA()
    {
        SceneManager.LoadScene("CameraTestScene");
    }

    public void ShowPanelOne()
    {
        if(panelOne.activeInHierarchy == false)
        {
            panelOne.SetActive(true);
        }
        else
        {
            panelOne.SetActive(false);
        }
    }
    public void ShowPanelTwo()
    {
        if(panelTwo.activeInHierarchy == false)
        {
            panelTwo.SetActive(true);
        }
        else
        {
            panelTwo.SetActive(false);
        }
    }
    public void ShowPanelThree()
    {
        if(panelThree.activeInHierarchy == false)
        {
            panelThree.SetActive(true);
        }
        else
        {
            panelThree.SetActive(false);
        }
    }
}
