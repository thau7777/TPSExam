using UnityEngine;
using UnityEngine.UI;

public class AimIconFollower : MonoBehaviour
{
    [SerializeField]
    private InputReader _inputReader;    // Reference to the input reader for aiming state

    [Header("References")]
    public Transform aimTarget;         // World-space target to follow
    public Camera mainCamera;           // Main camera
    public RectTransform aimIconUI;     // The UI Image's RectTransform

    [Header("Settings")]
    [SerializeField]
    private bool isAiming = false;               // Controlled from your aim logic

    private void OnEnable()
    {
        _inputReader.onAim += SetIsAiming;
    }

    private void OnDisable()
    {
        _inputReader.onAim -= SetIsAiming;
    }

    void Update()
    {
        // Show/hide the icon based on aiming state

        if (!isAiming || aimTarget == null || mainCamera == null)
            return;

        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(aimTarget.position);

        // Update UI icon position (Canvas must be Screen Space - Overlay or Screen Space - Camera)
        aimIconUI.position = screenPos;
    }

    private void SetIsAiming(bool value)
    {
        isAiming = value;

        aimIconUI.gameObject.SetActive(isAiming);
    }
}
