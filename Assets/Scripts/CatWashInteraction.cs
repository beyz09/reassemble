using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Implements the 'wash the cat' interaction designed to teach patience.
/// Attach to the cat GameObject (must have collider). This class implements
/// the 4 mechanics described in the task:
/// 1) Single slow click to capture (fast double-click = penalty)
/// 2) Three washing sub-stages that require intermittent (~1s) clicks. Fast spam triggers escape attempt.
/// 3) Escape countdown: player must not press any key or click for 3s or progress resets.
/// 4) On full completion, adds `brokenCupItem` to Inventory and starts a dialogue.
///
/// UI hooks are provided as public fields so the designer can wire sliders/texts in the Inspector.
/// </summary>
public class CatWashInteraction : Interactable
{
    public float captureWindow = 0.5f; // window to detect fast double-click for capture
    public float escapeCountdownSeconds = 3f;

    // Washing stages: soap, rinse, dry
    public int washingStages = 3;
    [Tooltip("How much progress a 'good' click gives (0-1)")]
    public float goodClickProgress = 0.10f; // ~10% per good click
    [Tooltip("How much progress a 'bad' (timing off) click gives")]
    public float badClickProgress = 0.02f; // small progress
    [Tooltip("Ideal time between clicks (seconds)")]
    public float idealClickInterval = 1f;
    [Tooltip("Allowed deviation from ideal interval to count as a 'good' click")]
    public float goodIntervalTolerance = 0.3f; // so good window = [0.7, 1.3]

    // Spam detection (4 or more clicks inside one second triggers penalty)
    public int spamThresholdPerSecond = 4;
    public float spamWindowSeconds = 1f;

    // UI hooks
    public Slider stageProgressSlider; // 0..1 slider showing current stage progress
    public Text messageText; // short messages like "Capture the cat" or "DON'T CLICK"
    public GameObject escapePanel; // optional panel to show during escape countdown
    public Text escapeCountdownText;

    // Completion hooks
    public ItemSO brokenCupItem; // assign in inspector
    [TextArea] public string[] completionDialogueLines = new string[] { "You calmed down.", "A broken cup is left behind." };
    public string[] completionDialogueChoices = new string[] { "OK" };

    // Internal state
    enum State { Idle, WaitingCaptureConfirm, Captured_Washing, EscapeCountdown, Completed }
    State state = State.Idle;

    // Capture click handling
    int captureClickCount = 0;
    Coroutine captureCoroutine = null;

    // Washing stage tracking
    int currentStage = 0;
    float stageProgress = 0f;
    float lastStageClickTime = -999f;
    List<float> recentClickTimes = new List<float>();

