using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

public class PlayerStateMachine : StateManager<PlayerStateMachine.EPlayerState>
{
    [Header("References")]
    [SerializeField] private Transform _gun;
    private bool _wasInDanger = false;

    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private MultiAimConstraint _aimConstraint;
    public enum EPlayerState
    {
        Idle,
        Run,
        Jump,
        Aim,
        Shoot,
        Reload,
        Dead
    }

    private PlayerContext _context;
    public UnityEvent<bool, float> EnableCameraShaking;

    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Shooter _shooter;
    private Damageable _damageable;

    [SerializeField]
    private float _strafeSpeed = 1.5f;

    [SerializeField]
    private float _runSpeed = 3f;

    CinemachineRotationComposer cameraRotationComposer;
    CinemachineOrbitalFollow cameraOrbitalFollow;
    private Coroutine _zoomCoroutine;

    Vector2 ogScreenPosition;
    float ogCenterRadius;
    Vector3 ogTargetOffset;

    // store original constraint offset so we can restore
    Vector3 ogAimConstraintOffset;

    [Header("Buffs Value")]
    [SerializeField] private int _healthRegenPercent = 1; // percent per second

    private bool _isDead = false;
    protected override void OnEnable()
    {
        base.OnEnable();
        ModifyInputActionsEvents(true);
        OnStateChanged += OnStateChangedListener;

        GameManager.Instance.onPickupSpeedBuff += OnPickupSpeedBuff;
        GameManager.Instance.onPickupMaxHealthBuff += OnPickupMaxHealthBuff;
        GameManager.Instance.onPickupHealthRegenBuff += OnPickupHealthRegenBuff;
        GameManager.Instance.onPickupInstantHealBuff += OnPickupInstantHealBuff;
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        ModifyInputActionsEvents(false);
        OnStateChanged -= OnStateChangedListener;

        GameManager.Instance.onPickupSpeedBuff -= OnPickupSpeedBuff;
        GameManager.Instance.onPickupMaxHealthBuff -= OnPickupMaxHealthBuff;
        GameManager.Instance.onPickupHealthRegenBuff -= OnPickupHealthRegenBuff;
        GameManager.Instance.onPickupInstantHealBuff -= OnPickupInstantHealBuff;

        _context.onIsJumpingChange -= _shooter.SetIsJumping;
    }


    private void OnPickupInstantHealBuff(int obj)
    {
        _damageable.Heal(obj);
        GameManager.Instance.onCurrentHealthChange?.Invoke(_damageable.CurrentHealth,_damageable.MaxHealth);
        if (_damageable.CurrentHealth > _damageable.MaxHealth * 0.3f && _wasInDanger)
        {
            GameManager.Instance.OnPlayerHealthDanger(false);
            _wasInDanger = false;
        }

    }

    private void OnPickupHealthRegenBuff(int obj)
    {
        _healthRegenPercent += obj;
    }
    private IEnumerator HealthRegenHandler()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (_damageable.CurrentHealth < _damageable.MaxHealth)
            {
                int regenAmount = Mathf.Max(1, Mathf.RoundToInt(_damageable.MaxHealth * (_healthRegenPercent / 100f)));

                // ✅ Use Heal() instead of manually changing CurrentHealth
                _damageable.Heal(regenAmount);

                // If you want to keep the GameManager event, do it inside Heal() 
                // or just keep this call:
                GameManager.Instance.onCurrentHealthChange?.Invoke(_damageable.CurrentHealth, _damageable.MaxHealth);
            }

