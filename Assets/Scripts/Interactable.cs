using UnityEngine;

/// <summary>
/// Base class for anything the player can interact with.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Name shown in UI or logs")] public string displayName = "Interactable";

    /// <summary>
    /// Called when the player interacts (click / use).
    /// </summary>
    public abstract void Interact();
}
