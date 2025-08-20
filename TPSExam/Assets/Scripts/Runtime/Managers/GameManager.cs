using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private BuffReceivedDisplay _buffReceivedDisplay;


    [SerializeField] private RectTransform _instructionPanel;
    [SerializeField] private float _slideDuration = 0.5f; // speed of sliding

    private Vector2 _hiddenPos;
    private Vector2 _visiblePos;
    private Coroutine _slideCoroutine;
    private bool _isInstructionVisible = false;


    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _SettingsMenu;
    [SerializeField] private InputReader _inputReader;

    public Action onGameStarted;
    public Action onGamePaused;
    public Action<bool> onGameOver;

    public Action<int> onChangeShootingMode;
    public Action<int, int> onAmmoChange;
    public Action<string, float> onSpecialShoot;
    public Action<Shooter.EShootingMethod> onOutOfAmmo;

    public Action<int, int> onCurrentHealthChange;
    public Action<bool> onCurrentHealthDanger;
    public Action<int> onEnemyDeath;

    public Action<int> onMinuteIncrease;
    public Action<int, int> onSecondIncrease;

    // Buff pickup events
    public Action<int> onPickupAmmo;
    public Action<float> onPickupSpeedBuff;
    public Action<int> onPickupBulletDamageBuff;
    public Action<int> onPickupGrenadeDamageBuff;
    public Action<int> onPickupMaxHealthBuff;
    public Action<float> onPickupInfinityBulletBuff;
    public Action<float> onPickupInfinityGrenadeBuff;
    public Action<int> onPickupHealthRegenBuff;
    public Action<int> onPickupInstantHealBuff;
    public Action onPickupKillAllEnemiesBuff;
    public Action<float> onPickupReloadSpeedBuff;

    private float _elapsedTime = 0f;
    private int _currentMinute = 0;

    private void OnEnable()
    {
        _inputReader.onPause += PauseGame;
        _inputReader.onShowInstruction += ShowInstruction;  
    }

    private void OnDisable()
    {
        _inputReader.onPause -= PauseGame;
        _inputReader.onShowInstruction -= ShowInstruction;
    }
    private void Start()
    {
        float panelWidth = _instructionPanel.rect.width;

        // set positions
        _visiblePos = _instructionPanel.anchoredPosition;
        _hiddenPos = _visiblePos + Vector2.left * panelWidth;

        // start hidden
        _instructionPanel.anchoredPosition = _hiddenPos;
    }
    private void PauseGame()
    {
        if(_SettingsMenu.activeSelf)
        {
            _SettingsMenu.SetActive(false);
            return; // Don't pause if settings menu is open
        }
        bool isPaused = !_pauseMenu.activeSelf;
        _pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1; // Pause the game
        onGamePaused?.Invoke();
    }

    private void ShowInstruction()
    {
        // If already sliding, stop it
        if (_slideCoroutine != null)
            StopCoroutine(_slideCoroutine);

        // Toggle target based on current state
        Vector2 target = _isInstructionVisible ? _hiddenPos : _visiblePos;

        // Start animation
        _slideCoroutine = StartCoroutine(SlidePanel(_instructionPanel, _instructionPanel.anchoredPosition, target));

        // Flip state
        _isInstructionVisible = !_isInstructionVisible;
    }

    private IEnumerator SlidePanel(RectTransform panel, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime; // use unscaled so it works even if paused
            float t = Mathf.Clamp01(elapsed / _slideDuration);
            panel.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        panel.anchoredPosition = to;
        _slideCoroutine = null;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);

        if (minutes > _currentMinute)
        {
            _currentMinute = minutes;
            onMinuteIncrease?.Invoke(10);
        }

        OnSecondIncrease(minutes, seconds);
    }

    private void OnSecondIncrease(int minutes, int seconds)
    {
        onSecondIncrease?.Invoke(minutes, seconds);
    }

    public void OnPlayerHealthDanger(bool isDanger) => onCurrentHealthDanger?.Invoke(isDanger);

    public void UpdateAmmo(int currentAmmo, int ammoRemains) => onAmmoChange?.Invoke(currentAmmo, ammoRemains);

    public void UpdateShootingMode(int modeIndex) => onChangeShootingMode?.Invoke(modeIndex);

    public void OnEnemyDeath(int scoreToAdd) => onEnemyDeath?.Invoke(scoreToAdd);

    public void ApplyBuff(Buff.BuffType buffType)
    {
        _buffReceivedDisplay.ShowBuffReceived(buffType);
        switch (buffType)
        {
            case Buff.BuffType.MovementSpeed: onPickupSpeedBuff?.Invoke(5); break;
            case Buff.BuffType.BulletDamage: onPickupBulletDamageBuff?.Invoke(10); break;
            case Buff.BuffType.GrenadeDamage: onPickupGrenadeDamageBuff?.Invoke(10); break;
            case Buff.BuffType.MaxHealth: onPickupMaxHealthBuff?.Invoke(20); break;
            case Buff.BuffType.Ammo: onPickupAmmo?.Invoke(100); break;
            case Buff.BuffType.InfinityBullet: onPickupInfinityBulletBuff?.Invoke(10); break;
            case Buff.BuffType.InfinityGrenade: onPickupInfinityGrenadeBuff?.Invoke(10); break;
            case Buff.BuffType.HealthRegen: onPickupHealthRegenBuff?.Invoke(1); break;
            case Buff.BuffType.InstantHeal: onPickupInstantHealBuff?.Invoke(50); break;
            case Buff.BuffType.KillAllEnemies: onPickupKillAllEnemiesBuff?.Invoke(); break;
            case Buff.BuffType.ReloadSpeed: onPickupReloadSpeedBuff?.Invoke(0.1f); break;
        }
    }

    // 🔄 Reset everything when restarting
    public void ResetGameManager()
    {
        _elapsedTime = 0f;
        _currentMinute = 0;

        // Clear all event subscriptions so they don't double-up
        onGameStarted = null;
        onGamePaused = null;
        onGameOver = null;

        onChangeShootingMode = null;
        onAmmoChange = null;
        onSpecialShoot = null;
        onOutOfAmmo = null;

        onCurrentHealthChange = null;
        onCurrentHealthDanger = null;
        onEnemyDeath = null;

        onMinuteIncrease = null;
        onSecondIncrease = null;

        onPickupAmmo = null;
        onPickupSpeedBuff = null;
        onPickupBulletDamageBuff = null;
        onPickupGrenadeDamageBuff = null;
        onPickupMaxHealthBuff = null;
        onPickupInfinityBulletBuff = null;
        onPickupInfinityGrenadeBuff = null;
        onPickupHealthRegenBuff = null;
        onPickupInstantHealBuff = null;
        onPickupKillAllEnemiesBuff = null;
        onPickupReloadSpeedBuff = null;
    }

    public void Restart()
    {
        //_elapsedTime = 0f;
        //_currentMinute = 0;
        //ResetGameManager(); // 👈 clear state
        //BuffManager.Instance.ResetManager();
        //EnemyManager.Instance.ResetManager();
        //ParticleManager.Instance.ResetManager();
        Time.timeScale = 1; // Ensure time is running
        _inputReader.EnableInput();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name,LoadSceneMode.Single);
    }
    
    public void QuitToMainMenu()
    {
        ResetGameManager(); // 👈 clear state before switching scene
        SceneManager.LoadScene(0);
    }

    public void QuitToDesktop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
