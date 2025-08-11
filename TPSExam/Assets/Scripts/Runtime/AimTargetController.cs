using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    public Camera mainCamera;
    public float maxDistance = 100f;
    public Color debugRayColor = Color.red;

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Visualize the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, debugRayColor);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * maxDistance;
        }
    }
}
