using System;
using TMPro;
using UnityEngine;

public class TimeDisplay : MonoBehaviour
{
    private TextMeshProUGUI _timeText; // 00:00 format

    private void Awake()
    {
        _timeText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameManager.Instance.onSecondIncrease += UpdateTimeDisplay;
    }

    private void OnDisable()
    {
        GameManager.Instance.onSecondIncrease -= UpdateTimeDisplay;
    }

    private void UpdateTimeDisplay(int minute , int second)
    {
        _timeText.text = $"{minute:D2}:{second:D2}";
    }
}
