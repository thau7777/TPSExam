using Unity.VisualScripting;
using UnityEngine;

public class ShootingPoint : MonoBehaviour
{
    public Transform aimTarget;
    public Color debugRayColor = Color.green;
    public float maxDistance = 100f;

    private void LateUpdate()
    {
        Ray ray = new Ray(transform.position, transform.up);
        Debug.DrawRay(transform.position, transform.up * 100f, debugRayColor);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            aimTarget.position = hit.point;
        }
        else
        {
            aimTarget.position = ray.origin + ray.direction * maxDistance;
        }
    }
}
