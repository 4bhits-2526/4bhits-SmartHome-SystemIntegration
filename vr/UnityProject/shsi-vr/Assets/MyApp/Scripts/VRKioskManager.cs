using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class VRKioskManager : MonoBehaviour
{
    [Header("Idle Settings")]
    [SerializeField] private float idleTimeout = 120f;

    [Header("Fade")]
    [SerializeField] private ScreenFader screenFader;

    private float lastActivityTime;

    private int sessionCount;
    private int resetCount;

    private bool isResetting;

    private void Start()
    {
        sessionCount = PlayerPrefs.GetInt("SessionCount", 0);
        resetCount = PlayerPrefs.GetInt("ResetCount", 0);

        sessionCount++;

        PlayerPrefs.SetInt("SessionCount", sessionCount);
        PlayerPrefs.Save();

        Debug.Log($"Session gestartet: {sessionCount}");

        RegisterActivity();
    }

    private void Update()
    {
        CheckDeveloperInput();
        CheckActivity();
        CheckIdleTimeout();
    }

    private void CheckDeveloperInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("R erkannt -> Reset");
            StartReset();
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Debug.Log("I erkannt -> Idle Reset simuliert");
            StartReset();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("P erkannt -> Pause simuliert");
            OnApplicationPause(true);
        }
    }

    private void CheckActivity()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            RegisterActivity();
        }
    }

    private void RegisterActivity()
    {
        lastActivityTime = Time.time;
    }

    private void CheckIdleTimeout()
    {
        if (isResetting)
            return;

        if (Time.time - lastActivityTime > idleTimeout)
        {
            Debug.Log("Idle Timeout erreicht");
            StartReset();
        }
    }

    private void StartReset()
    {
        if (isResetting)
            return;

        StartCoroutine(ResetSceneRoutine());
    }

    private IEnumerator ResetSceneRoutine()
    {
        isResetting = true;

        resetCount++;

        PlayerPrefs.SetInt("ResetCount", resetCount);
        PlayerPrefs.Save();

        Debug.Log($"Reset Nummer: {resetCount}");

        if (screenFader != null)
        {
            yield return screenFader.FadeOut();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("App pausiert");
        }
        else
        {
            Debug.Log("App fortgesetzt -> Reset");
            StartReset();
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Debug.Log("Focus zurück -> Reset");
            StartReset();
        }
    }
}