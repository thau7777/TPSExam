using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    private Animator _animator;

    public enum EShootingMethod
    {
        Auto,
        BurstShot,
        SingleShot
    }

    
    public EShootingMethod CurrentShootingMethod;
    public Transform aimTarget;
    public Color debugRayColor = Color.green;

    #region AimSettings
    public float maxDistance = 100f;

    [Tooltip("Select all layers to ignore in the Inspector")]
    public LayerMask ignoreLayers;
    public LayerMask shootIgnoreLayers;

    private Vector3 targetPosition;
    public float smoothTime = 0.1f;
    private Vector3 currentVelocity;
    [SerializeField] private Transform _shootingPoint;
    #endregion

    [Header("Shooting Settings")]
    [SerializeField] private float _singleShotCooldown = 0.25f;
    [SerializeField] private float _burstShotCooldown = 0.5f;
    private float _lastSingleShotTime = 0f;
    private float _lastBurstShotTime = 0f;

    private bool _isShootingHeld; // from input
    private bool _isAiming;
    private bool _isJumping;

    [SerializeField]
    private float _bulletDamage = 20; // Damage dealt by bullets
    [SerializeField]
    private ParticleSystem _muzzleFlash;
    [SerializeField]
    private MuzzleLightController _muzzleLight;
    [SerializeField]
    private float _grenadeLaunchForce = 20f;
    [SerializeField]
    private GameObject _grenadePrefab;
    [SerializeField]
    private float _grenadeDamage = 50; // Damage dealt by the grenade
    [SerializeField]
    private float _grenadeKnockbackForce = 5; // Damage dealt by the grenade
    [SerializeField]
    private int _currentAmmo = 30; // Current ammo count
    public int CurrentAmmo
    {
        get => _currentAmmo;
        private set
        {
            _currentAmmo = value;
            if(_currentAmmo == 0 && CurrentShootingMethod == EShootingMethod.Auto)
            {
                _animator.SetBool("IsAutoShot", false); // Loop anim until release
                _muzzleFlash.Stop();
                _muzzleLight.StopLooping();
                GameManager.Instance.onOutOfAmmo?.Invoke(CurrentShootingMethod);
                AudioManager.Instance.PlaySFX("NoAmmoShoot");
            }
        }
    }

    [SerializeField]
    private int _ammoPerCapacity = 30;
    public int AmmoPerCapacity
    {
        get => _ammoPerCapacity;
        private set
        {
            _ammoPerCapacity = value;
        }
    }

    [SerializeField]
    private int _totalAmmoRemaining = 90; // Total ammo available]
    public int TotalAmmoRemaining
    {
        get => _totalAmmoRemaining;
        private set
        {
            _totalAmmoRemaining = value;
        }
    }


    // Fields for buff tracking
    private Coroutine _infinityBulletCoroutine;
    private Coroutine _infinityGrenadeCoroutine;
    private bool _hasInfinityBullet;
    private bool _hasInfinityGrenade;

    private void OnEnable()
    {
        _inputReader.onAim += SetIsAiming;
        _inputReader.onShoot += OnShoot;
        _inputReader.onChangeShootingMethod += OnChangeShootingMethod;


        GameManager.Instance.onPickupAmmo += OnPickupAmmo;
        GameManager.Instance.onPickupReloadSpeedBuff += OnPickupReloadSpeedBuff;
        GameManager.Instance.onPickupBulletDamageBuff += OnPickupBulletDamageBuff;
        GameManager.Instance.onPickupGrenadeDamageBuff += OnPickupGrenadeDamageBuff;
        GameManager.Instance.onPickupInfinityBulletBuff += OnPickupInfinityBulletBuff;
        GameManager.Instance.onPickupInfinityGrenadeBuff += OnPickupInfinityGrenadeBuff;
    }


    private void OnDisable()
    {
        _inputReader.onAim -= SetIsAiming;
        _inputReader.onShoot -= OnShoot;
        _inputReader.onChangeShootingMethod -= OnChangeShootingMethod;

        GameManager.Instance.onPickupAmmo -= OnPickupAmmo;
        GameManager.Instance.onPickupReloadSpeedBuff -= OnPickupReloadSpeedBuff;
        GameManager.Instance.onPickupBulletDamageBuff -= OnPickupBulletDamageBuff;
        GameManager.Instance.onPickupGrenadeDamageBuff -= OnPickupGrenadeDamageBuff;
        GameManager.Instance.onPickupInfinityBulletBuff -= OnPickupInfinityBulletBuff;
        GameManager.Instance.onPickupInfinityGrenadeBuff -= OnPickupInfinityGrenadeBuff;
    }

    

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        CurrentShootingMethod = EShootingMethod.Auto; // default
    }

    private void LateUpdate()
    {
        if (!_isAiming) return;
        // Aiming logic
        Ray ray = new Ray(_shootingPoint.position, _shootingPoint.up);
        Debug.DrawRay(_shootingPoint.position, _shootingPoint.up * maxDistance, debugRayColor);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool foundHit = false;
        foreach (var hit in hits)
        {
            if ((ignoreLayers.value & (1 << hit.collider.gameObject.layer)) != 0)
                continue;

            targetPosition = hit.point;
            foundHit = true;
            break;
        }

        if (!foundHit)
            targetPosition = ray.origin + ray.direction * maxDistance;

        aimTarget.position = Vector3.SmoothDamp(
            aimTarget.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
    public void SetIsJumping(bool value)
    {
        _isJumping = value;
    }
    public void SetIsAiming(bool value)
    {
        if (_isJumping)
            return;
        _isAiming = value;
        if (!_isAiming && CurrentShootingMethod == EShootingMethod.Auto)
        {
            _muzzleFlash.Stop();
            _muzzleLight.StopLooping();
            _animator.SetBool("IsAutoShot", false); // Stop auto shooting anim
        }
        if(!_isAiming)
        {
            _isShootingHeld = false;
        }
        
    }
    private void OnShoot(bool isPressed)
    {
        if (!_isAiming) return;
       
        _isShootingHeld = isPressed;

        if (_currentAmmo == 0 && isPressed)
        {
            GameManager.Instance.onOutOfAmmo?.Invoke(CurrentShootingMethod);
            AudioManager.Instance.PlaySFX("NoAmmoShoot");
            _isShootingHeld = false;
            return;
        }
        switch (CurrentShootingMethod)
        {
            case EShootingMethod.Auto:
                if (isPressed)
                {
                    _muzzleFlash.Play();
                    _muzzleLight.StartLooping();
                }
                else
                {
                    _muzzleFlash.Stop();
                    _muzzleLight.StopLooping();
                }
                _animator.SetBool("IsAutoShot", isPressed); // Loop anim until release
            break;

        case EShootingMethod.SingleShot:
                if (!isPressed) return;
                else if(isPressed && CurrentAmmo < 10)
                {
                    AudioManager.Instance.PlaySFX("NoAmmoShoot");
                    GameManager.Instance.onOutOfAmmo?.Invoke(CurrentShootingMethod);
                    return;
                }

                TryShootOnce("SingleShot", _singleShotCooldown, ref _lastSingleShotTime);
                break;

        case EShootingMethod.BurstShot:
                if (!isPressed) return;
                TryShootOnce("BurstShot", _burstShotCooldown, ref _lastBurstShotTime);
            break;
        }
    }

    private IEnumerator PlayMuzzleOnce()
    {
        _muzzleFlash.Play();

        yield return new WaitForSeconds(0.1f); // Flash duration
        _muzzleFlash.Stop();
    }

    private void OnChangeShootingMethod()
    {
        if (_isShootingHeld) return;
        CurrentShootingMethod =
            (EShootingMethod)(((int)CurrentShootingMethod + 1) %
            System.Enum.GetValues(typeof(EShootingMethod)).Length);

        GameManager.Instance.onChangeShootingMode?.Invoke((int)CurrentShootingMethod);
        AudioManager.Instance.PlaySFX("ChangeShootingMode");
    }

    

    private void TryShootOnce(string shootAnim, float cooldown, ref float lastShootTime)
    {
        if (Time.time - lastShootTime < cooldown)
            return;

        _animator.Play(shootAnim, 1);
        lastShootTime = Time.time;

        GameManager.Instance.onSpecialShoot?.Invoke(shootAnim,cooldown);
    }

    public void StartShoot() // Animation event
    {
        switch (CurrentShootingMethod)
        {
            case EShootingMethod.Auto:
            case EShootingMethod.BurstShot:
                if (CurrentAmmo <= 0)
                {
                    AudioManager.Instance.PlaySFX("NoAmmoShoot");
                    return;
                }
                ShootRaycast();
                break;
            case EShootingMethod.SingleShot:
                
                ShootGrenade();
                break;

            
        }

        GameManager.Instance.onAmmoChange?.Invoke(CurrentAmmo, TotalAmmoRemaining);

    }
    private void ShootRaycast()
    {
        if (CurrentShootingMethod == EShootingMethod.BurstShot)
        {
            StartCoroutine(PlayMuzzleOnce());
            _muzzleLight.PlayOnce();
        }
        if (!_hasInfinityBullet)
            CurrentAmmo -= 1;

        var direction = _shootingPoint.up;
        Ray ray = new Ray(_shootingPoint.position, direction.normalized);

        // 👇 Use layerMask: ~ignoreLayers
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance + 10, ~shootIgnoreLayers))
        {
            string ParticleEffectId = hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")
                ? "BloodImpact"
                : "BulletImpact";

            ParticleManager.Instance.Spawn(ParticleEffectId, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));

            if (hitInfo.collider.TryGetComponent<Damageable>(out Damageable damageable))
            {
                damageable.TakeDamage((int)_bulletDamage);
            }
        }
        AudioManager.Instance.PlaySFX("BulletShoot");
    }

    private void ShootGrenade()
    {
        if(!_hasInfinityGrenade)
            CurrentAmmo -= 10;
        AudioManager.Instance.PlaySFX("GrenadeShoot");
        // Instantiate grenade at shooting point
        GrenadeProjectile grenade = Instantiate(
            _grenadePrefab, // assign in Inspector
            _shootingPoint.position,
            _shootingPoint.rotation
        ).GetComponent<GrenadeProjectile>();
        grenade.knockbackForce = _grenadeKnockbackForce; // Set knockback force
        grenade.damage = _grenadeDamage; // Set grenade damage
        // Give it velocity
        if (grenade.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = _shootingPoint.up * _grenadeLaunchForce;
        }
    }



    //event called when the reload animation ends
    public void OnReloadDone()
    {
        int ammoUsed = _ammoPerCapacity - _currentAmmo;
        int ammoToReload = _totalAmmoRemaining >= ammoUsed ? ammoUsed : _totalAmmoRemaining;
        CurrentAmmo += ammoToReload;
        _totalAmmoRemaining -= ammoToReload;
        GameManager.Instance.UpdateAmmo(CurrentAmmo, TotalAmmoRemaining);

    }

    private void OnPickupInfinityBulletBuff(float duration)
    {
        if (_infinityBulletCoroutine != null)
        {
            StopCoroutine(_infinityBulletCoroutine);
        }
        _infinityBulletCoroutine = StartCoroutine(InfinityBulletRoutine(duration));
    }

    private void OnPickupInfinityGrenadeBuff(float duration)
    {
        if (_infinityGrenadeCoroutine != null)
        {
            StopCoroutine(_infinityGrenadeCoroutine);
        }
        _infinityGrenadeCoroutine = StartCoroutine(InfinityGrenadeRoutine(duration));
    }

    private IEnumerator InfinityBulletRoutine(float duration)
    {
        _hasInfinityBullet = true;
        Debug.Log($"Infinity Bullet buff active for {duration} seconds!");
        yield return new WaitForSeconds(duration);
        _hasInfinityBullet = false;
        Debug.Log("Infinity Bullet buff expired.");
        _infinityBulletCoroutine = null;
    }

    private IEnumerator InfinityGrenadeRoutine(float duration)
    {
        _hasInfinityGrenade = true;
        Debug.Log($"Infinity Grenade buff active for {duration} seconds!");
        yield return new WaitForSeconds(duration);
        _hasInfinityGrenade = false;
        Debug.Log("Infinity Grenade buff expired.");
        _infinityGrenadeCoroutine = null;
    }

    private void OnPickupGrenadeDamageBuff(int percent)
    {
        _grenadeDamage *= 1f + (percent / 100f);
    }

    private void OnPickupBulletDamageBuff(int percent)
    {
        _bulletDamage *= 1f + (percent / 100f);
    }


    private void OnPickupReloadSpeedBuff(float amount)
    {
        float reloadSpeed = _animator.GetFloat("ReloadSpeed");
        reloadSpeed = Mathf.Clamp(reloadSpeed + amount, 1, 2f); // Ensure reload speed is within a reasonable range
        _animator.SetFloat("ReloadSpeed", reloadSpeed);
    }

    private void OnPickupAmmo(int amount)
    {
        TotalAmmoRemaining += amount;
        GameManager.Instance.UpdateAmmo(CurrentAmmo, TotalAmmoRemaining);
    }
}
