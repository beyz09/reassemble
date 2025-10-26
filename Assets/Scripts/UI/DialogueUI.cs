using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TMP_Text tmpLineText;
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
        if (panel == null || choicesContainer == null || (lineText == null && tmpLineText == null))
        {
            Debug.LogWarning("DialogueUI: panel/lineText/choicesContainer not set. Cannot show dialogue.");
            onChoice?.Invoke(choices != null && choices.Length > 0 ? 0 : -1);
            return;
        }

        // Start a coroutine to show lines one-by-one, then show choices.
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowDialogueCoroutine(lines ?? new string[0], choices, onChoice));
    }

    Coroutine activeRoutine = null;

    IEnumerator ShowDialogueCoroutine(string[] lines, string[] choices, Action<int> onChoice)
    {
        panel.SetActive(true);

        // Clear any existing choice buttons
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        // Show lines one at a time; advance with a Next button
        for (int li = 0; li < lines.Length; li++)
        {
            SetLineText(lines[li]);

            // Clear any existing choice buttons
            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
                Destroy(choicesContainer.GetChild(i).gameObject);

            bool advanced = false;
            // Create a Next button to advance (unless it's the last line and there are choices)
            bool createNext = !(li == lines.Length - 1 && choices != null && choices.Length > 0);
            if (createNext)
            {
                CreateChoiceButton("Next", () => { advanced = true; });
            }

            // Wait until advanced by button
            while (!advanced)
            {
                yield return null;
            }
        }

        // After all lines shown, present choices (or an OK)
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        if (choices == null || choices.Length == 0)
        {
            CreateChoiceButton("OK", () => { Hide(); onChoice?.Invoke(-1); });
        }
        else
        {
            for (int i = 0; i < choices.Length; i++)
            {
                int idx = i;
                CreateChoiceButton(choices[i], () => { Hide(); onChoice?.Invoke(idx); });
            }
        }

        activeRoutine = null;
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

        // If prefab has a Text or TMP child, set its label.
        if (choiceButtonPrefab != null)
        {
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.text = label;
            var tt = btn.GetComponentInChildren<TMP_Text>();
            if (tt != null) tt.text = label;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        // clear choice buttons when hiding
        if (choicesContainer != null)
        {
            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
                Destroy(choicesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Show a short hint message in the same line area for `duration` seconds.
    /// Useful for item-blocked messages like "You need to deal with the ghost first.".
    /// </summary>
    public void ShowHint(string message, float duration = 2f)
    {
        if (panel == null || choicesContainer == null || (tmpLineText == null && lineText == null))
        {
            Debug.Log(message);
            return;
        }

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowHintCoroutine(message, duration));
    }

    IEnumerator ShowHintCoroutine(string message, float duration)
    {
        panel.SetActive(true);
        SetLineText(message);

        // clear any choices
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Hide();
    }

    void SetLineText(string s)
    {
        if (tmpLineText != null)
        {
            tmpLineText.text = s;
            tmpLineText.enableAutoSizing = true;
            tmpLineText.fontSizeMin = 18;
            tmpLineText.fontSizeMax = 40;
            return;
        }

        if (lineText != null)
        {
            lineText.text = s;
            lineText.resizeTextForBestFit = true;
            lineText.resizeTextMinSize = 14;
            lineText.resizeTextMaxSize = 36;
        }
    }
}
