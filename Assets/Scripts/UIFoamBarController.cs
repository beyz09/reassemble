using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI controller to update a filled Image as a progress bar.
/// Exposes SetProgress(float) for UnityEvent wiring.
/// </summary>
public class UIFoamBarController : MonoBehaviour
{
    public Image fillImage;

    void Reset()
    {
        // try to auto-find
        if (fillImage == null)
        {
            var img = GetComponentInChildren<Image>();
            if (img != null) fillImage = img;
        }
    }

    public void SetProgress(float p)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(p);
    }
}
