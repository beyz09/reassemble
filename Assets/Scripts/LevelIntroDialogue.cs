using System.Collections;
using UnityEngine;

/// <summary>
/// Shows an intro "inner voice" dialog box a few seconds after level start if the player
/// hasn't interacted with anything yet.
/// Attach this to a persistent scene object (e.g., LevelManager or an empty GameObject).
/// </summary>
public class LevelIntroDialogue : MonoBehaviour
{
    [Tooltip("Delay (seconds) after level start before showing intro if no interaction has occurred")]
    public float delaySeconds = 3f;

    [TextArea]
    [Tooltip("Lines to show as the character's inner voice")]
    public string[] introLines = new string[] { "Hmm... where should I start?" };

    // If true, the intro will only be shown once per scene load
    public bool onlyOnce = true;

    bool shown = false;

    void OnEnable()
    {
        // reset tracker when level (re)starts
        InteractionTracker.Reset();
        if (!shown) StartCoroutine(CheckAndShowIntro());
    }

    IEnumerator CheckAndShowIntro()
    {
        float t = 0f;
        while (t < delaySeconds)
        {
            if (InteractionTracker.HasInteracted) yield break; // player already interacted
            t += Time.deltaTime;
            yield return null;
        }

        if (!InteractionTracker.HasInteracted && !shown)
        {
            // Use DialogueSystem to present the lines; fallback to ShowHint if no UI
            var ds = DialogueSystem.Instance;
            if (ds != null)
            {
                ds.StartDialogue(introLines, null, (choice) => { /* nothing */ });
            }
            shown = true;
            if (!onlyOnce) InteractionTracker.Reset();
        }
    }
}
