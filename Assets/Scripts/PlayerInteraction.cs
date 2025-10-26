using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float maxDistance = 3f;
    Camera cam;
    [Tooltip("If true, player must be within maxDistance to interact. If false, clicking interacts from any distance.")]
    public bool requireProximity = true;
    [Tooltip("If true and requireProximity is enabled, distance is measured from the main camera position instead of this GameObject (useful for click-to-interact without a player avatar).")]
    public bool useCameraAsOrigin = true;

    void Start() => cam = Camera.main;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            // 2D point check
            Collider2D col = Physics2D.OverlapPoint(worldPoint);
            if (col != null)
            {
                var interact = col.GetComponent<Interactable>();
                if (interact != null)
                {
                    Vector2 origin = useCameraAsOrigin && cam != null ? (Vector2)cam.transform.position : (Vector2)transform.position;
                    float d = Vector2.Distance(origin, col.transform.position);
                    if (!requireProximity || d <= maxDistance)
                    {
                        // report that the player interacted (used for intro hints)
                        InteractionTracker.ReportInteraction();
                        interact.Interact();
                    }
                    else
                    {
                        Debug.Log($"Too far to interact with {interact.displayName} (distance {d:F1} > {maxDistance})");
                    }
                }
            }
        }
    }
}
