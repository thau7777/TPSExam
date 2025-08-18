using System;
using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _currentAmmoText;
    [SerializeField]
    private TextMeshProUGUI _ammoRemainsText;

    private void OnEnable()
    {
        GameManager.Instance.onAmmoChange += UpdateAmmoDisplay;
    }
    private void OnDisable()
    {
        GameManager.Instance.onAmmoChange -= UpdateAmmoDisplay;
    }   
    private void UpdateAmmoDisplay(int currentAmmo, int ammoRemains)
    {
        _currentAmmoText.text = currentAmmo.ToString();
        _ammoRemainsText.text = ammoRemains.ToString();

    }
}
