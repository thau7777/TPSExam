using Unity.Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [SerializeField] private CinemachineBasicMultiChannelPerlin _shaker;

    public float maxShakeAmplitude = 0.5f;   // Max shake when moving fast
    public float maxShakeFrequency = 2f;     // Max shake speed when moving fast
    public float shakeSpeedThreshold = 5f;   // Speed where shake is at max
    public float shakeLerpSpeed = 5f;        // How fast the shake transitions

    private bool _canShake = false;
    private float _currentSpeed;
    private float _targetSpeed;

    private void Update()
    {
        if (_shaker == null) return;

        if (_canShake)
        {
            // Lerp toward movement speed
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * shakeLerpSpeed);
        }
        else
        {
            // Smoothly fade out when disabled
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * shakeLerpSpeed);
        }

        // Normalize speed into 0–1 range
        float normalizedSpeed = Mathf.InverseLerp(0f, shakeSpeedThreshold, _currentSpeed);

        // Map speed to amplitude & frequency
        float shakeAmount = normalizedSpeed * maxShakeAmplitude;
        float shakeFrequency = normalizedSpeed * maxShakeFrequency;

        // Apply to camera
        _shaker.AmplitudeGain = shakeAmount;
        _shaker.FrequencyGain = shakeFrequency;
    }

    /// <summary>
    /// Called via UnityEvent with canShake and newSpeed
    /// </summary>
    public void SetCanShake(bool value, float newSpeed)
    {
        _canShake = value;
        _targetSpeed = newSpeed;
    }
}