            if (_damageable.CurrentHealth > _damageable.MaxHealth * 0.3f && _wasInDanger)
            {
                GameManager.Instance.OnPlayerHealthDanger(false);
                _wasInDanger = false;
            }
        }
    }

    private void OnPickupMaxHealthBuff(int percent)
    {
        // Increase MaxHealth by given percent
        _damageable.MaxHealth += Mathf.RoundToInt(_damageable.MaxHealth * (percent / 100f));
    }

    private void OnPickupSpeedBuff(float percent)
    {
        // Increase run speed by given percent
        _runSpeed += _runSpeed * (percent / 100f);
        _runSpeed = Mathf.Clamp(_runSpeed, 1f, 10f);
    }



    private void Awake()
    {
        _shooter = GetComponent<Shooter>();
        _damageable = GetComponent<Damageable>();
        _cameraTransform = GameObject.FindWithTag("CinemachineCamera").transform;
        cameraRotationComposer = _cameraTransform.GetComponent<CinemachineRotationComposer>();
        cameraOrbitalFollow = _cameraTransform.GetComponent<CinemachineOrbitalFollow>();
        ogScreenPosition = cameraRotationComposer.Composition.ScreenPosition;
        ogCenterRadius = cameraOrbitalFollow.Orbits.Center.Radius;
        ogTargetOffset = cameraOrbitalFollow.TargetOffset;
        _context = new PlayerContext
        (
            transform,
            _characterController,
            _animator,
            _cameraTransform,
            _aimConstraint,
            cameraRotationComposer,
            cameraOrbitalFollow
        );

        // store original aim constraint offset
        ogAimConstraintOffset = _aimConstraint.data.offset;

        InitializeStates();

        _context.onIsJumpingChange += _shooter.SetIsJumping;

    }
    private void InitializeStates()
    {
        States.Add(EPlayerState.Idle, new IdleState(_context, EPlayerState.Idle));
        States.Add(EPlayerState.Run, new RunState(_context, EPlayerState.Run));
        States.Add(EPlayerState.Jump, new JumpState(_context, EPlayerState.Jump));
        States.Add(EPlayerState.Aim, new AimState(_context, EPlayerState.Aim));
        States.Add(EPlayerState.Reload, new ReloadState(_context, EPlayerState.Reload));
        States.Add(EPlayerState.Dead, new DeadState(_context, EPlayerState.Dead));


        CurrentState = States[EPlayerState.Idle];

    }

    private void Start()
    {
        StartCoroutine(HealthRegenHandler());
    }
    private void ModifyInputActionsEvents(bool enable)
    {
        if (enable)
        {
            _inputReader.onMove += OnMove;
            _inputReader.onReload += OnReload;
            _inputReader.onAim += OnAim;
            _inputReader.onJump += OnJump;
        }
        else
        {
            _inputReader.onMove -= OnMove;
            _inputReader.onReload -= OnReload;
            _inputReader.onAim -= OnAim;
            _inputReader.onJump -= OnJump;
        }
    }


    protected override void Update()
    {
        UpdateMovement();
        base.Update();
    }
    private void UpdateMovement()
    {
        if (!_context.IsAiming)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_context.LastMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 650 * Time.deltaTime);
        }


        if (_context.CharacterController.isGrounded && _context.VelocityY < 0)
            _context.VelocityY = -2f;
        else if (!_context.CharacterController.isGrounded)
            _context.VelocityY += Physics.gravity.y * Time.deltaTime;

        Vector3 verticalMovement = new Vector3(0f, _context.VelocityY, 0f);

        Vector3 finalVelocity = _context.HorizontalVelocity + verticalMovement;
        _context.CharacterController.Move(finalVelocity * Time.deltaTime);
        _context.HorizontalVelocity = Vector3.zero; // Reset final velocity after applying movement
    }

   
    private void OnStateChangedListener(EPlayerState state)
    {
        switch (state)
        {

            case EPlayerState.Run:
                EnableCameraShaking?.Invoke(true, _context.CurrentSpeed);
                break;

            default:
                EnableCameraShaking?.Invoke(false, 0f);
                
                break;
        }

    }
    
    private void OnJump()
    {
        if (_context.IsAiming || _context.IsReloading)
            return;
        _context.IsJumping = true;
    }

    private void OnReload()
    {
        if (_shooter.CurrentAmmo == _shooter.AmmoPerCapacity || _shooter.TotalAmmoRemaining == 0 || _context.IsJumping)
            return;
        AudioManager.Instance.PlaySFX("Reload");
        _context.IsReloading = true;
        if (_context.MoveInput != Vector2.zero)
        {
            _context.TargetSpeed = _strafeSpeed;
        }
        else
        {
            _context.TargetSpeed = 0f;
        }
        if(_zoomCoroutine == null) _zoomCoroutine = StartCoroutine(ResetCameraSettings(0.5f));
    }

    private void OnAim(bool value)
    {
        if (_context.IsJumping || _context.IsReloading)
            return;


        _context.IsAiming = value;
        _context.TargetSpeed = value ? _strafeSpeed : _runSpeed;

        if (value && _zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = null;
        }
        if (!value)
        {
            _zoomCoroutine = StartCoroutine(ResetCameraSettings(0.5f));
        }
    }


    private IEnumerator ResetCameraSettings(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpValue = t / duration;

            cameraRotationComposer.Composition.ScreenPosition =
                Vector2.Lerp(cameraRotationComposer.Composition.ScreenPosition, ogScreenPosition, lerpValue);

            cameraOrbitalFollow.Radius =
                Mathf.Lerp(cameraOrbitalFollow.Radius, ogCenterRadius, lerpValue);

            _context.AimConstraint.weight =
                Mathf.Lerp(_context.AimConstraint.weight, 0, lerpValue);

            cameraOrbitalFollow.TargetOffset =
                Vector3.Lerp(cameraOrbitalFollow.TargetOffset, ogTargetOffset, lerpValue);

            // also lerp the aim constraint offset back to original
            if (_aimConstraint != null)
            {
                var d = _aimConstraint.data;
                d.offset = Vector3.Lerp(d.offset, ogAimConstraintOffset, lerpValue);
                _aimConstraint.data = d;
            }

            yield return null;
        }

        // Snap to final
        cameraRotationComposer.Composition.ScreenPosition = ogScreenPosition;
        cameraOrbitalFollow.Orbits.Center.Radius = ogCenterRadius;
        _context.AimConstraint.weight = 0;
        cameraOrbitalFollow.TargetOffset = ogTargetOffset;

        if (_aimConstraint != null)
        {
            var d = _aimConstraint.data;
            d.offset = ogAimConstraintOffset;
            _aimConstraint.data = d;
        }

        //_context.CinemachineOrbitalFollow.HorizontalAxis.Value = AimHorizontalAfter;
    }


    private void OnMove(Vector2 vector)
    {
        _context.MoveInput = vector;
        if (vector != Vector2.zero)
        {
            _context.TargetSpeed = _context.IsAiming || _context.IsReloading ? _strafeSpeed : _runSpeed;
        }
        else
        {
            _context.TargetSpeed = 0f;
        }
    }

    public void OnReloadBuffPickup(float percent)
    {
        float currentReloadSpeed = _animator.GetFloat("ReloadSpeed");
        _animator.SetFloat("ReloadSpeed", currentReloadSpeed + percent);
    }

    #region  Animation Events
    public void OnReloadAnimation()
    {
        _gun.SetParent(_leftHand);
    }

    public void OnReloadAnimationEnd()
    {
        _context.IsReloading = false;
        _gun.SetParent(_rightHand);

        if (_context.MoveInput != Vector2.zero)
        {
            _context.TargetSpeed = _context.IsAiming ? _strafeSpeed : _runSpeed;
        }
        else
        {
            _context.TargetSpeed = 0f;
        }

    }
    [ContextMenu("TestDie")]
    public void TestDie()
    {
        OnTakeDamage(0);
    }
    public void OnTakeDamage(int currentHealth)
    {
        if(_isDead) return;
        GameManager.Instance.onCurrentHealthChange?.Invoke(currentHealth, _damageable.MaxHealth);

        if (currentHealth <= 0)
        {
            AudioManager.Instance.PlayMusic("GameOver");
            _animator.CrossFade("Die", 0.1f);
            _inputReader.DisableInput();
            _isDead = true;
        }
        else
        {
            AudioManager.Instance.PlaySFX("PlayerHit");
            bool isDanger = currentHealth <= _damageable.MaxHealth * 0.3f;

            // Only fire event if state changed
            if (isDanger != _wasInDanger)
            {
                GameManager.Instance.OnPlayerHealthDanger(isDanger);
                _wasInDanger = isDanger;
            }
        }
    }


    public void OnDeadAnimationEnd()
    {
        GameManager.Instance.onGameOver?.Invoke(false);
        GameManager.Instance.EnableMouse();
    }
    #endregion

}
