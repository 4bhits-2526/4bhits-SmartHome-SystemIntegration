using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public sealed class InbetriebnahmeViewBootstrap : MonoBehaviour
{
    private const string PdfFolderName = "InbetriebnahmePDFs";
    private const string ControllerAddress = "192.168.0.10";

    private readonly List<DocumentEntry> documents = new()
    {
        new DocumentEntry(
            "Allgemeine Anleitung",
            "allgemeine_anleitung.pdf",
            "Allgemeine Anleitung",
            "Diese Anleitung begleitet die Inbetriebnahme des Systems vom Start bis zur Verbindungspruefung.",
            "1. Anwendung starten und dieses Startfenster pruefen.\n\n2. Passendes PDF in der linken Liste auswaehlen.\n\n3. Windows-System, VR-Brillen-System und Tablet-System nach den jeweiligen Beschreibungen vorbereiten.\n\n4. Industriesteuerung einschalten und Netzwerkverbindung herstellen.\n\n5. Mit \"Verbindung testen\" pruefen, ob die Steuerung erreichbar ist.\n\n6. Bei negativer Pruefung wird automatisch die Fehlerbehebung geoeffnet."),
        new DocumentEntry(
            "Windows-System",
            "windows_system.pdf",
            "Beschreibung Windows-System",
            "Das Windows-System dient als Hauptarbeitsplatz fuer Bedienung, Diagnose und Verbindungstest.",
            "Aufgaben:\n- Anwendung starten und PDF-Dokumente anzeigen.\n- WLAN oder LAN-Verbindung zur Industriesteuerung herstellen.\n- Verbindungstest ausfuehren.\n- Fehlerstatus anzeigen und passende Anleitung oeffnen.\n\nVorbereitung:\n- Netzwerkkarte aktivieren.\n- Mit dem vorgesehenen Anlagen-WLAN verbinden.\n- Firewall-Regeln fuer die Anwendung pruefen, falls keine Antwort kommt."),
        new DocumentEntry(
            "VR-Brillen-System",
            "vr_brillen_system.pdf",
            "Beschreibung VR-Brillen-System",
            "Das VR-Brillen-System zeigt die Inbetriebnahmeinformationen in einer immersiven Bedienumgebung.",
            "Aufgaben:\n- VR-Brille starten und ausreichend laden.\n- Mit demselben Netzwerk wie die Industriesteuerung verbinden.\n- Anwendung oeffnen und Systemstatus kontrollieren.\n- Bedienhinweise im Sichtfeld beachten.\n\nHinweise:\n- Trackingbereich freihalten.\n- Controller koppeln.\n- Bei Verbindungsproblemen WLAN kurz deaktivieren und erneut verbinden."),
        new DocumentEntry(
            "Tablet-System",
            "tablet_system.pdf",
            "Beschreibung Tablet-System",
            "Das Tablet-System ist fuer mobile Bedienung, kurze Kontrollen und schnelle Diagnose vorgesehen.",
            "Aufgaben:\n- Tablet einschalten und WLAN verbinden.\n- Anwendung starten.\n- Passende Dokumentation oeffnen.\n- Verbindung zur Industriesteuerung pruefen.\n\nHinweise:\n- Energiesparmodus waehrend der Inbetriebnahme vermeiden.\n- Displayhelligkeit fuer Lesbarkeit einstellen.\n- Bei schwacher Verbindung naeher an den Access Point gehen."),
        new DocumentEntry(
            "Fehlerbehebung",
            "fehlerbehebung.pdf",
            "Anleitung zur Fehlerbehebung",
            "Diese Anleitung wird automatisch geoeffnet, wenn der Verbindungstest negativ verlaeuft.",
            "Moegliche Schritte:\n- Ping erneut versuchen.\n- WLAN aus- und wieder einschalten.\n- Pruefen, ob Windows, VR-Brille oder Tablet im richtigen Netzwerk sind.\n- Industriesteuerung neu starten, wenn sie nicht erreichbar ist.\n- IP-Adresse und Subnetzmaske kontrollieren.\n- Firewall oder VPN kurz pruefen.\n- Kabel, Access Point und Spannungsversorgung kontrollieren."),
        new DocumentEntry(
            "Industriesteuerung",
            "industriesteuerung.pdf",
            "Beschreibung Industriesteuerung",
            "Allgemeine Daten und Rolle der Industriesteuerung im Inbetriebnahmeprozess.",
            "Allgemeine Daten:\n- Funktion: Zentrale Steuerung der Anlage.\n- Netzwerk: Erreichbar ueber die konfigurierte Anlagen-IP.\n- Standard-Testadresse in dieser View: 192.168.0.10.\n- Verbindungstest: ICMP Ping.\n\nBetrieb:\n- Steuerung muss eingeschaltet und hochgefahren sein.\n- Netzwerkstatus am Geraet oder Schaltschrank pruefen.\n- Bei Stoerung zuerst Versorgung, Netzwerk und IP-Konfiguration kontrollieren.")
    };

    private Font font;
    private Text titleText;
    private Text subtitleText;
    private Text bodyText;
    private Text statusText;
    private Button openPdfButton;
    private readonly List<Button> documentButtons = new();
    private int selectedIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStartup()
    {
        if (FindAnyObjectByType<InbetriebnahmeViewBootstrap>() != null)
        {
            return;
        }

        var host = new GameObject("Inbetriebnahme View Bootstrap");
        DontDestroyOnLoad(host);
        host.AddComponent<InbetriebnahmeViewBootstrap>();
    }

    private void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        EnsureEventSystem();
        BuildView();
        SelectDocument(0);
    }

    private void EnsureEventSystem()
    {
        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            DontDestroyOnLoad(eventSystemObject);
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            standaloneInputModule.enabled = false;
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private void BuildView()
    {
        var canvasObject = new GameObject("Inbetriebnahme Startfenster");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366, 768);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var overlay = CreatePanel("Overlay", canvasObject.transform, new Color(0.04f, 0.05f, 0.06f, 0.96f));
        Stretch(overlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var window = CreatePanel("Fenster", overlay.transform, new Color(0.93f, 0.95f, 0.96f, 1f));
        Stretch(window.rectTransform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f), Vector2.zero, Vector2.zero);

        var header = CreatePanel("Kopfbereich", window.transform, new Color(0.12f, 0.16f, 0.20f, 1f));
        Stretch(header.rectTransform, new Vector2(0f, 0.86f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

        CreateText("Titel", header.transform, "Inbetriebnahme View", 30, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
            new Vector2(0.03f, 0.2f), new Vector2(0.54f, 0.85f));
        CreateText("Untertitel", header.transform, "PDF-Dokumentation fuer Windows, VR, Tablet und Industriesteuerung", 16, FontStyle.Normal,
            new Color(0.82f, 0.88f, 0.92f, 1f), TextAnchor.MiddleLeft, new Vector2(0.03f, 0.02f), new Vector2(0.7f, 0.42f));

        var testButton = CreateButton("Verbindung testen", header.transform, "Verbindung testen", new Color(0.10f, 0.46f, 0.40f, 1f),
            new Vector2(0.72f, 0.42f), new Vector2(0.89f, 0.82f));
        testButton.onClick.AddListener(TestConnection);

        var closeButton = CreateButton("Schliessen", header.transform, "Schliessen", new Color(0.36f, 0.40f, 0.44f, 1f),
            new Vector2(0.91f, 0.42f), new Vector2(0.98f, 0.82f));
        closeButton.onClick.AddListener(() => canvasObject.SetActive(false));

        statusText = CreateText("Status", header.transform, "Bereit fuer Verbindungstest", 15, FontStyle.Normal,
            new Color(0.86f, 0.90f, 0.94f, 1f), TextAnchor.MiddleRight, new Vector2(0.58f, 0.05f), new Vector2(0.98f, 0.34f));

        var navigation = CreatePanel("PDF Auswahl", window.transform, new Color(0.82f, 0.87f, 0.89f, 1f));
        Stretch(navigation.rectTransform, new Vector2(0f, 0f), new Vector2(0.28f, 0.86f), Vector2.zero, Vector2.zero);

        CreateText("Auswahl Label", navigation.transform, "PDF-Dokumente", 20, FontStyle.Bold, new Color(0.10f, 0.14f, 0.18f, 1f),
            TextAnchor.MiddleLeft, new Vector2(0.08f, 0.9f), new Vector2(0.92f, 0.98f));

        for (var i = 0; i < documents.Count; i++)
        {
            var index = i;
            var yMax = 0.86f - i * 0.105f;
            var yMin = yMax - 0.08f;
            var button = CreateButton(documents[i].Name, navigation.transform, documents[i].Name, new Color(0.18f, 0.24f, 0.29f, 1f),
                new Vector2(0.08f, yMin), new Vector2(0.92f, yMax));
            button.onClick.AddListener(() => SelectDocument(index));
            documentButtons.Add(button);
        }

        openPdfButton = CreateButton("PDF Oeffnen", navigation.transform, "PDF extern oeffnen", new Color(0.75f, 0.36f, 0.16f, 1f),
            new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.13f));
        openPdfButton.onClick.AddListener(OpenSelectedPdf);

        var content = CreatePanel("PDF Viewer", window.transform, Color.white);
        Stretch(content.rectTransform, new Vector2(0.28f, 0f), new Vector2(1f, 0.86f), Vector2.zero, Vector2.zero);

        titleText = CreateText("PDF Titel", content.transform, "", 26, FontStyle.Bold, new Color(0.09f, 0.12f, 0.16f, 1f),
            TextAnchor.MiddleLeft, new Vector2(0.06f, 0.87f), new Vector2(0.94f, 0.97f));
        subtitleText = CreateText("PDF Kurzbeschreibung", content.transform, "", 16, FontStyle.Normal, new Color(0.33f, 0.39f, 0.44f, 1f),
            TextAnchor.UpperLeft, new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.87f));

        var scrollRoot = CreatePanel("Scroll View", content.transform, new Color(0.96f, 0.97f, 0.98f, 1f));
        Stretch(scrollRoot.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.73f), Vector2.zero, Vector2.zero);
        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        var viewport = CreatePanel("Viewport", scrollRoot.transform, new Color(0.96f, 0.97f, 0.98f, 1f));
        Stretch(viewport.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var textObject = new GameObject("PDF Text");
        textObject.transform.SetParent(viewport.transform, false);
        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.offsetMin = new Vector2(24f, -980f);
        textRect.offsetMax = new Vector2(-24f, -18f);

        bodyText = textObject.AddComponent<Text>();
        bodyText.font = font;
        bodyText.fontSize = 19;
        bodyText.color = new Color(0.10f, 0.13f, 0.16f, 1f);
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.lineSpacing = 1.15f;

        scrollRect.viewport = viewport.rectTransform;
        scrollRect.content = textRect;
    }

    private void SelectDocument(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, documents.Count - 1);
        var document = documents[selectedIndex];

        titleText.text = document.Title;
        subtitleText.text = document.Subtitle + "\nDatei: " + document.FileName + " - " + GetPdfLoadState(document);
        bodyText.text = document.Body;

        for (var i = 0; i < documentButtons.Count; i++)
        {
            var colors = documentButtons[i].colors;
            colors.normalColor = i == selectedIndex ? new Color(0.10f, 0.46f, 0.40f, 1f) : new Color(0.18f, 0.24f, 0.29f, 1f);
            colors.highlightedColor = i == selectedIndex ? new Color(0.12f, 0.54f, 0.47f, 1f) : new Color(0.25f, 0.32f, 0.38f, 1f);
            documentButtons[i].colors = colors;
        }

        openPdfButton.interactable = File.Exists(GetPdfPath(document));
    }

    private async void TestConnection()
    {
        statusText.text = "Teste Verbindung zu " + ControllerAddress + " ...";
        statusText.color = new Color(0.86f, 0.90f, 0.94f, 1f);

        var reachable = false;
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync(ControllerAddress, 1500);
            reachable = reply.Status == IPStatus.Success;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Verbindungstest fehlgeschlagen: " + exception.Message);
        }

        if (reachable)
        {
            statusText.text = "Verbindung erfolgreich: Industriesteuerung erreichbar.";
            statusText.color = new Color(0.45f, 0.95f, 0.70f, 1f);
            return;
        }

        statusText.text = "Keine Verbindung. Fehlerbehebung wurde geoeffnet.";
        statusText.color = new Color(1f, 0.68f, 0.38f, 1f);
        SelectDocument(4);
    }

    private void OpenSelectedPdf()
    {
        var path = GetPdfPath(documents[selectedIndex]);
        if (!File.Exists(path))
        {
            statusText.text = "PDF nicht gefunden: " + documents[selectedIndex].FileName;
            statusText.color = new Color(1f, 0.55f, 0.45f, 1f);
            return;
        }

        Application.OpenURL(new Uri(path).AbsoluteUri);
    }

    private string GetPdfLoadState(DocumentEntry document)
    {
        return File.Exists(GetPdfPath(document)) ? "PDF geladen" : "PDF fehlt";
    }

    private static string GetPdfPath(DocumentEntry document)
    {
        return Path.Combine(Application.streamingAssetsPath, PdfFolderName, document.FileName);
    }

    private Image CreatePanel(string name, Transform parent, Color color)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        var image = panel.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int size, FontStyle style, Color color, TextAnchor anchor,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        var rect = textObject.AddComponent<RectTransform>();
        Stretch(rect, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        var text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        var image = CreatePanel(name, parent, color);
        Stretch(image.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        var button = image.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        CreateText("Label", image.transform, label, 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));

        return button;
    }

    private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private readonly struct DocumentEntry
    {
        public DocumentEntry(string name, string fileName, string title, string subtitle, string body)
        {
            Name = name;
            FileName = fileName;
            Title = title;
            Subtitle = subtitle;
            Body = body;
        }

        public string Name { get; }
        public string FileName { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Body { get; }
    }
}
