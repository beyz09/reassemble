using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lightweight dialogue presenter that you can wire to your own UI elements.
/// - Assign `panel`, `lineText` and `choicesContainer` in the Inspector.
/// - Optional `choiceButtonPrefab` for nicely styled buttons (if null a simple button will be created).
/// - Shows lines one-by-one with a "Next" button, then renders choices and invokes the callback.
///
/// This is intentionally minimal so you can replace internals to use TextMeshPro or custom anims.
/// </summary>
public class CustomDialogueController : MonoBehaviour
{
    public static CustomDialogueController Instance { get; private set; }

    [Header("UI refs")]
    public GameObject panel;
    public TMP_Text tmpLineText;
    public Text lineText;
    public RectTransform choicesContainer;
    public Button choiceButtonPrefab;
    [Tooltip("Optional array of prefabs to use per-choice index. If provided, choiceButtonPrefabs[0] will be used for the first choice, etc.")]
    public Button[] choiceButtonPrefabs;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    // A small runner to ensure coroutines are started on an active GameObject
    class CoroutineRunner : MonoBehaviour { }
    static CoroutineRunner _runner;
    static CoroutineRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                var go = new GameObject("DialogueCoroutineRunner");
                DontDestroyOnLoad(go);
                _runner = go.AddComponent<CoroutineRunner>();
            }
            return _runner;
        }
    }

    /// <summary>
    /// Show dialogue lines and choices. Callback receives chosen index or -1 for OK/no-choice.
    /// </summary>
    public void ShowDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
        if (panel == null || choicesContainer == null || (tmpLineText == null && lineText == null))
        {
            Debug.LogWarning("CustomDialogueController: UI refs missing. Falling back to callback immediately.");
            onChoice?.Invoke(choices != null && choices.Length > 0 ? 0 : -1);
            return;
        }

        // Use a persistent runner so coroutines can start even if this component is inactive in hierarchy
        Runner.StartCoroutine(RunDialogue(lines ?? new string[0], choices, onChoice));
    }

    IEnumerator RunDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
    panel.SetActive(true);

        // clear previous choices
        for (int i = choicesContainer.childCount - 1; i >= 0; i--) Destroy(choicesContainer.GetChild(i).gameObject);

        // show each line and wait for Next
        for (int i = 0; i < lines.Length; i++)
        {
            if (tmpLineText != null) tmpLineText.text = lines[i]; else lineText.text = lines[i];

            // create Next button (unless last line and we will show choices)
            bool willShowChoices = (i == lines.Length - 1) && (choices != null && choices.Length > 0);
            if (!willShowChoices)
            {
                bool advanced = false;
                CreateChoiceButton("Next", () => { advanced = true; });
                while (!advanced) yield return null;
                // clear Next button
                for (int j = choicesContainer.childCount - 1; j >= 0; j--) Destroy(choicesContainer.GetChild(j).gameObject);
            }
            else
            {
                // last line and will show choices -> break to choices rendering
            }
        }

        // render choices or a single OK
        for (int j = choicesContainer.childCount - 1; j >= 0; j--) Destroy(choicesContainer.GetChild(j).gameObject);

        if (choices == null || choices.Length == 0)
        {
            CreateChoiceButton("OK", () => { Hide(); onChoice?.Invoke(-1); });
        }
        else
        {
            for (int k = 0; k < choices.Length; k++)
            {
                int idx = k;
                CreateChoiceButton(choices[k], () => { Hide(); onChoice?.Invoke(idx); }, idx);
            }

            // Force a layout rebuild so multiple buttons are arranged properly
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        }

        yield break;
    }

    void CreateChoiceButton(string label, Action onClick, int prefabIndex = -1)
    {
        Button btn = null;
        // Choose prefab: prefer per-index prefab if available, else single prefab
        Button prefabToUse = null;
        if (prefabIndex >= 0 && choiceButtonPrefabs != null && prefabIndex < choiceButtonPrefabs.Length)
            prefabToUse = choiceButtonPrefabs[prefabIndex];
        if (prefabToUse == null) prefabToUse = choiceButtonPrefab;

    GameObject instantiatedRoot = null;
    if (prefabToUse != null)
        {
            // Robustly find a Button component on the prefab (root or children)
            var hasButton = prefabToUse.GetComponent<Button>() != null || prefabToUse.GetComponentInChildren<Button>() != null;
            if (!hasButton)
            {
                Debug.LogWarning($"CustomDialogueController: assigned prefab '{prefabToUse.name}' does not contain a Button component. Falling back to default button.");
            }
            else
            {
                var instance = Instantiate(prefabToUse, choicesContainer);
                // Try to get Button from root first, then children
                var foundBtn = instance.GetComponent<Button>() ?? instance.GetComponentInChildren<Button>();
                if (foundBtn == null)
                {
                    Debug.LogWarning($"CustomDialogueController: prefab '{prefabToUse.name}' instantiation produced no Button. Falling back to default.");
                }
                else
                {
                    btn = foundBtn;
                    // Ensure instantiated prefab is active
                    btn.gameObject.SetActive(true);
                    instantiatedRoot = instance.gameObject;
                    // Set label on Text or TMP children if present
                    var t = btn.GetComponentInChildren<Text>();
                    if (t != null) t.text = label;
                    var tt = btn.GetComponentInChildren<TMP_Text>();
                    if (tt != null) tt.text = label;

                    // Ensure a LayoutElement so layout groups size them
                    var le = btn.gameObject.GetComponent<LayoutElement>();
                    if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
                    le.minHeight = 36f;
                    le.flexibleWidth = 1f;

                    Debug.Log($"CustomDialogueController: using prefab '{prefabToUse.name}' for choice '{label}' (index {prefabIndex}).");
                }
            }
        }
        else
        {
            GameObject go = new GameObject("ChoiceButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(choicesContainer, false);
            var img = go.GetComponent<Image>(); img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            btn = go.GetComponent<Button>();

            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;
            txt.color = Color.black;

            var rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Add a LayoutElement so layout groups can size buttons consistently
            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 36f;
            layout.flexibleWidth = 1f;

            instantiatedRoot = go;
        }

        // Ensure the button rect stretches to fill available space when a LayoutGroup exists
        if (btn != null)
        {
            var btnRt = btn.GetComponent<RectTransform>();
            if (btnRt != null)
            {
                btnRt.anchorMin = new Vector2(0f, 0f);
                btnRt.anchorMax = new Vector2(1f, 1f);
                btnRt.sizeDelta = new Vector2(0f, 0f);
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick?.Invoke());
        }

        // Normalize the instantiated root RectTransform so prefab's own offsets won't break layout
        if (instantiatedRoot != null)
        {
            var rootRt = instantiatedRoot.GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.localScale = Vector3.one;
                rootRt.anchorMin = new Vector2(0f, 0f);
                rootRt.anchorMax = new Vector2(1f, 1f);
                rootRt.anchoredPosition = Vector2.zero;
                rootRt.sizeDelta = Vector2.zero;
            }
        }

        // If the container has no LayoutGroup, add a HorizontalLayoutGroup so multiple buttons arrange nicely
        if (choicesContainer != null)
        {
            var lg = choicesContainer.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (lg == null)
            {
                var h = choicesContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                h.childForceExpandWidth = true;
                h.childForceExpandHeight = false;
                h.spacing = 8f;
                h.childAlignment = TextAnchor.MiddleCenter;
            }

            // Force rebuild so the new button sizes and positions update immediately
            UnityEngine.Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        // clear buttons
        if (choicesContainer != null)
        {
            for (int i = choicesContainer.childCount - 1; i >= 0; i--) Destroy(choicesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Show a short hint message in the same line area for `duration` seconds.
    /// </summary>
    public void ShowHint(string message, float duration = 2f)
    {
        if (panel == null || choicesContainer == null || (tmpLineText == null && lineText == null))
        {
            Debug.Log(message);
            return;
        }

        Runner.StartCoroutine(ShowHintCoroutine(message, duration));
    }

    IEnumerator ShowHintCoroutine(string message, float duration)
    {
        panel.SetActive(true);
        if (tmpLineText != null) tmpLineText.text = message; else lineText.text = message;

        // clear any choices
        for (int i = choicesContainer.childCount - 1; i >= 0; i--) Destroy(choicesContainer.GetChild(i).gameObject);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Hide();
    }
}
