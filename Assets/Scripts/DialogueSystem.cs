using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Minimal dialogue system. For now it logs lines and auto-selects choices after a short delay.
/// Replace with UI-driven implementation later.
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Starts a dialogue. This minimal version prints lines and then auto-answers with the first choice after 1s.
    /// </summary>
    public void StartDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
        // If a DialogueUI exists in the scene, use it to present choices to the player.
        var ui = DialogueUI.Instance;
        if (ui != null)
        {
            ui.ShowDialogue(lines, choices, onChoice);
            return;
        }

        // Fallback to console-driven auto dialogue when no UI present
        StartCoroutine(RunDialogue(lines, choices, onChoice));
    }

    IEnumerator RunDialogue(string[] lines, string[] choices, Action<int> onChoice)
    {
        foreach (var l in lines)
        {
            Debug.Log("Dialogue: " + l);
            yield return new WaitForSeconds(0.35f);
        }

        if (choices != null && choices.Length > 0)
        {
            Debug.Log("Choices:");
            for (int i = 0; i < choices.Length; i++)
                Debug.Log(i + ": " + choices[i]);

            // In absence of UI, default to first choice after a delay so designers can see the flow.
            yield return new WaitForSeconds(0.5f);
            onChoice?.Invoke(0);
        }
        else
        {
            // No choices, treat as single-line acknowledgement
            yield return new WaitForSeconds(0.25f);
            onChoice?.Invoke(-1);
        }
    }
}
