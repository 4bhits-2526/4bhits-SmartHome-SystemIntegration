using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class AutoImageSlideshow : MonoBehaviour
{
    [Header("UI (Tablet)")]
    public Canvas canvas;
    public Image tabletImage;

    [Header("VR")]
    public Renderer vrDisplay;

    [Header("Folder Settings")]
    public string imageFolder = "Images";

    [Header("Slideshow Settings")]
    public float switchTime = 5f;

    private List<Sprite> sprites = new List<Sprite>();
    private int currentIndex = 0;

    private bool isVR;
    // Schauen wie man prüft ob es auf tablet ider Vr läuft und schaune was umgestellt werden muss für funktion auf beidem

    void Start()
    {
        DetectMode();
        SetCanvasMode();

        StartCoroutine(LoadImages());
    }

    void DetectMode()
    {
        isVR = XRSettings.isDeviceActive;
    }

    void SetCanvasMode()
    {
        if (canvas == null) return;

        if (isVR)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    IEnumerator LoadImages()
    {
        string path = Path.Combine(Application.streamingAssetsPath, imageFolder);

        if (!Directory.Exists(path))
        {
            Debug.LogError("Ordner nicht gefunden: " + path);
            yield break;
        }

        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
            {
                byte[] data = File.ReadAllBytes(file);

                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(data))
                {
                    Sprite sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f)
                    );

                    sprites.Add(sprite);
                }
            }
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning("Keine Bilder gefunden.");
            yield break;
        }

        StartCoroutine(Slideshow());
    }

    IEnumerator Slideshow()
    {
        while (true)
        {
            Sprite current = sprites[currentIndex];

            if (tabletImage != null)
                tabletImage.sprite = current;

            if (vrDisplay != null)
                vrDisplay.sharedMaterial.mainTexture = current.texture;

            currentIndex = (currentIndex + 1) % sprites.Count;

            yield return new WaitForSeconds(switchTime);
        }
    }
}

