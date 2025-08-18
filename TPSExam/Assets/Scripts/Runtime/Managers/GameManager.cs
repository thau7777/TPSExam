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
    public Action<int> onPickupAmmo;

    public Action onPlayerHit;
    public Action onPlayerDeath;
    public Action<int> onEnemyDeath;

    // Pass minutes and seconds to listeners
    public Action<int, int> onMinuteIncrease;

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
            OnMinuteIncrease(minutes, seconds);
        }

        // Also update every frame if you want live seconds display
        OnMinuteIncrease(minutes, seconds);
    }

    private void OnMinuteIncrease(int minutes, int seconds)
    {
        onMinuteIncrease?.Invoke(minutes, seconds);
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
}
