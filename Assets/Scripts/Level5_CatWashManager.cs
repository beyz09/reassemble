using System;
using UnityEngine;

/// <summary>
/// Orchestrates Level 5 - Cat Wash flow. Connect the DragSensitiveHold (place), FoamAction (foam),
/// and two DragSensitiveHold instances for rinse/dry (or wire their success callbacks to the RinseAndDryAction).
/// This manager talks to DialogueSystem and resets on failure.
/// </summary>
public class Level5_CatWashManager : MonoBehaviour
{
    [Header("References")]
    public DragSensitiveHold placeHold; // leğene yerleştirme
    public DragDropPlace placeDrop; // alternative: drag-and-drop placement
    public FoamAction foamAction; // köpükleme
    public RinseAndDryAction rinseDryAction; // (kept for compatibility, not used in new linear flow)

    [Header("Interaction refs")]
    public DragSensitiveHold waterHold; // durulama
    public DragSensitiveHold towelHold; // kurulama on cat2
    public BathTubController bathController;

    [Header("Config")]
    public float restartDelay = 1f;

    enum Stage { Ready, Place, Foam, Rinse, Dry, Completed }
    Stage stage = Stage.Ready;

    void Start()
    {
        // subscribe
        if (placeHold != null)
        {
            placeHold.OnSuccess += OnPlaceSuccess;
            placeHold.OnFail += () => OnFail("place_fail");
        }
        if (placeDrop != null)
        {
            placeDrop.OnSuccess += OnPlaceSuccess;
            placeDrop.OnFail += () => OnFail("place_fail");
        }

        if (foamAction != null)
        {
            foamAction.OnSuccess += OnFoamSuccess;
            foamAction.OnFail += () => OnFail("foam_fail");
        }

        if (waterHold != null)
        {
            waterHold.OnSuccess += OnRinseSuccess;
            waterHold.OnFail += () => OnFail("rinse_fail");
        }

        if (towelHold != null)
        {
            towelHold.OnSuccess += OnFinalDrySuccess;
            towelHold.OnFail += () => OnFail("dry_fail");
        }

        StartLevel();
    }

    void StartLevel()
    {
        stage = Stage.Place;
        DialogueSystem.Instance?.StartDialogue(new string[] { "İşte Mırnav. Sakin olmalıyım.", "Leğene nazikçe yerleştir." }, null, null);
        // ensure components reset
        placeHold?.ResetState();
        placeDrop?.ResetState();
        foamAction?.ResetState();
        waterHold?.ResetState();
        towelHold?.ResetState();
        rinseDryAction?.ResetState();
    }

    void OnPlaceSuccess()
    {
        if (stage != Stage.Place) return;
        stage = Stage.Foam;
        DialogueSystem.Instance?.StartDialogue(new string[] { "Tamam Mırnav, sakin ol... Sadece birkaç dakika." }, null, null);
    }

    void OnFoamSuccess()
    {
        if (stage != Stage.Foam) return;
        stage = Stage.Rinse;
        DialogueSystem.Instance?.StartDialogue(new string[] { "Köpükleme bitti. Şimdi durula." }, null, null);
    }

    void OnRinseSuccess()
    {
        if (stage != Stage.Rinse) return;
        // After successful rinse, revert tub to empty and show wet cat (cat2)
        bathController?.RevertToEmptyShowWetCat();
        stage = Stage.Dry;
        DialogueSystem.Instance?.StartDialogue(new string[] { "Durulama bitti. Şimdi kurulama zamanı." }, null, null);
    }

    void OnFinalDrySuccess()
    {
        if (stage != Stage.Dry) return;
        // After drying wet cat, revert visuals to original cat and complete level
        bathController?.RevertToOriginalCat();
        stage = Stage.Completed;
        DialogueSystem.Instance?.StartDialogue(new string[] {
            "Bitti! Sakin kaldım. Ona saygı duydum.",
            "Gerçek güç, fırtına ortasında sakin kalabilmektir."
        }, null, null);
        // here: play success effects, reward, progress story
    }

    void OnRinseDrySuccess()
    {
        // This compatibility handler accepts either Rinse or Dry stages
        // (some older wiring used a combined rinse/dry action).
        if (stage != Stage.Rinse && stage != Stage.Dry) return;
        stage = Stage.Completed;
        DialogueSystem.Instance?.StartDialogue(new string[] {
            "Bitti! Sakin kaldım. Ona saygı duydum.",
            "Gerçek güç, fırtına ortasında sakin kalabilmektir."
        }, null, null);
        // here: play success effects, reward, progress story
    }

    void OnFail(string reason)
    {
        Debug.Log("Level5 fail: " + reason);
        DialogueSystem.Instance?.ShowHint("Mırnav kaçtı! Daha nazik ol.");
        // reset stage after a short delay
        CancelInvoke(nameof(Restart));
        Invoke(nameof(Restart), restartDelay);
    }

    void Restart()
    {
        StartLevel();
    }

    void OnDestroy()
    {
        // unsubscribe to avoid memory leaks
        if (placeHold != null)
        {
            placeHold.OnSuccess -= OnPlaceSuccess;
        }
        if (placeDrop != null)
        {
            placeDrop.OnSuccess -= OnPlaceSuccess;
        }
        if (foamAction != null)
        {
            foamAction.OnSuccess -= OnFoamSuccess;
        }
        if (rinseDryAction != null)
        {
            rinseDryAction.OnSuccess -= OnRinseDrySuccess;
        }
        if (waterHold != null)
        {
            waterHold.OnSuccess -= OnRinseSuccess;
        }
        if (towelHold != null)
        {
            towelHold.OnSuccess -= OnFinalDrySuccess;
        }
    }
}
