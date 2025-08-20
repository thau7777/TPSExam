using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentHealthText;
    [SerializeField] private TextMeshProUGUI _maxHealthText;
    [SerializeField] private RectTransform _healthBarContainer;
    [SerializeField] private Image _healthBar;

    private Coroutine _dangerRoutine;
    private Vector2 _originalPos;

    private void Start() // use Start instead of Awake
    {
        _originalPos = _healthBarContainer.anchoredPosition;
    }

    private void OnEnable()
    {
        GameManager.Instance.onCurrentHealthChange += UpdateHealthDisplay;
        GameManager.Instance.onCurrentHealthDanger += UpdateHealthDangerDisplay;
    }

    private void OnDisable()
    {
        GameManager.Instance.onCurrentHealthChange -= UpdateHealthDisplay;
        GameManager.Instance.onCurrentHealthDanger -= UpdateHealthDangerDisplay;
    }

    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (_currentHealthText != null)
            _currentHealthText.text = currentHealth.ToString();

        if (_maxHealthText != null)
            _maxHealthText.text = maxHealth.ToString();

        if (_healthBar != null)
            _healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void UpdateHealthDangerDisplay(bool isDanger)
    {
        if (isDanger)
        {
            if (_dangerRoutine == null)
                _dangerRoutine = StartCoroutine(DangerShake());
        }
        else
        {
            if (_dangerRoutine != null)
            {
                StopCoroutine(_dangerRoutine);
                _dangerRoutine = null;
            }
            StartCoroutine(RestoreToOriginal(0.2f)); // smooth restore
        }
    }

    private IEnumerator DangerShake()
    {
        float shakeAmount = 5f;     // px offset
        float shakeInterval = 0.03f;

        while (true)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );

            _healthBarContainer.anchoredPosition = _originalPos + randomOffset;

            yield return new WaitForSecondsRealtime(shakeInterval);
        }
    }

    private IEnumerator RestoreToOriginal(float duration)
    {
        Vector2 startPos = _healthBarContainer.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            _healthBarContainer.anchoredPosition = Vector2.Lerp(startPos, _originalPos, t);
            yield return null;
        }

        _healthBarContainer.anchoredPosition = _originalPos;
    }
}
