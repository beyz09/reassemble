using System;
using UnityEngine;

/// <summary>
/// A ghost that blocks items until the player selects the correct dialogue choice.
/// Place in scene and set id + dialogue options.
/// </summary>
public class Ghost : Interactable
{
    [Tooltip("Unique id used to mark which items this ghost blocks")] public string ghostId = "ghost_1";

    [TextArea] public string[] dialogueLines;
    public string[] dialogueChoices;
    [Tooltip("Index of the choice that dispels the ghost")] public int correctChoiceIndex = 0;

    public override void Interact()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            // No dialogue, auto-clear
            LevelManager.Instance.ClearGhost(ghostId);
            Dismiss();
            return;
        }

        // If a CustomDialogueController is present, use it so the designer can wire custom UI elements.
        if (CustomDialogueController.Instance != null)
        {
            CustomDialogueController.Instance.ShowDialogue(dialogueLines, dialogueChoices, (choice) =>
            {
                Debug.Log("Player chose option: " + choice);
                if (choice == correctChoiceIndex)
                {
                    LevelManager.Instance.ClearGhost(ghostId);
                    Dismiss();
                }
                else
                {
                    Debug.Log("Wrong choice. The ghost remains.");
                }
            });
            return;
        }

        // Fallback to the global DialogueSystem (may route to DialogueUI if present)
        DialogueSystem.Instance.StartDialogue(dialogueLines, dialogueChoices, (choice) =>
        {
            Debug.Log("Player chose option: " + choice);
            if (choice == correctChoiceIndex)
            {
                LevelManager.Instance.ClearGhost(ghostId);
                Dismiss();
            }
            else
            {
                // Wrong choice - you can add effects here (e.g., scare animation)
                Debug.Log("Wrong choice. The ghost remains.");
            }
        });
    }

    void Dismiss()
    {
        // Simple dismissal: play animation or destroy
        Debug.Log("Ghost " + ghostId + " dismissed.");
        Destroy(gameObject);
    }
}
