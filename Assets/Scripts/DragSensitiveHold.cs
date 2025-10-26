using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Generic drag-and-hold behaviour that fails if pointer moves too fast (aggressive motion) or if released early.
/// Attach to a draggable UI/World element and subscribe to OnSuccess/OnFail.
/// </summary>
public class DragSensitiveHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Tooltip("Pixels per second above which motion is considered aggressive and causes a fail.")]
    public float aggressiveSpeedThreshold = 1000f;

    [Tooltip("Required continuous hold time in seconds to succeed.")]
    public float requiredHoldTime = 3f;

    public Action OnSuccess;
    public Action OnFail;

    bool holding;
    float holdTimer;
    Vector2 lastPos;
    float lastTime;

    void Update()
    {
        if (!holding) return;
        holdTimer += Time.unscaledDeltaTime;
        // optional: clamp
        if (holdTimer >= requiredHoldTime)
        {
            holding = false;
            OnSuccess?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        holdTimer = 0f;
        lastPos = eventData.position;
        lastTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!holding) return;
        float now = Time.unscaledTime;
        float dt = now - lastTime;
        if (dt <= 0f) return;
        float speed = Vector2.Distance(eventData.position, lastPos) / dt;
        lastPos = eventData.position;
        lastTime = now;
        if (speed > aggressiveSpeedThreshold)
        {
            // aggressive motion -> fail immediately
            holding = false;
            OnFail?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!holding)
            return;
        holding = false;
        if (holdTimer >= requiredHoldTime)
            OnSuccess?.Invoke();
        else
            OnFail?.Invoke();
    }

    /// <summary>
    /// Resets internal state so the component can be reused.
    /// </summary>
    public void ResetState()
    {
        holding = false;
        holdTimer = 0f;
    }
}
