using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _currentAmmoText;
    [SerializeField]
    private TextMeshProUGUI _ammoRemainsText;

    [SerializeField]
    private Image _gunPanel; // assign in inspector

    private Color _ogColor;
    private Coroutine _outOfAmmoRoutine;

    private void Awake()
    {
        _gunPanel = GetComponent<Image>();
        _ogColor = _gunPanel.color;
    }

    private void OnEnable()
    {
        GameManager.Instance.onAmmoChange += UpdateAmmoDisplay;
        GameManager.Instance.onOutOfAmmo += HandleOutOfAmmo;
    }

    private void OnDisable()
    {
        GameManager.Instance.onAmmoChange -= UpdateAmmoDisplay;
        GameManager.Instance.onOutOfAmmo -= HandleOutOfAmmo;
    }

    private void UpdateAmmoDisplay(int currentAmmo, int ammoRemains)
    {
        _currentAmmoText.text = currentAmmo.ToString();
        _ammoRemainsText.text = ammoRemains.ToString();
    }

    private void HandleOutOfAmmo(Shooter.EShootingMethod method)
    {
        if (_outOfAmmoRoutine != null)
            StopCoroutine(_outOfAmmoRoutine);

        _outOfAmmoRoutine = StartCoroutine(OutOfAmmoFlashRoutine());
    }

    private IEnumerator OutOfAmmoFlashRoutine()
    {
        Color targetColor = Color.red;
        float duration = 0.2f; // speed of each lerp

        for (int i = 0; i < 2; i++) // flash twice
        {
            // lerp to red
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                _gunPanel.color = Color.Lerp(_ogColor, targetColor, t);
                yield return null;
            }

            // lerp back to original
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                _gunPanel.color = Color.Lerp(targetColor, _ogColor, t);
                yield return null;
            }
        }

        // ensure back to original at the end
        _gunPanel.color = _ogColor;
        _outOfAmmoRoutine = null;
    }
}
