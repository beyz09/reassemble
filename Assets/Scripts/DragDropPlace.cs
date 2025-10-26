using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Simple drag-and-drop placement component. OnPointerDown begins drag, on pointer up it checks whether the drop occurred
/// over a valid drop target (by GameObject name matching or by provided Transform). If dropped correctly, invokes success.
/// </summary>
public class DragDropPlace : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Tooltip("Optional explicit drop target. If null, will check pointer raycast target's name against validDropTargetNames.")]
    public Transform dropTarget;

    [Tooltip("If no explicit dropTarget, any GameObject whose name matches one of these will be accepted.")]
    public string[] validDropTargetNames = new string[] { "toob1", "toob", "tub", "bath", "bathtub" };

    [Tooltip("Event invoked when drop succeeds")] public UnityEvent OnSuccessEvent = new UnityEvent();
    [Tooltip("Event invoked when drop fails")] public UnityEvent OnFailEvent = new UnityEvent();

    public Action OnSuccess;
    public Action OnFail;

    bool dragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // optional: visual dragging handled by UI/animation
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        bool ok = false;
        // if explicit target set, check pointer overlap with target collider/bounds
        if (dropTarget != null)
        {
            // check world distance (works for world-space)
            Vector3 wp = Camera.main.ScreenToWorldPoint(eventData.position);
            wp.z = 0f;
            var targetPos = dropTarget.position;
            float dist = Vector2.Distance(new Vector2(wp.x, wp.y), new Vector2(targetPos.x, targetPos.y));
            // accept if within some radius (tweakable)
            if (dist < 1.5f) ok = true;
        }
        else
        {
            // try to use eventData.pointerCurrentRaycast
            var go = eventData.pointerCurrentRaycast.gameObject;
            if (go != null)
            {
                string n = go.name.ToLowerInvariant();
                foreach (var want in validDropTargetNames)
                {
                    if (n.Contains(want.ToLowerInvariant()))
                    {
                        ok = true;
                        break;
                    }
                }
            }
            else
            {
                // fallback: use OverlapPoint to detect Collider2D at pointer position
                if (Camera.main != null)
                {
                    Vector3 wp = Camera.main.ScreenToWorldPoint(eventData.position);
                    Vector2 p2 = new Vector2(wp.x, wp.y);
                    var coll = Physics2D.OverlapPoint(p2);
                    if (coll != null)
                    {
                        string n2 = coll.gameObject.name.ToLowerInvariant();
                        foreach (var want in validDropTargetNames)
                        {
                            if (n2.Contains(want.ToLowerInvariant())) { ok = true; break; }
                        }
                    }
                }
            }
        }

        if (ok)
        {
            OnSuccess?.Invoke();
            OnSuccessEvent?.Invoke();
        }
        else
        {
            OnFail?.Invoke();
            OnFailEvent?.Invoke();
        }
    }

    public void ResetState()
    {
        dragging = false;
    }
}