    // Escape countdown
    Coroutine escapeCoroutine = null;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // If we're in escape countdown, any key or mouse press outside Interact should also be penalized
        if (state == State.EscapeCountdown)
        {
            if (Input.anyKeyDown)
            {
                // Player reacted during countdown -> full reset
                FailEscape();
            }
        }
    }

    public override void Interact()
    {
        // Called by PlayerInteraction when the player clicks the cat collider.
        // Route input based on current state.
        if (state == State.Idle)
        {
            OnCaptureClick();
        }
        else if (state == State.WaitingCaptureConfirm)
        {
            // second click inside captureWindow -> considered aggressive for capture
            captureClickCount++;
            if (captureClickCount >= 2)
            {
                // Immediate penalty: cat flees, restart capture
                CancelCaptureWaitAndReset("You clicked too fast! The cat fled.");
                return;
            }
        }
        else if (state == State.Captured_Washing)
        {
            // Handle washing click
            HandleWashingClick();
        }
        else if (state == State.EscapeCountdown)
        {
            // Click during countdown => fail
            FailEscape();
        }
    }

    void OnCaptureClick()
    {
        // First click: start a short window to detect double-click spam.
        captureClickCount = 1;
        state = State.WaitingCaptureConfirm;
        messageTextSafe("Hold still... single click to grab the cat.");
        if (captureCoroutine != null) StopCoroutine(captureCoroutine);
        captureCoroutine = StartCoroutine(CaptureConfirmWindow());
    }

    IEnumerator CaptureConfirmWindow()
    {
        float t = 0f;
        while (t < captureWindow)
        {
            t += Time.deltaTime;
            yield return null;
            // If captureClickCount was increased to >=2 by Interact, the cancellation happens in Interact.
        }

        // No second click -> success capture
        captureCoroutine = null;
        if (captureClickCount == 1)
        {
            // Capture successful
            state = State.Captured_Washing;
            currentStage = 0;
            stageProgress = 0f;
            recentClickTimes.Clear();
            lastStageClickTime = -999f;
            messageTextSafe("Cat captured. Start soap stage. Click slowly (~1s intervals).");
            UpdateUI();
        }
        else
        {
            // Should not reach here since Interact handles >=2
            state = State.Idle;
            messageTextSafe("Capture failed.");
        }
    }

    void CancelCaptureWaitAndReset(string reason)
    {
        if (captureCoroutine != null) StopCoroutine(captureCoroutine);
        captureCoroutine = null;
        captureClickCount = 0;
        state = State.Idle;
        messageTextSafe(reason);
        // Reset any washing progress
        currentStage = 0;
        stageProgress = 0f;
        UpdateUI();
    }

    void HandleWashingClick()
    {
        float now = Time.time;

        // Spam detection: push this click time and purge older than spamWindowSeconds
        recentClickTimes.Add(now);
        recentClickTimes.RemoveAll(ts => now - ts > spamWindowSeconds);
        if (recentClickTimes.Count >= spamThresholdPerSecond)
        {
            // Trigger forced escape attempt
            StartEscapeCountdown("You clicked too fast! The cat panics — stay still!");
            return;
        }

        // Determine whether this click is well-timed
        bool goodClick = Mathf.Abs((lastStageClickTime < 0 ? idealClickInterval : now - lastStageClickTime) - idealClickInterval) <= goodIntervalTolerance;

        float delta = goodClick ? goodClickProgress : badClickProgress;
        stageProgress += delta;
        stageProgress = Mathf.Clamp01(stageProgress);
        lastStageClickTime = now;
        UpdateUI();

        if (stageProgress >= 1f)
        {
            // Stage complete
            currentStage++;
            if (currentStage >= washingStages)
            {
                CompleteWash();
            }
            else
            {
                // Proceed to next stage
                stageProgress = 0f;
                recentClickTimes.Clear();
                lastStageClickTime = -999f;
                messageTextSafe($"Stage {currentStage} complete. Continue calmly for next stage.");
                UpdateUI();
            }
        }
    }

    void StartEscapeCountdown(string message)
    {
        // Enter forced waiting state. Player must not click or press any key for escapeCountdownSeconds.
        if (escapeCoroutine != null) StopCoroutine(escapeCoroutine);
        state = State.EscapeCountdown;
        if (escapePanel != null) escapePanel.SetActive(true);
        messageTextSafe(message);
        escapeCoroutine = StartCoroutine(EscapeCountdown());
    }

    IEnumerator EscapeCountdown()
    {
        float t = escapeCountdownSeconds;
        while (t > 0f)
        {
            if (escapeCountdownText != null) escapeCountdownText.text = Mathf.CeilToInt(t).ToString();
            yield return null;
            t -= Time.deltaTime;
        }

        // Success: player did not react
        if (escapePanel != null) escapePanel.SetActive(false);
        escapeCoroutine = null;
        state = State.Captured_Washing; // return to washing where left off
        messageTextSafe("You stayed calm. Continue washing.");
        UpdateUI();
    }

    void FailEscape()
    {
        // Called when player reacts during escape countdown; full reset to capture stage.
        if (escapeCoroutine != null) StopCoroutine(escapeCoroutine);
        escapeCoroutine = null;
        if (escapePanel != null) escapePanel.SetActive(false);
        // Reset everything back to Idle - player must recapture the cat
        state = State.Idle;
        currentStage = 0;
        stageProgress = 0f;
        recentClickTimes.Clear();
        lastStageClickTime = -999f;
        messageTextSafe("You panicked — the cat fled. Start over and be calm.");
        UpdateUI();
    }

    void CompleteWash()
    {
        state = State.Completed;
        messageTextSafe("Washing complete. Aggressive ghost dissipates.");
        UpdateUI();

        // Add broken cup to inventory if assigned
        if (brokenCupItem != null && Inventory.Instance != null)
        {
            Inventory.Instance.Add(brokenCupItem);
        }

        // Start completion dialogue
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartDialogue(completionDialogueLines, completionDialogueChoices, (choice) => { /* nothing */ });
        }

        // Optional: destroy or disable this script/object to mark done
        // Destroy(gameObject, 2f);
    }

    void messageTextSafe(string s)
    {
        if (messageText != null) messageText.text = s;
        else Debug.Log(s);
    }

    void UpdateUI()
    {
        if (stageProgressSlider != null)
        {
            stageProgressSlider.value = stageProgress;
        }

        // Optionally show stage in message
        if (state == State.Idle)
        {
            messageTextSafe("Click the cat once to pick it up. Don't click rapidly.");
        }
        else if (state == State.Captured_Washing)
        {
            messageTextSafe($"Washing stage {currentStage + 1} of {washingStages} — click slowly (~{idealClickInterval:F1}s)");
        }
    }
}
