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
    public FoamAction foamAction; // köpükleme
    public RinseAndDryAction rinseDryAction; // rinse & dry manager

    [Header("Config")]
    public float restartDelay = 1f;

    enum Stage { Ready, Place, Foam, RinseDry, Completed }
    Stage stage = Stage.Ready;

    void Start()
    {
        // subscribe
        if (placeHold != null)
        {
            placeHold.OnSuccess += OnPlaceSuccess;
            placeHold.OnFail += () => OnFail("place_fail");
        }

        if (foamAction != null)
        {
            foamAction.OnSuccess += OnFoamSuccess;
            foamAction.OnFail += () => OnFail("foam_fail");
        }

        if (rinseDryAction != null)
        {
            rinseDryAction.OnSuccess += OnRinseDrySuccess;
            rinseDryAction.OnFail += (reason) => OnFail("rinse_dry_fail:" + reason);
        }

        StartLevel();
    }

    void StartLevel()
    {
        stage = Stage.Place;
        DialogueSystem.Instance?.StartDialogue(new string[] { "İşte Mırnav. Sakin olmalıyım.", "Leğene nazikçe yerleştir." }, null, null);
        // ensure components reset
        placeHold?.ResetState();
        foamAction?.ResetState();
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
        stage = Stage.RinseDry;
        DialogueSystem.Instance?.StartDialogue(new string[] { "Köpükleme bitti. Şimdi durula ve kurula." }, null, null);
    }

    void OnRinseDrySuccess()
    {
        if (stage != Stage.RinseDry) return;
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
        if (foamAction != null)
        {
            foamAction.OnSuccess -= OnFoamSuccess;
        }
        if (rinseDryAction != null)
        {
            rinseDryAction.OnSuccess -= OnRinseDrySuccess;
        }
    }
}
