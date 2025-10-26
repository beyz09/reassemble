using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple controller to manage bathtub visual states and cat parenting.
/// Configure: tubEmpty (visible when no cat), tubWithCat (visible when cat placed), catObject (the cat GameObject in scene), catWetObject (optional final wet version prefab/object).
/// </summary>
public class BathTubController : MonoBehaviour
{
    [Tooltip("The empty tub GameObject (shown initially)")]
    public GameObject tubEmpty;
    [Tooltip("Alternative tub GameObject that shows the cat inside (optional)")]
    public GameObject tubWithCat;
    [Tooltip("Reference to the cat GameObject in the scene")]
    public GameObject catObject;
    [Tooltip("Optional wet cat GameObject to swap to on completion")]
    public GameObject catWetObject;

    [Tooltip("Event invoked when the cat is placed into the tub")]
    public UnityEvent OnCatPlaced = new UnityEvent();
    [Tooltip("Event invoked when the cat becomes wet / level completes")]
    public UnityEvent OnCatWet = new UnityEvent();

    public void ShowTubWithCat()
    {
        if (tubEmpty != null) tubEmpty.SetActive(false);
        if (tubWithCat != null) tubWithCat.SetActive(true);

        if (catObject != null)
        {
            // parent cat to tubWithCat (if available) or to this controller
            Transform parent = tubWithCat != null ? tubWithCat.transform : this.transform;
            // If tubWithCat contains its own cat visual, hide the original catObject; otherwise parent it under the tub
            if (tubWithCat != null && tubWithCat.transform.childCount > 0)
            {
                // hide the scene cat and rely on the tub-with-cat visual
                catObject.SetActive(false);
            }
            else
            {
                catObject.transform.SetParent(parent, worldPositionStays: false);
            }
        }

        OnCatPlaced?.Invoke();
    }

    public void MakeCatWet()
    {
        // swap cat visuals: if catWetObject provided, enable it and disable original
        if (catWetObject != null)
        {
            catWetObject.SetActive(true);
            if (catObject != null)
                catObject.SetActive(false);
        }

        OnCatWet?.Invoke();
    }

    /// <summary>
    /// Revert tub to empty and show the 'wet' cat (cat2) in scene.
    /// </summary>
    public void RevertToEmptyShowWetCat()
    {
        if (tubWithCat != null) tubWithCat.SetActive(false);
        if (tubEmpty != null) tubEmpty.SetActive(true);
        if (catWetObject != null)
        {
            catWetObject.SetActive(true);
        }
        if (catObject != null)
        {
            catObject.SetActive(false);
        }
    }

    /// <summary>
    /// After drying, hide wet cat and restore original cat (cat1) and ensure tub empty.
    /// </summary>
    public void RevertToOriginalCat()
    {
        if (catWetObject != null) catWetObject.SetActive(false);
        if (catObject != null) catObject.SetActive(true);
        if (tubWithCat != null) tubWithCat.SetActive(false);
        if (tubEmpty != null) tubEmpty.SetActive(true);
    }

    public void ResetBath()
    {
        if (tubEmpty != null) tubEmpty.SetActive(true);
        if (tubWithCat != null) tubWithCat.SetActive(false);
        if (catWetObject != null) catWetObject.SetActive(false);
        if (catObject != null) catObject.SetActive(true);
        // unparent cat
        if (catObject != null) catObject.transform.SetParent(null);
    }
}
