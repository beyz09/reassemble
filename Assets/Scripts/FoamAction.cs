using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPos = eventData.position;
        lastTime = Time.unscaledTime;
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        foamCoroutine = StartCoroutine(FoamTimer());
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
        if (foamCoroutine != null)
        {
            StopCoroutine(foamCoroutine);
            foamCoroutine = null;
        }
        OnFail?.Invoke(); // releasing early fails
    }

    IEnumerator FoamTimer()
    {
        float t = 0f;
        while (t < requiredTime)
        {
            t += Time.unscaledDeltaTime;
            // UI update hook could be added here via event
            yield return null;
        }
        foamCoroutine = null;
        OnSuccess?.Invoke();
    }

    void StopFoamFail()
    {
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        foamCoroutine = null;
        OnFail?.Invoke();
    }

    public void ResetState()
    {
        if (foamCoroutine != null) StopCoroutine(foamCoroutine);
        foamCoroutine = null;
    }
}
