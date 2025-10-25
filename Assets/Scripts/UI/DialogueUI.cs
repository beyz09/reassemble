using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal dialogue UI. Place a Canvas in the scene and attach this component.
/// Assign `panel`, `lineText` and `choicesContainer` in the inspector. Optionally assign a `choiceButtonPrefab`.
/// If no prefab is provided, buttons will be created dynamically using the built-in Arial font.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject panel;
    public Text lineText;
    public RectTransform choicesContainer;
    public Button choiceButtonPrefab; // optional

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// Show dialogue lines and choices. onChoice should be invoked with the chosen index.
    /// </summary>
    public void ShowDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
        if (panel == null || lineText == null || choicesContainer == null)
        {
            Debug.LogWarning("DialogueUI: panel/lineText/choicesContainer not set. Cannot show dialogue.");
            // fallback: immediately pick first choice
            onChoice?.Invoke(choices != null && choices.Length > 0 ? 0 : -1);
            return;
        }

        panel.SetActive(true);
        lineText.text = string.Join("\n", lines ?? new string[0]);

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

            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = label;
            txt.color = Color.black;

            // Stretch text to fill button
            var rt = txtGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // Set the label if we used a prefab that has a Text child
        if (choiceButtonPrefab != null)
        {
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.text = label;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
