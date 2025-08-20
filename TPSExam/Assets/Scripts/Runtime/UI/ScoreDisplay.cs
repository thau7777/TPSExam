using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float scaleUpSize = 1.2f; // 20% bigger
    [SerializeField] private float scaleSpeed = 8f;    // How fast it scales

    private int currentScore = 0;
    private Coroutine animCoroutine;
    private Vector3 originalScale;

    [SerializeField]
    private GameObject _gameEndContainer;
    [SerializeField]
    private TextMeshProUGUI _gameEndText;
    [SerializeField]
    private TextMeshProUGUI _gameEndScoreText;

    private void Awake()
    {
        if (!scoreText) scoreText = GetComponent<TextMeshProUGUI>();
        scoreText.text = currentScore.ToString();
        originalScale = scoreText.rectTransform.localScale;
    }
    private void OnEnable()
    {
        GameManager.Instance.onEnemyDeath += UpdateScore;
        GameManager.Instance.onGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameManager.Instance.onEnemyDeath -= UpdateScore;
        GameManager.Instance.onGameOver -= OnGameOver;
    }

    private void OnGameOver(bool isWin)
    {
        _gameEndContainer.SetActive(true);
        _gameEndText.text = isWin ? "You Win!" : "Game Over!";
        _gameEndScoreText.text = $"Score: {currentScore}";
    }


    private void UpdateScore(int value)
    {
        SetScore(currentScore + value);
    }
    public void SetScore(int newScore)
    {
        currentScore = newScore; // Update immediately so the next add stacks properly

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        int startValue = int.Parse(scoreText.text);
        animCoroutine = StartCoroutine(AnimateScoreChange(startValue, newScore));
    }


    private IEnumerator AnimateScoreChange(int startScore, int targetScore)
    {
        int difference = Mathf.Abs(targetScore - startScore);

        // Step size based on difference
        int step;
        if (difference >= 1000) step = 100;
        else if (difference >= 500) step = 50;
        else if (difference >= 200) step = 10;
        else step = 1;

        float timePerStep = Mathf.Clamp(0.05f / (difference * 0.02f + 1f), 0.005f, 0.05f);

        int displayedScore = startScore;
        Vector3 targetScale = originalScale * scaleUpSize;

        while (displayedScore != targetScore)
        {
            displayedScore += (targetScore > startScore ? step : -step);

            // Clamp so we don’t overshoot
            if ((targetScore > startScore && displayedScore > targetScore) ||
                (targetScore < startScore && displayedScore < targetScore))
            {
                displayedScore = targetScore;
            }

            scoreText.text = displayedScore.ToString();

            // Scale pop
            scoreText.rectTransform.localScale = Vector3.Lerp(
                scoreText.rectTransform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );

            yield return new WaitForSeconds(timePerStep);
        }

        // Smoothly back to normal
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            scoreText.rectTransform.localScale = Vector3.Lerp(scoreText.rectTransform.localScale, originalScale, t);
            yield return null;
        }

        currentScore = targetScore;
        scoreText.rectTransform.localScale = originalScale;
    }


}
