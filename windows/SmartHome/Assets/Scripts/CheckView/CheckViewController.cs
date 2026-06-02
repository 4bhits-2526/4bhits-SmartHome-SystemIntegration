using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CheckView – erste Ansicht beim Start.
/// Prueft Verbindung zum OPC UA Server und zum Tablet.
/// Weiter-Button ist erst aktiv wenn alle Verbindungen gruen sind.
/// Info-Button (unten rechts) oeffnet das Panel jederzeit.
/// </summary>
public class CheckViewController : MonoBehaviour
{
    [Header("Netzwerk")]
    [SerializeField] private string opcuaIP      = "192.168.1.61";
    [SerializeField] private int    opcuaPort     = 4840;
    [SerializeField] private string tabletIP      = "192.168.1.193";
    [SerializeField] private int    tabletPort    = 80;
    [SerializeField] private float  checkInterval = 5f;

    [Header("PDF Tutorial")]
    [SerializeField] private string pdfFileName = "SmartHome_Tutorial.pdf";

    private bool opcuaConnected  = false;
    private bool tabletReachable = false;

    private GameObject overlayRoot;
    private Button     weiterBtn;

    private Image    dotOpc;
    private TMP_Text txtOpc;
    private Image    dotTablet;
    private TMP_Text txtTablet;

    private static readonly Color32 C_Green   = new Color32( 34, 197,  94, 255);
    private static readonly Color32 C_Red     = new Color32(239,  68,  68, 255);
    private static readonly Color32 C_Yellow  = new Color32(250, 204,  21, 255);
    private static readonly Color32 C_Panel   = new Color32( 18,  22,  36, 218);
    private static readonly Color32 C_Header  = new Color32( 28,  38,  62, 255);
    private static readonly Color32 C_Row     = new Color32( 30,  38,  58, 255);
    private static readonly Color32 C_RowAlt  = new Color32( 35,  44,  66, 255);
    private static readonly Color32 C_Blue    = new Color32( 37,  99, 235, 255);
    private static readonly Color32 C_BtnGrn  = new Color32( 22, 163,  74, 255);
    private static readonly Color32 C_BtnRed  = new Color32(220,  38,  38, 255);
    private static readonly Color32 C_BtnGray = new Color32( 71,  85, 105, 255);

    // ═════════════════════════════════════════════════════════════════════════
    #region Unity Lifecycle

    void Awake() => CreateUI();

    void Start()
    {
        SetDot(dotOpc,    txtOpc,    C_Red,    "Pruefe...");
        SetDot(dotTablet, txtTablet, C_Yellow, "Warte auf Steuerung...");
        StartCoroutine(CheckLoop());
        RefreshWeiterButton();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Verbindungscheck

    private IEnumerator CheckLoop()
    {
        while (true)
        {
            yield return StartCoroutine(CheckTcp(opcuaIP, opcuaPort, 2f,
                result =>
                {
                    opcuaConnected = result;
                    SetDot(dotOpc, txtOpc,
                           result ? C_Green : C_Red,
                           result ? "Verbunden" : "Getrennt");
                    if (!result)
                    {
                        tabletReachable = false;
                        SetDot(dotTablet, txtTablet, C_Yellow, "Warte auf Steuerung...");
                    }
                    RefreshWeiterButton();
                }));

            // Tablet erst pruefen wenn Steuerung (OPC UA) verbunden ist
            if (opcuaConnected)
            {
                yield return StartCoroutine(CheckTcp(tabletIP, tabletPort, 2f,
                    result =>
                    {
                        tabletReachable = result;
                        SetDot(dotTablet, txtTablet,
                               result ? C_Green : C_Red,
                               result ? "Online" : "Offline");
                        RefreshWeiterButton();
                    }));
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private IEnumerator CheckTcp(string ip, int port, float timeout, Action<bool> callback)
    {
        bool reached = false;
        bool done    = false;

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                using var tcp = new TcpClient();
                var ar = tcp.BeginConnect(ip, port, null, null);
                bool ok = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout));
                if (ok && tcp.Connected) { reached = true; tcp.EndConnect(ar); }
            }
            catch { }
            finally { done = true; }
        });
        thread.IsBackground = true;
        thread.Start();

        yield return new WaitUntil(() => done);
        callback(reached);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Helpers

