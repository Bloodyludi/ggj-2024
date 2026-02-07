using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class BeatDebugOverlay : MonoBehaviour
{
    [Header("Assign Assets/Shaders/BeatDebugGraph")]
    [SerializeField] private Shader graphShader;
    [SerializeField] private Key toggleKey = Key.Backquote;
    [SerializeField, Range(2, 12)] private float beatsToShow = 6;
    [SerializeField] private bool visibleOnStart;

    private BeatManager beatManager;
    private Material graphMaterial;
    private GameObject overlayRoot;
    private TextMeshProUGUI label;
    private bool isVisible;

    private const int MaxClicks = 16;
    private float[] clickTimes = new float[MaxClicks];
    private int clickIndex;
    private int clickCount;
    private bool clicksDirty;

    private int lastLabelBeat = -1;

    private static readonly Key[] TrackedKeys =
    {
        Key.W, Key.A, Key.S, Key.D,
        Key.UpArrow, Key.DownArrow, Key.LeftArrow, Key.RightArrow
    };

    private static readonly int PropCurrentTime = Shader.PropertyToID("_CurrentTime");
    private static readonly int PropBeatInterval = Shader.PropertyToID("_BeatInterval");
    private static readonly int PropWindowFrac = Shader.PropertyToID("_WindowFrac");
    private static readonly int PropBeatsToShow = Shader.PropertyToID("_BeatsToShow");
    private static readonly int PropClickTimes = Shader.PropertyToID("_ClickTimes");
    private static readonly int PropClickCount = Shader.PropertyToID("_ClickCount");

    private void Start()
    {
        beatManager = Services.Get<BeatManager>();

        if (graphShader == null)
        {
            Debug.LogError("BeatDebugOverlay: Assign the BeatDebugGraph shader.");
            enabled = false;
            return;
        }

        for (int i = 0; i < MaxClicks; i++)
            clickTimes[i] = -9999f;

        graphMaterial = new Material(graphShader);
        CreateUI();
        SetVisible(visibleOnStart);
    }

    private void CreateUI()
    {
        overlayRoot = new GameObject("BeatDebugOverlay");
        overlayRoot.transform.SetParent(transform, false);

        var canvas = overlayRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = overlayRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var graphGO = new GameObject("Graph");
        graphGO.transform.SetParent(overlayRoot.transform, false);

        var image = graphGO.AddComponent<RawImage>();
        image.material = graphMaterial;

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.offsetMin = new Vector2(20, 10);
        rect.offsetMax = new Vector2(-20, 130);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(overlayRoot.transform, false);

        label = labelGO.AddComponent<TextMeshProUGUI>();
        label.fontSize = 14;
        label.color = new Color(0.7f, 0.7f, 0.85f, 0.9f);
        label.alignment = TextAlignmentOptions.BottomLeft;

        var lr = label.rectTransform;
        lr.anchorMin = new Vector2(0, 0);
        lr.anchorMax = new Vector2(1, 0);
        lr.pivot = new Vector2(0, 0);
        lr.offsetMin = new Vector2(25, 132);
        lr.offsetMax = new Vector2(-20, 152);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        overlayRoot.SetActive(visible);

        if (!visible)
        {
            lastLabelBeat = -1;
        }
    }

    private void LateUpdate()
    {
        var kb = Keyboard.current;
        if (kb != null && kb[toggleKey].wasPressedThisFrame)
            SetVisible(!isVisible);

        if (!isVisible || beatManager == null) return;

        RecordClicks(kb);

        float currentTime = beatManager.GetCurrentTime();
        float beatInterval = beatManager.BeatInterval;
        float windowFrac = beatManager.MoveWindowTimePercent / 100f;

        graphMaterial.SetFloat(PropCurrentTime, currentTime);
        graphMaterial.SetFloat(PropBeatInterval, beatInterval);
        graphMaterial.SetFloat(PropWindowFrac, windowFrac);
        graphMaterial.SetFloat(PropBeatsToShow, beatsToShow);
        graphMaterial.SetInteger(PropClickCount, clickCount);

        if (clicksDirty)
        {
            graphMaterial.SetFloatArray(PropClickTimes, clickTimes);
            clicksDirty = false;
        }

        int beat = beatManager.BeatCounter;
        if (beat != lastLabelBeat)
        {
            lastLabelBeat = beat;
            float windowMs = (float)(beatManager.MoveWindowSeconds * 1000.0);
            label.text = $"BPM: {60f / beatInterval:F0}  |  Window: \u00b1{windowMs:F1}ms  |  Beat: {beat}";
        }
    }

    private void RecordClicks(Keyboard kb)
    {
        if (kb == null || !beatManager.ShouldPerformTicks) return;

        bool anyPressed = false;
        for (int i = 0; i < TrackedKeys.Length; i++)
        {
            if (kb[TrackedKeys[i]].wasPressedThisFrame)
            {
                anyPressed = true;
                break;
            }
        }

        if (!anyPressed) return;

        float time = beatManager.GetCurrentTime();
        float lapsed = time - beatManager.LastBeatTime;
        float until = beatManager.NextBeatTime - time;
        float window = (float)beatManager.MoveWindowSeconds;
        bool hit = (lapsed <= window) || (until <= window);

        clickTimes[clickIndex] = hit ? time : -time;
        clickIndex = (clickIndex + 1) % MaxClicks;
        if (clickCount < MaxClicks) clickCount++;
        clicksDirty = true;
    }

    private void OnDestroy()
    {
        if (graphMaterial != null)
            Destroy(graphMaterial);
    }
}
