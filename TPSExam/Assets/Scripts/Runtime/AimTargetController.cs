using TMPro;
using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    public Camera mainCamera;
    public float maxDistance = 100f;
    public Color debugRayColor = Color.red;
    Vector3 targetPosition;
    public float smoothTime = 0.05f; // smaller = faster snap, larger = smoother
    private Vector3 currentVelocity; // for SmoothDamp
    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Visualize the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, debugRayColor);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = ray.origin + ray.direction * maxDistance;
        }

        // Smoothly move aimTarget to new position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}
