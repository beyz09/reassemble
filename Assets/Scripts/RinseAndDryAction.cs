using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Manager for the rinse and dry two-step process with a single-slip tolerance.
/// This class assumes two separate drag-hold interactables (e.g. a rinse control and a dry control) will call StartRinse/StartDry and report success/fail via provided callbacks.
/// For simplicity this example exposes methods that should be called by the UI components when they succeed/fail.
/// </summary>
public class RinseAndDryAction : MonoBehaviour
{
    [Tooltip("How many non-fatal slips are allowed (1 for the level design).")]
    public int allowedSlips = 1;

    int slipsRemaining;
    bool rinsed = false;
    bool dried = false;

    public Action OnSuccess;
    public Action<string> OnFail; // reason

    [Tooltip("Inspector-friendly success event")] public UnityEvent OnSuccessEvent = new UnityEvent();
    [Tooltip("Inspector-friendly fail event")] public UnityEvent OnFailEvent = new UnityEvent();

    void Awake()
    {
        ResetState();
    }

    public void ResetState()
    {
        slipsRemaining = allowedSlips;
        rinsed = false;
        dried = false;
    }

    /// <summary>
    /// Call when rinse step succeeded.
    /// </summary>
    public void RinseSucceeded()
    {
        rinsed = true;
        CheckComplete();
    }

    /// <summary>
    /// Call when dry step succeeded.
    /// </summary>
    public void DrySucceeded()
    {
        dried = true;
        CheckComplete();
    }

    /// <summary>
    /// Call when the player makes a slip (drop or aggressive motion) during rinse/dry.
    /// If slipsRemaining reaches below 0, fail.
    /// </summary>
    public void RegisterSlip(string reason = "slip")
    {
        slipsRemaining--;
        if (slipsRemaining < 0)
        {
            OnFail?.Invoke(reason);
            OnFailEvent?.Invoke();
        }
        else
        {
            // soft feedback can be handled by level manager via hint
        }
    }

    // Convenience methods for inspector-wired events
    public void RegisterTowelSlip()
    {
        RegisterSlip("towel_fail");
    }

    public void RegisterWaterSlip()
    {
        RegisterSlip("water_fail");
    }

    void CheckComplete()
    {
        if (rinsed && dried)
        {
            OnSuccess?.Invoke();
            OnSuccessEvent?.Invoke();
        }
    }
}