    private void SetDot(Image dot, TMP_Text txt, Color32 color, string label)
    {
        if (dot) dot.color = color;
        if (txt) txt.text  = label;
    }

    private void RefreshWeiterButton()
    {
        if (weiterBtn == null) return;
        bool ok = opcuaConnected && tabletReachable;
        weiterBtn.interactable = ok;
        if (weiterBtn.TryGetComponent<Image>(out var img))
            img.color = ok ? C_BtnGrn : C_BtnGray;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Button Handler

    private void ShowPanel() => overlayRoot.SetActive(true);
    private void HidePanel() => overlayRoot.SetActive(false);

    private void OnWeiterClicked()
    {
        HidePanel();
        // Hier Navigation zur naechsten View einfuegen
    }

    private void OnStopClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnPdfClicked()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, pdfFileName);
        if (System.IO.File.Exists(path))
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        else
            Debug.LogWarning("[CheckView] PDF nicht gefunden: " + path);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region UI Builder

    private void CreateUI()
    {
        Canvas canvas = GetOrCreateCanvas();
        BuildInfoButton(canvas.transform);
        overlayRoot = BuildOverlay(canvas.transform);
        overlayRoot.SetActive(false);
    }

    private Canvas GetOrCreateCanvas()
    {
        // Immer einen eigenen Canvas erstellen – nie einen fremden wiederverwenden,
        // damit Scaler-Einstellungen und Sortierung vollständig kontrolliert sind.
        var go = new GameObject("CheckView_Canvas");
        go.transform.SetParent(transform);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode      = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder    = 200;
        canvas.pixelPerfect    = false; // TMP SDF braucht kein pixel-perfect

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight   = 1f;   // nur Höhe matchen → Schrift bleibt proportional
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void BuildInfoButton(Transform canvasT)
    {
        var go = NewGO("InfoBtn_CheckView", canvasT);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.sizeDelta        = new Vector2(60f, 60f);
        rt.anchoredPosition = new Vector2(-20f, 20f);

        var img = go.AddComponent<Image>();
        img.color = C_Blue;

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(ShowPanel);
        var cb = btn.colors;
        cb.normalColor      = C_Blue;
        cb.highlightedColor = new Color32(59, 130, 246, 255);
        cb.pressedColor     = new Color32(29,  78, 216, 255);
        btn.colors = cb;

        AddTMP(go.transform, "i", 32f, TextAlignmentOptions.Center, FontStyles.Bold);
    }

    private GameObject BuildOverlay(Transform canvasT)
    {
        var overlay = NewGO("CheckView_Overlay", canvasT);
        Stretch(overlay.GetComponent<RectTransform>());
        overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var panel = NewGO("CheckView_Panel", overlay.transform);
        var pRT   = panel.GetComponent<RectTransform>();
        pRT.anchorMin = pRT.anchorMax = pRT.pivot = new Vector2(0.5f, 0.5f);
        pRT.sizeDelta        = new Vector2(520f, 450f);
        pRT.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = C_Panel;

        BuildHeader(panel.transform);

        BuildStatusRow(panel.transform, "Steuerung (OPC UA)", opcuaIP + ":" + opcuaPort,
                       -80f, C_Row, out dotOpc, out txtOpc);
        BuildStatusRow(panel.transform, "Tablet (Bewertung)", tabletIP,
                       -155f, C_RowAlt, out dotTablet, out txtTablet);

        Image wDot; TMP_Text wTxt;
        BuildStatusRow(panel.transform, "Windows App", "Lokal",
                       -230f, C_Row, out wDot, out wTxt);
        SetDot(wDot, wTxt, C_Green, "Aktiv");

        var pdfBtn = BuildButton(panel.transform, "Tutorial oeffnen (PDF)",
                                 C_Blue, 0f, -305f, 260f, 44f);
        pdfBtn.onClick.AddListener(OnPdfClicked);

        var stopBtn  = BuildButton(panel.transform, "Stop",
                                   C_BtnRed,  -152f, -370f, 138f, 48f);
        weiterBtn    = BuildButton(panel.transform, "Weiter  ->",
                                   C_BtnGray,    0f, -370f, 138f, 48f);
        var closeBtn = BuildButton(panel.transform, "Schliessen",
                                   C_BtnGray,  152f, -370f, 138f, 48f);

        stopBtn .onClick.AddListener(OnStopClicked);
        weiterBtn.onClick.AddListener(OnWeiterClicked);
        closeBtn.onClick.AddListener(HidePanel);

        return overlay;
    }

    private void BuildHeader(Transform parent)
    {
        var go = NewGO("Header", parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.sizeDelta        = new Vector2(0f, 62f);
        rt.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = C_Header;

        AddTMP(go.transform, "Verbindungsstatus", 24f, TextAlignmentOptions.Center, FontStyles.Bold);

        var line = NewGO("Divider", go.transform);
        var lRT  = line.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0f, 0f);
        lRT.anchorMax = new Vector2(1f, 0f);
        lRT.pivot            = new Vector2(0.5f, 0f);
        lRT.sizeDelta        = new Vector2(0f, 2f);
        lRT.anchoredPosition = Vector2.zero;
        line.AddComponent<Image>().color = new Color32(37, 99, 235, 200);
    }

    private void BuildStatusRow(Transform parent, string title, string detail,
                                float yOffset, Color32 rowColor,
                                out Image dot, out TMP_Text statusText)
    {
        var row   = NewGO("Row_" + title, parent);
        var rowRT = row.GetComponent<RectTransform>();
        rowRT.anchorMin = rowRT.anchorMax = new Vector2(0.5f, 1f);
        rowRT.pivot            = new Vector2(0.5f, 1f);
        rowRT.sizeDelta        = new Vector2(460f, 60f);
        rowRT.anchoredPosition = new Vector2(0f, yOffset);
        row.AddComponent<Image>().color = rowColor;

        // Status-Punkt
        var dotGO = NewGO("Dot", row.transform);
        var dotRT = dotGO.GetComponent<RectTransform>();
        dotRT.anchorMin = dotRT.anchorMax = new Vector2(0f, 0.5f);
        dotRT.pivot            = new Vector2(0f, 0.5f);
        dotRT.sizeDelta        = new Vector2(20f, 20f);
        dotRT.anchoredPosition = new Vector2(18f, 0f);
        dot = dotGO.AddComponent<Image>();
        dot.color = C_Red;

        // Titel + Detail als ein TMP-Label mit Rich Text
        var lblGO = NewGO("Label", row.transform);
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0f, 0.5f);
        lblRT.anchorMax = new Vector2(1f, 0.5f);
        lblRT.pivot            = new Vector2(0f, 0.5f);
        lblRT.sizeDelta        = new Vector2(-130f, 50f);
        lblRT.anchoredPosition = new Vector2(50f, 0f);
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = "<b>" + title + "</b>\n<size=13><color=#7ba3d4>" + detail + "</color></size>";
        lbl.fontSize  = 18f;
        lbl.color     = Color.white;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;

        // Status-Text (rechts)
        var stGO = NewGO("Status", row.transform);
        var stRT = stGO.GetComponent<RectTransform>();
        stRT.anchorMin = new Vector2(1f, 0.5f);
        stRT.anchorMax = new Vector2(1f, 0.5f);
        stRT.pivot            = new Vector2(1f, 0.5f);
        stRT.sizeDelta        = new Vector2(120f, 40f);
        stRT.anchoredPosition = new Vector2(-14f, 0f);
        statusText = stGO.AddComponent<TextMeshProUGUI>();
        statusText.text      = "—";
        statusText.fontSize  = 16f;
        statusText.color     = Color.white;
        statusText.alignment = TextAlignmentOptions.MidlineRight;
    }

    private Button BuildButton(Transform parent, string label, Color32 bgColor,
                               float xOff, float yOff, float w, float h)
    {
        var go = NewGO("Btn_" + label, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.sizeDelta        = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(xOff, yOff);

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.15f);
        cb.pressedColor     = Color.Lerp(bgColor, Color.black, 0.2f);
        cb.disabledColor    = new Color32(71, 85, 105, 180);
        btn.colors = cb;

        AddTMP(go.transform, label, 16f, TextAlignmentOptions.Center, FontStyles.Bold);
        return btn;
    }

    private static GameObject NewGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static TextMeshProUGUI AddTMP(Transform parent, string text, float size,
                                          TextAlignmentOptions align,
                                          FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color     = Color.white;
        return tmp;
    }

    #endregion
}
