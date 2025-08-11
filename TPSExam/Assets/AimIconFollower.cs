using UnityEngine;
using UnityEngine.UI;

public class AimIconFollower : MonoBehaviour
{
    [Header("References")]
    public Transform aimTarget;         // World-space target to follow
    public Camera mainCamera;           // Main camera
    public RectTransform aimIconUI;     // The UI Image's RectTransform

    [Header("Settings")]
    public bool isAiming;               // Controlled from your aim logic

    void Update()
    {
        // Show/hide the icon based on aiming state
        aimIconUI.gameObject.SetActive(isAiming);

        if (!isAiming || aimTarget == null || mainCamera == null)
            return;

        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(aimTarget.position);

        // Update UI icon position (Canvas must be Screen Space - Overlay or Screen Space - Camera)
        aimIconUI.position = screenPos;
    }
}
