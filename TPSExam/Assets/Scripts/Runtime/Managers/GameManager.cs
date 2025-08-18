using System;
using TMPro;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    public Action onGameStarted;
    public Action onGamePaused;
    public Action onGameOver;

    public Action<int> onChangeShootingMode;
    public Action<int,int> onAmmoChange;

    public Action<bool> onCurrentHealthDanger;
    public Action onPlayerDeath;
    public Action<int> onEnemyDeath;

    // Pass minutes and seconds to listeners
    public Action<float> onMinuteIncrease; // Triggered when a minute increases
    public Action<int, int> onSecondIncrease;

    //Buff pickup event
    public Action<int> onPickupAmmo;
    public Action<float> onPickupSpeedBuff;
    public Action<float> onPickupBulletDamageBuff;
    public Action<float> onPickupGrenadeDamageBuff;
    public Action<float> onPickupMaxHealthBuff;
    public Action<float> onPickupInfinityBulletBuff;
    public Action<float> onPickupInfinityGrenadeBuff;
    public Action<float> onPickupHealthRegenBuff;
    public Action<float> onPickupInstantHealBuff;
    public Action onPickupKillAllEnemiesBuff;
    public Action<float> onPickupReloadSpeedBuff;

    private float _elapsedTime = 0f;
    private int _currentMinute = 0;

    private void Update()
    {
        // Count time each frame
        _elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);

        // Trigger when minutes change
        if (minutes > _currentMinute)
        {
            _currentMinute = minutes;
            onMinuteIncrease?.Invoke(10);
        }

        // Also update every frame if you want live seconds display
        OnSecondIncrease(minutes, seconds);
    }

    public void OnPlayerHealthDanger(bool isDanger)
    {
        onCurrentHealthDanger?.Invoke(isDanger);
    }

    private void OnSecondIncrease(int minutes, int seconds)
    {
        onSecondIncrease?.Invoke(minutes, seconds);
    }

    public void UpdateAmmo(int currentAmmo, int ammoRemains)
    {
        onAmmoChange?.Invoke(currentAmmo, ammoRemains);
    }

    public void UpdateShootingMode(int modeIndex)
    {
        onChangeShootingMode?.Invoke(modeIndex);
    }

    public void OnEnemyDeath(int scoreToAdd)
    {
        onEnemyDeath?.Invoke(scoreToAdd);
    }


    public void ApplyBuff(Buff.BuffType buffType)
    {
        switch(buffType)
        {
            case Buff.BuffType.Speed:
                onPickupSpeedBuff?.Invoke(2); // percent increase
                break;
            case Buff.BuffType.BulletDamage:
                onPickupBulletDamageBuff?.Invoke(10); // percent increase
                break;
            case Buff.BuffType.GrenadeDamage:
                onPickupGrenadeDamageBuff?.Invoke(10); // percent increase
                break;
            case Buff.BuffType.MaxHealth:
                onPickupMaxHealthBuff?.Invoke(20); // percent increase
                break;
            case Buff.BuffType.Ammo:
                onPickupAmmo?.Invoke(100); // amount to add
                break;
            case Buff.BuffType.InfinityBullet:
                onPickupInfinityBulletBuff?.Invoke(10); // duration in seconds
                break;
            case Buff.BuffType.InfinityGrenade:
                onPickupInfinityGrenadeBuff?.Invoke(10); // duration in seconds
                break;
            case Buff.BuffType.HealthRegen:
                onPickupHealthRegenBuff?.Invoke(1); // increase the health regen rate
                break;
            case Buff.BuffType.InstantHeal:
                onPickupInstantHealBuff?.Invoke(50); // amount to heal
                break;
            case Buff.BuffType.KillAllEnemies:
                onPickupKillAllEnemiesBuff?.Invoke(); // no parameters needed
                break;
            case Buff.BuffType.ReloadSpeed:
                onPickupReloadSpeedBuff?.Invoke(0.1f); // percent increase
                break;
        }
    }
}
