using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any GameObject with a Collider / Collider2D to allow clicks
/// to directly trigger its Interactable behavior without a central PlayerInteraction.
/// Works with the EventSystem (PhysicsRaycaster / Physics2DRaycaster on the Camera)
/// and also with the legacy OnMouseDown path as a fallback.
/// </summary>
public class Clickable : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("If true, ignore clicks when pointer is over UI elements.")]
    public bool ignoreClicksOverUI = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(eventData.pointerId))
        {
            Debug.Log("Clickable: pointer over UI, ignoring click on " + gameObject.name);
            return;
        }

        TryInteract();
    }

    // Legacy fallback for simple projects: Unity calls this if the object has a Collider/Collider2D
    void OnMouseDown()
    {
        // OnMouseDown won't tell us pointer id; check general UI overlap if requested
        if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Clickable: OnMouseDown ignored because pointer is over UI on " + gameObject.name);
            return;
        }

        TryInteract();
    }

    void TryInteract()
    {
        var interact = GetComponent<Interactable>();
        if (interact != null)
        {
            Debug.Log("Clickable: Interacting with " + interact.displayName + " (" + gameObject.name + ")");
            interact.Interact();
        }
        else
        {
            Debug.Log("Clickable: No Interactable component found on " + gameObject.name);
        }
    }
}
