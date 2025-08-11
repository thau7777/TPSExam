using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerStateMachine : StateManager<PlayerStateMachine.EPlayerState>
{
    [Header("References")]
    [SerializeField] private Transform _gun;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private MultiAimConstraint _aimConstraint;
    [SerializeField] private Transform _aimTarget;
    [SerializeField] private Transform _shootingPoint;
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

    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _cameraTransform;

    [SerializeField]
    private float _walkSpeed = 1.5f;

    [SerializeField]
    private float _runSpeed = 3f;



    CinemachineRotationComposer cameraRotationComposer;
    CinemachineOrbitalFollow cameraOrbitalFollow;
    private Coroutine _zoomCoroutine;

    Vector2 ogScreenPosition;
    float ogCenterRadius;
    Vector3 ogTargetOffset;

    bool returningToCenter = false;
    [Header("Recoil Settings")]
    [SerializeField] float recoilReturnSpeed = 5f; // how fast it returns

    [MinMaxRangeSlider(-5f, 5f)]
    [SerializeField]
    private Vector2 AutoShotRecoilRangeX = new Vector2(-0.01f, 0.02f); // range of recoil offset

    [SerializeField]
    [Range(0f, 10f)]
    private float AutoShotRecoilY = 5f; // fixed recoil offset in Y direction

    [MinMaxRangeSlider(-0.1f, 0.1f)]
    [SerializeField]
    private Vector2 BurstShotRecoilRangeX = new Vector2(-0.01f, 0.02f); // range of recoil offset

    [SerializeField]
    [Range(0.01f, 0.05f)]
    private float BurstShotRecoilY = 0.03f; // fixed recoil offset in Y direction

    [MinMaxRangeSlider(-0.1f, 0.1f)]
    [SerializeField]
    private Vector2 SingleShotRecoilRangeX = new Vector2(-0.01f, 0.02f); // range of recoil offset

    [SerializeField]
    [Range(0.01f, 0.05f)]
    private float SingleShotRecoilY = 0.03f; // fixed recoil offset in Y direction

    // --- New: limits for the aimConstraint offset so recoil can't exceed boundaries ---
    [Header("Aim Constraint Offset Limits")]
    [SerializeField]
    private Vector2 AimOffsetLimitX = new Vector2(-0.1f, 0.1f); // clamp X offset min/max

    // store original constraint offset so we can restore
    Vector3 ogAimConstraintOffset;


    protected override void OnEnable()
    {
        base.OnEnable();
        ModifyInputActionsEvents(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ModifyInputActionsEvents(false);
    }

    private void Awake()
    {
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
    }
    private void InitializeStates()
    {
        States.Add(EPlayerState.Idle, new IdleState(_context, EPlayerState.Idle));
        States.Add(EPlayerState.Run, new RunState(_context, EPlayerState.Run));
        States.Add(EPlayerState.Jump, new JumpState(_context, EPlayerState.Jump));
        States.Add(EPlayerState.Aim, new AimState(_context, EPlayerState.Aim));
        States.Add(EPlayerState.Shoot, new ShootState(_context, EPlayerState.Shoot));
        States.Add(EPlayerState.Reload, new ReloadState(_context, EPlayerState.Reload));
        States.Add(EPlayerState.Dead, new DeadState(_context, EPlayerState.Dead));


        CurrentState = States[EPlayerState.Idle];

    }


    private void ModifyInputActionsEvents(bool enable)
    {
        if (enable)
        {
            _inputReader.onMove += OnMove;
            _inputReader.onAim += OnAim;
            _inputReader.onShoot += OnShoot;
            _inputReader.onJump += OnJump;
            _inputReader.onChangeShootingMethod += OnChangeShootingMethod;
        }
        else
        {
            _inputReader.onMove -= OnMove;
            _inputReader.onAim -= OnAim;
            _inputReader.onShoot -= OnShoot;
            _inputReader.onJump -= OnJump;
            _inputReader.onChangeShootingMethod -= OnChangeShootingMethod;
        }
    }
    protected override void Update()
    {
        UpdateMovement();
        //UpdateRecoilRecovery();
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

    //void UpdateRecoilRecovery()
    //{
    //    if (returningToCenter && _aimConstraint != null)
    //    {
    //        // lerp the constraint offset back to saved before-shoot offset
    //        var d = _aimConstraint.data;
    //        d.offset = Vector3.Lerp(d.offset, ogAimConstraintOffset, Time.deltaTime * recoilReturnSpeed);
    //        _aimConstraint.data = d;

    //        // Stop lerping when close enough
    //        if (Vector3.Distance(d.offset, ogAimConstraintOffset) < 0.001f)
    //        {
    //            d.offset = ogAimConstraintOffset;
    //            _aimConstraint.data = d;
    //            returningToCenter = false;
    //        }
    //    }
    //}
    private void OnJump()
    {
        throw new NotImplementedException();
    }

    private void OnShoot(bool value)
    {
        _context.IsShooting = value;
    }

    private void OnAim(bool value)
    {
        _context.IsAiming = value;
        _context.TargetSpeed = value ? _walkSpeed : _runSpeed;


        if (value && _zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }
        if (!value)
        {
            returningToCenter = false;
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

    private void OnChangeShootingMethod()
    {
        _context.CurrentShootingMethod =
        (PlayerContext.EShootingMethod)(((int)_context.CurrentShootingMethod + 1) %
        System.Enum.GetValues(typeof(PlayerContext.EShootingMethod)).Length);
    }

    private void OnMove(Vector2 vector)
    {
        _context.MoveInput = vector;
        if (vector != Vector2.zero)
        {
            _context.TargetSpeed = _context.IsAiming ? _walkSpeed : _runSpeed;
        }
        else
        {
            _context.TargetSpeed = 0f;
        }
    }

    #region  Animation Events
    public void OnReloadAnimation()
    {
        _gun.SetParent(_leftHand);
    }

    public void OnReloadAnimationEnd()
    {
        _gun.SetParent(_rightHand);
    }

    public void OnShootAnimation()
    {
        //RecoilSetter();
        StartShoot();
    }

    //void RecoilSetter()
    //{
    //    if (_aimConstraint == null)
    //        return;

    //    // Save original offset if we are not currently returning
    //      //  aimConstraintBeforeShoot = _aimConstraint.data.offset;

    //    // Calculate recoil offset in local offset-space (X,Y)
    //    float xOffset = 0f;
    //    float yOffset = 0f;

    //    switch (_context.CurrentShootingMethod)
    //    {
    //        case PlayerContext.EShootingMethod.Auto:
    //            xOffset = UnityEngine.Random.Range(AutoShotRecoilRangeX.x, AutoShotRecoilRangeX.y);
    //            yOffset = AutoShotRecoilY;
    //            break;
    //        case PlayerContext.EShootingMethod.Burst:
    //            xOffset = UnityEngine.Random.Range(BurstShotRecoilRangeX.x, BurstShotRecoilRangeX.y);
    //            yOffset = BurstShotRecoilY;
    //            break;
    //        case PlayerContext.EShootingMethod.Single:
    //            xOffset = UnityEngine.Random.Range(SingleShotRecoilRangeX.x, SingleShotRecoilRangeX.y);
    //            yOffset = SingleShotRecoilY;
    //            break;
    //    }

    //    // Apply recoil by modifying the MultiAimConstraint.data.offset and clamp within limits
    //    var d = _aimConstraint.data;
    //    Vector3 current = d.offset;
    //    Vector3 target = current + new Vector3(0, xOffset, yOffset);

    //    // Clamp X and Y separately
    //    target.y = Mathf.Clamp(target.y, AimOffsetLimitX.x, AimOffsetLimitX.y);
        

    //    d.offset = target;
    //    _aimConstraint.data = d;
    //    Debug.Log($"Recoil applied: {_aimConstraint.data.offset}");
    //    // Start returning to saved before-shoot offset
    //    returningToCenter = true;
    //}

    void StartShoot()
    {
        var direction = _aimTarget.position - _shootingPoint.position;
        Ray ray = new Ray(_shootingPoint.position, direction.normalized);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
        {
            Debug.Log($"Hit: {hitInfo.collider.name}");
            // Here you can add logic to handle the hit, like damaging an enemy
        }
        else
        {
            Debug.Log("Missed!");
        }
    }
    #endregion
}
