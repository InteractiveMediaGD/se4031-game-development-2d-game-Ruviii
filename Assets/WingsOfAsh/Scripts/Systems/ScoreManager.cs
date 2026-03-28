using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text scoreText;

    public int Score { get; private set; }

    private void Start()
    {
        ResetScore();
    }

    public void AddGatePoint()
    {
        AddScore(1);
    }

    public void AddEnemyProjectileKillPoints()
    {
        AddScore(5);
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    private void AddScore(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Score}";
        }
    }
}
