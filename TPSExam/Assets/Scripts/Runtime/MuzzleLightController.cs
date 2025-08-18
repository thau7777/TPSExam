using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class MuzzleLightController : MonoBehaviour
{
    public float flickerInterval = 0.05f; // Time between on/off
    public float onceDuration = 0.1f;     // How long to flicker in PlayOnce

    private Light _light;
    private Coroutine _flickerRoutine;

    void Awake()
    {
        _light = GetComponent<Light>();
        _light.enabled = false; // start off
    }

    // --- Mode 1: Looping ---
    public void StartLooping()
    {
        StopFlicker();
        _flickerRoutine = StartCoroutine(FlickerLoop());
    }

    public void StopLooping()
    {
        StopFlicker();
        _light.enabled = false;
    }

    // --- Mode 2: Play Once ---
    public void PlayOnce()
    {
        StopFlicker();
        _flickerRoutine = StartCoroutine(FlickerOnce());
    }

    // --- Internal ---
    private void StopFlicker()
    {
        if (_flickerRoutine != null)
        {
            StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
        }
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            _light.enabled = !_light.enabled;
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    private IEnumerator FlickerOnce()
    {
        float elapsed = 0f;
        while (elapsed < onceDuration)
        {
            _light.enabled = !_light.enabled;
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }
        _light.enabled = false;
    }
}
