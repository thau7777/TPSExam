using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    public Camera mainCamera;
    public float maxDistance = 100f;
    public Color debugRayColor = Color.red;

    [Tooltip("Select all layers to ignore in the Inspector")]
    public LayerMask ignoreLayers;

    private Vector3 targetPosition;
    public float smoothTime = 0.05f;
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Visualize the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, debugRayColor);

        // Perform RaycastAll with inverted mask to ignore layers
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~ignoreLayers);

        if (hits.Length > 0)
        {
            // Sort hits by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Use the closest hit
            targetPosition = hits[0].point;
        }
        else
        {
            // No hit, aim at max range
            targetPosition = ray.origin + ray.direction * maxDistance;
        }

        // Smoothly move aim target to new position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}
