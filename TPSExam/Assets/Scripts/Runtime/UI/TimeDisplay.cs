using System;
using TMPro;
using UnityEngine;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private int _maxMinutes = 20; // stop at 20:00
    private TextMeshProUGUI _timeText;

    private bool _stopped = false;

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

    public void TestWin()
    {
        UpdateTimeDisplay(20, 0);
    }
    private void UpdateTimeDisplay(int minute, int second)
    {
        if (_stopped)
            return;

        _timeText.text = $"{minute:D2}:{second:D2}";

        if (minute >= _maxMinutes)
        {
            _stopped = true;

            // Unsubscribe to stop updating
            GameManager.Instance.onSecondIncrease -= UpdateTimeDisplay;

            // Force display to exactly 20:00
            _timeText.text = $"{_maxMinutes:D2}:00";

            // Invoke event so others can react
            GameManager.Instance.onGameOver?.Invoke(true); // Assuming this means player won
        }
    }
}
