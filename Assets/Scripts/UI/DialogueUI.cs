using System;
using UnityEngine;
using UnityEngine.UI;
// Optional TextMeshPro support
#if TMP_PRESENT
using TMPro;
#endif

/// <summary>
/// Dialogue UI with improved responsiveness.
/// - Supports Unity UI `Text` and TextMeshPro (`TMP_Text`) if present.
/// - Enables Best Fit for dynamically created text elements.
/// - Can optionally auto-fix CanvasScaler to `Scale With Screen Size` at Awake.
///
/// Setup:
/// - Create a Canvas with a panel for the dialogue. Attach this script to a persistent UI object.
/// - Assign `panel`, `lineText` (or `tmpLineText`) and `choicesContainer`. Optionally provide `choiceButtonPrefab`.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject panel;

    [Tooltip("If using TextMeshPro, assign this. Otherwise assign `lineText` below.")]
#if TMP_PRESENT
    public TMP_Text tmpLineText;
#else
    // placeholder field kept for compiler when TMP isn't present
    // (leave null, we'll use legacy Text if assigned)
#endif
    public Text lineText;
    public RectTransform choicesContainer;
    public Button choiceButtonPrefab; // optional

    [Header("Canvas Scaler Auto-Fix (optional)")]
    [Tooltip("If true, the script will set any CanvasScaler to Scale With Screen Size and a 1920x1080 reference at Awake.")]
    public bool autoFixCanvasScaler = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);

        if (autoFixCanvasScaler)
        {
            var c = GetComponentInParent<Canvas>();
            if (c == null) c = FindObjectOfType<Canvas>();
            if (c != null)
            {
                var scaler = c.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }
    }

    /// <summary>
    /// Show dialogue lines and choices. onChoice should be invoked with the chosen index.
    /// </summary>
    public void ShowDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
        // Validate references
        if (panel == null || choicesContainer == null || (lineText == null
#if TMP_PRESENT
            && tmpLineText == null
#endif
            ))
        {
            Debug.LogWarning("DialogueUI: panel/lineText/choicesContainer not set. Cannot show dialogue.");
            onChoice?.Invoke(choices != null && choices.Length > 0 ? 0 : -1);
            return;
        }

        panel.SetActive(true);

        // Set the line text using TMP if available/assigned, otherwise legacy Text
#if TMP_PRESENT
        if (tmpLineText != null)
        {
            tmpLineText.text = string.Join("\n", lines ?? new string[0]);
            // Optionally enable auto-size for TMP
            tmpLineText.enableAutoSizing = true;
            tmpLineText.fontSizeMin = 18;
            tmpLineText.fontSizeMax = 40;
        }
        else
#endif
        if (lineText != null)
        {
            lineText.text = string.Join("\n", lines ?? new string[0]);
            // Enable best fit so text scales when the panel size changes
            lineText.resizeTextForBestFit = true;
            lineText.resizeTextMinSize = 14;
            lineText.resizeTextMaxSize = 36;
        }

        // Clear existing choices
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        if (choices == null || choices.Length == 0)
        {
            // No choices: provide an OK button
            CreateChoiceButton("OK", () => { Hide(); onChoice?.Invoke(-1); });
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            int idx = i;
            CreateChoiceButton(choices[i], () => { Hide(); onChoice?.Invoke(idx); });
        }
    }

    void CreateChoiceButton(string label, Action onClick)
    {
        Button btn = null;
        if (choiceButtonPrefab != null)
        {
            btn = Instantiate(choiceButtonPrefab, choicesContainer);
        }
        else
        {
            // Create a simple Button with Text using builtin Arial font
            GameObject go = new GameObject("ChoiceButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(choicesContainer, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            btn = go.GetComponent<Button>();

            // Add a LayoutElement so layout groups can size buttons consistently if present
            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 36f;

            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = label;
            txt.color = Color.black;

            // Enable best fit so label sizes with panel scale
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 12;
            txt.resizeTextMaxSize = 28;

            // Stretch text to fill button
            var rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // If prefab has a Text child, set its label. Also try TMP if present.
        if (choiceButtonPrefab != null)
        {
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.text = label;
#if TMP_PRESENT
            var tt = btn.GetComponentInChildren<TMP_Text>();
            if (tt != null) tt.text = label;
#endif
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
