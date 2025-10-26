using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

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

    [Tooltip("Inspector-friendly event fired on success (hold complete).")]
    public UnityEvent OnSuccessEvent = new UnityEvent();
    [Tooltip("Inspector-friendly event fired on fail (released early or aggressive motion).")]
    public UnityEvent OnFailEvent = new UnityEvent();

    bool holding;
    float holdTimer;
    Vector2 lastPos;
    float lastTime;
    float speedAverage = 0f;

    [Tooltip("Smoothing factor for pointer speed (0..1). Higher = quicker to react to spikes.")]
    public float speedSmoothing = 0.2f;

    void Update()
    {
        if (!holding) return;
        holdTimer += Time.unscaledDeltaTime;
        // optional: clamp
        if (holdTimer >= requiredHoldTime)
        {
            holding = false;
            OnSuccess?.Invoke();
            OnSuccessEvent?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        holdTimer = 0f;
        lastPos = eventData.position;
        lastTime = Time.unscaledTime;
        speedAverage = 0f;
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
        // apply smoothing to avoid single-frame spikes causing failures
        speedAverage = Mathf.Lerp(speedAverage, speed, Mathf.Clamp01(speedSmoothing));
        if (speedAverage > aggressiveSpeedThreshold)
        {
            // aggressive motion -> fail immediately
            holding = false;
            OnFail?.Invoke();
            OnFailEvent?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!holding)
            return;
        holding = false;
        if (holdTimer >= requiredHoldTime)
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

    /// <summary>
    /// Resets internal state so the component can be reused.
    /// </summary>
    public void ResetState()
    {
        holding = false;
        holdTimer = 0f;
    }
}
