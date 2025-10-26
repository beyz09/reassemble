using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// FoamAction: requires a continuous drag/hold action for a duration (e.g. 10s). Any aggressive motion or release before time causes fail.
/// Subscribe to OnSuccess and OnFail.
/// </summary>
public class FoamAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float requiredTime = 10f;
    public float aggressiveSpeedThreshold = 1000f;

    public Action OnSuccess;
    public Action OnFail;

    Coroutine foamCoroutine;
    Vector2 lastPos;
    float lastTime;
    bool isRunning = false;
    bool completed = false;

    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Tooltip("Progress event (0..1) invoked while foaming)")]
    public FloatEvent OnProgress = new FloatEvent();

    [Tooltip("Invoked when foam action starts")] public UnityEvent OnStarted = new UnityEvent();
    [Tooltip("Invoked when foam action stops (either success or fail)")] public UnityEvent OnStopped = new UnityEvent();
    [Tooltip("Invoked when foam completes successfully")] public UnityEvent OnSuccessEvent = new UnityEvent();
    [Tooltip("Invoked when foam fails")] public UnityEvent OnFailEvent = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPos = eventData.position;
        lastTime = Time.unscaledTime;
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        completed = false;
        isRunning = true;
        foamCoroutine = StartCoroutine(FoamTimer());
        OnStarted?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        float dt = now - lastTime;
        if (dt > 0f)
        {
            float speed = Vector2.Distance(eventData.position, lastPos) / dt;
            lastPos = eventData.position;
            lastTime = now;
            if (speed > aggressiveSpeedThreshold)
            {
                StopFoamFail();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If foam is still running (not yet completed), releasing is a fail.
        if (isRunning && !completed)
        {
            if (foamCoroutine != null)
            {
                StopCoroutine(foamCoroutine);
                foamCoroutine = null;
            }
            isRunning = false;
            OnFail?.Invoke(); // releasing early fails
            OnFailEvent?.Invoke();
            OnStopped?.Invoke();
        }
        else
        {
            // either already completed (success invoked) or not running; do nothing
        }
    }

    IEnumerator FoamTimer()
    {
        float t = 0f;
        while (t < requiredTime)
        {
            t += Time.unscaledDeltaTime;
            // update progress (0..1)
            float progress = Mathf.Clamp01(t / requiredTime);
            OnProgress?.Invoke(progress);
            yield return null;
        }
        foamCoroutine = null;
        completed = true;
        isRunning = false;
        OnSuccess?.Invoke();
        OnSuccessEvent?.Invoke();
        OnStopped?.Invoke();
    }

    void StopFoamFail()
    {
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        foamCoroutine = null;
        OnFail?.Invoke();
        OnFailEvent?.Invoke();
        OnStopped?.Invoke();
    }

    public void ResetState()
    {
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        foamCoroutine = null;
        isRunning = false;
        completed = false;
    }
}
