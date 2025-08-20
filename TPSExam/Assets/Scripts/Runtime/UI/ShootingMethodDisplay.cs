using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShootingMethodDisplay : MonoBehaviour
{
    [SerializeField] private Image autoImage;
    [SerializeField] private Image burstImage;
    [SerializeField] private Image grenadeImage;
    private Coroutine cooldownRoutine;
    private void OnEnable()
    {
        GameManager.Instance.onChangeShootingMode += UpdateShootingMethodDisplay;
        GameManager.Instance.onSpecialShoot += UpdateShotCooldown;
    }

    private void OnDisable()
    {
        GameManager.Instance.onChangeShootingMode -= UpdateShootingMethodDisplay;
        GameManager.Instance.onSpecialShoot -= UpdateShotCooldown;
    }
    private void UpdateShotCooldown(string animName, float cooldown)
    {
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        // Pick which image to animate
        Image targetImage = null;
        switch (animName)
        {
            case "BurstShot":
                targetImage = burstImage;
                break;
            case "SingleShot":
                targetImage = grenadeImage;
                break;
        }

        cooldownRoutine = StartCoroutine(CooldownFillRoutine(targetImage, cooldown));
    }

    private IEnumerator CooldownFillRoutine(Image img, float cooldown)
    {
        img.fillAmount = 0f; // start empty
        float time = 0f;

        while (time < cooldown)
        {
            time += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(time / cooldown);
            yield return null;
        }

        img.fillAmount = 1f; // snap full
        cooldownRoutine = null;
    }

    private void UpdateShootingMethodDisplay(int methodIndex)
    {
        StartCoroutine(FadeAndScale(autoImage, methodIndex == 0 ? 1f : 0.5f, methodIndex == 0, 0.1f));
        StartCoroutine(FadeAndScale(burstImage, methodIndex == 1 ? 1f : 0.5f, methodIndex == 1, 0.1f));
        StartCoroutine(FadeAndScale(grenadeImage, methodIndex == 2 ? 1f : 0.5f, methodIndex == 2, 0.1f));
    }

    private IEnumerator FadeAndScale(Image img, float targetAlpha, bool isActive, float duration)
    {
        // Fade
        Color startColor = img.color;
        float startAlpha = startColor.a;

        // Scale
        Vector3 startScale = img.rectTransform.sizeDelta;
        Vector3 targetScale = isActive ? new Vector2(120, 120) : new Vector2(100, 100);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Alpha
            startColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            img.color = startColor;

            // Size
            img.rectTransform.sizeDelta = Vector2.Lerp(startScale, targetScale, t);

            yield return null;
        }

        // Snap to final values
        startColor.a = targetAlpha;
        img.color = startColor;
        img.rectTransform.sizeDelta = targetScale;
    }
}
