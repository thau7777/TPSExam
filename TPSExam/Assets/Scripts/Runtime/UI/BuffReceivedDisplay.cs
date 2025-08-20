using System.Collections;
using TMPro;
using UnityEngine;

public class BuffReceivedDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform _buffReceivedPanel;
    [SerializeField] private TextMeshProUGUI _buffReceivedText;

    private Vector3 _ogScale;
    private Coroutine _displayRoutine;

    private void Awake()
    {
        _ogScale = _buffReceivedPanel.localScale;
        _buffReceivedPanel.gameObject.SetActive(false);
    }

    public void ShowBuffReceived(Buff.BuffType buffType)
    {
        // Stop any existing animation and reset
        if (_displayRoutine != null)
        {
            StopCoroutine(_displayRoutine);
            _buffReceivedPanel.localScale = _ogScale;
            _buffReceivedPanel.gameObject.SetActive(false);
        }

        // Decide text for the buff
        string displayText = buffType switch
        {
            Buff.BuffType.MovementSpeed => "Movement Speed Increased!",
            Buff.BuffType.InfinityBullet => "Infinity Bullets for 10 seconds!",
            Buff.BuffType.BulletDamage => "Bullet Damage Increased!",
            Buff.BuffType.GrenadeDamage => "Grenade Damage Increased!",
            Buff.BuffType.MaxHealth => "Max Health Increased!",
            Buff.BuffType.Ammo => "Ammo Received!",
            Buff.BuffType.InfinityGrenade => "Infinity Grenades for 10 seconds!",
            Buff.BuffType.HealthRegen => "Health Regeneration Increased!",
            Buff.BuffType.ReloadSpeed => "Reload Speed Increased!",
            Buff.BuffType.InstantHeal => "Restore 50 Health!",
            Buff.BuffType.KillAllEnemies => "All Enemies Killed!",
            _ => "Buff Received!"
        };

        _buffReceivedText.text = displayText;

        // Start new routine
        _displayRoutine = StartCoroutine(AnimateBuffReceived());
    }

    private IEnumerator AnimateBuffReceived()
    {
        _buffReceivedPanel.gameObject.SetActive(true);

        float scaleUpDuration = 0.25f;
        float overshootScale = 1.2f;  // a bit bigger than 1 for the bounce
        float settleDuration = 0.15f;
        float holdTime = 1f;

        // Scale up beyond 1 (overshoot)
        yield return ScaleTo(Vector3.one * overshootScale, scaleUpDuration);

        // Bounce back to 1 (normal size)
        yield return ScaleTo(Vector3.one, settleDuration);

        // Hold at full size
        yield return new WaitForSeconds(holdTime);

        // Scale back down
        yield return ScaleTo(_ogScale, 0.25f);

        _buffReceivedPanel.gameObject.SetActive(false);
        _displayRoutine = null;
    }


    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = _buffReceivedPanel.localScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            // Smooth easing (feels less linear)
            t = Mathf.SmoothStep(0f, 1f, t);

            _buffReceivedPanel.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        _buffReceivedPanel.localScale = target;
    }

}
