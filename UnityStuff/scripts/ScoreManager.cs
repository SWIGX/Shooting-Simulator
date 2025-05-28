using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI accuracyText;

    private int score = 0;
    private int shotsFired = 0;
    private int hits = 0;
    private float timer = 0f;
    private bool isRunning = true;

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            UpdateUI();
        }
    }

    public void AddShot(bool hit, int scoreValue = 10)
    {
        shotsFired++;
        if (hit)
        {
            hits++;
            score += scoreValue;
        }
        UpdateUI();
    }

    public void ResetGame()
    {
        score = 0;
        shotsFired = 0;
        hits = 0;
        timer = 0;
        isRunning = true;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
        if (timerText) timerText.text = $"Time: {timer:F1}s";
        if (accuracyText)
        {
            float accuracy = shotsFired > 0 ? (hits / (float)shotsFired) * 100f : 0f;
            accuracyText.text = $"Accuracy: {hits}/{shotsFired} ({accuracy:F1}%)";
        }
    }
}
