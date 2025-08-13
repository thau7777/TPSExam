using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform aimTarget;
    public Color debugRayColor = Color.green;
    public float maxDistance = 100f;

    [Tooltip("Select all layers to ignore in the Inspector")]
    public LayerMask ignoreLayers;

    private Vector3 targetPosition;
    public float smoothTime = 0.1f;
    private Vector3 currentVelocity;

    [SerializeField]
    private Transform _shootingPoint;

    private void LateUpdate()
    {
        Ray ray = new Ray(transform.position, transform.up);
        Debug.DrawRay(transform.position, transform.up * maxDistance, debugRayColor);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance)); // sort closest first

        bool foundHit = false;

        foreach (var hit in hits)
        {
            // Skip ignored layers
            if ((ignoreLayers.value & (1 << hit.collider.gameObject.layer)) != 0)
                continue;

            // First valid hit → use it and break
            targetPosition = hit.point;
            foundHit = true;
            break;
        }

        if (!foundHit)
            targetPosition = ray.origin + ray.direction * maxDistance;

        aimTarget.position = Vector3.SmoothDamp(
            aimTarget.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    public void StartShoot()
    {
        var direction = Vector3.up;
        Ray ray = new Ray(_shootingPoint.position, direction.normalized);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            Debug.Log($"Hit: {hitInfo.collider.name}");
            // Here you can add logic to handle the hit, like damaging an enemy
        }
        else
        {
            Debug.Log("Missed!");
        }
    }
}
