using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerContext
{
    

    public Transform RootTransform;
    public CharacterController CharacterController;
    public Animator Animator;
    public Transform CameraTransform;
    public MultiAimConstraint AimConstraint;    
    public Transform AimTarget;
    public float NextAnimCrossFadeTime;

    public Vector2 MoveInput;
    public bool IsAiming;
    public bool IsShooting;

    public float CurrentSpeed;
    
    private float _targetSpeed;
    public float TargetSpeed
    {
        get => _targetSpeed;
        set
        {
            _targetSpeed = value;
        }
    }

    public Vector3 LastMoveDirection;
    public Vector3 HorizontalVelocity;
    public float VelocityY;

    public CinemachineRotationComposer CinemachineRotationComposer;
    public CinemachineOrbitalFollow CinemachineOrbitalFollow;

    public bool IsReloading;
    public float ReloadTime;
    public PlayerContext(Transform rootTransform, 
        CharacterController characterController, 
        Animator animator, 
        Transform cameraTransform, 
        MultiAimConstraint aimConstraint,
        CinemachineRotationComposer cinemachineRotationComposer,
        CinemachineOrbitalFollow cinemachineOrbitalFollow
        )
    {
        RootTransform = rootTransform;
        CharacterController = characterController;
        Animator = animator;
        CameraTransform = cameraTransform;
        AimConstraint = aimConstraint;
        CinemachineRotationComposer = cinemachineRotationComposer;
        CinemachineOrbitalFollow = cinemachineOrbitalFollow;
        IsReloading = false;
    }
}
