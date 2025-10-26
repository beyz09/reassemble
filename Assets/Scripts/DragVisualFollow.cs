using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Makes a GameObject follow the pointer while dragging. Works for world-space sprites and UI (tries to use the event camera).
/// Use together with DragDropPlace: this component only handles visuals; drop logic remains in DragDropPlace.
/// </summary>
public class DragVisualFollow : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    Vector3 offset;
    bool dragging = false;
    Camera eventCam;

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        eventCam = eventData.pressEventCamera ?? Camera.main;
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        offset = transform.position - worldPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        transform.position = worldPoint + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (eventCam == null) eventCam = Camera.main;
        if (eventCam == null)
        {
            return Vector3.zero;
        }
        Vector3 p = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(eventCam.transform.position.z - transform.position.z));
        return eventCam.ScreenToWorldPoint(p);
    }
}
