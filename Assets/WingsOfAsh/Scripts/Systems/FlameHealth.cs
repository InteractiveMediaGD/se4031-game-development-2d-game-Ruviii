using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlameHealth : MonoBehaviour
{
    private const string HighScoreKey = "WingsOfAsh_HighScore";

    [Header("Health Values")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int obstacleDamage = 10;
    [SerializeField] private int enemyDamage = 20;
    [SerializeField] private float damageCooldown = 0.35f;
    [SerializeField] private bool obstacleInstantKill = true;
    [SerializeField] private float respawnInvulnerabilitySeconds = 0.8f;

    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text respawnCountdownText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text gameOverHighScoreText;
    [SerializeField] private string mainMenuSceneName = "01_MainMenu";

    [Header("Death overlay (optional)")]
    [Tooltip("Full-screen UI Image (e.g. red_background sprite). Shown while dead / respawn countdown / game over.")]
    [SerializeField] private Image deathRedOverlay;
    [SerializeField] [Range(0f, 1f)] private float deathOverlayAlpha = 0.35f;

    [Header("Rage Flame Mode (optional)")]
    [SerializeField] private RageFlameMode rageFlameMode;
    [SerializeField] private float damageShakeDuration = 0.18f;
    [SerializeField] private float damageShakeMagnitude = 0.12f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public bool IsControlLocked { get; private set; }
    public float NormalizedHealth => maxHealth <= 0 ? 0f : (float)CurrentHealth / maxHealth;

    private float nextDamageTime;
    private int currentLives;
    private bool gameOver;
    private Vector3 respawnPosition;
    private Rigidbody2D rb;
    private ScoreManager scoreManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPosition = transform.position;
        currentLives = Mathf.Max(1, maxLives);
        scoreManager = FindFirstObjectByType<ScoreManager>();

        if (rageFlameMode == null)
        {
            rageFlameMode = FindFirstObjectByType<RageFlameMode>();
        }

        ResetAllState();
        UpdateLivesUI();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(false);
        }

        SetDeathOverlay(false);
        EnsureHealthSliderHandlePreserveAspect();
    }

    /// <summary>
    /// Slider drives handle anchors at runtime; the coin Image should keep aspect (Inspector often reverts).
    /// </summary>
    private void EnsureHealthSliderHandlePreserveAspect()
    {
        if (healthBar == null || healthBar.handleRect == null)
        {
            return;
        }

        Image img = healthBar.handleRect.GetComponent<Image>();
        if (img != null)
        {
            img.preserveAspect = true;
        }
    }

    private void LateUpdate()
    {
        if (rageFlameMode == null)
        {
            rageFlameMode = FindFirstObjectByType<RageFlameMode>();
        }

        if (!IsDead && rageFlameMode != null)
        {
            rageFlameMode.Refresh(CurrentHealth, maxHealth);
        }
    }

    private void Update() {}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead || IsControlLocked)
        {
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            if (obstacleInstantKill)
            {
                TakeDamage(CurrentHealth);
            }
            else
            {
                TakeDamage(obstacleDamage);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(enemyDamage);
        }
    }

    public void TakeDamage(int amount)
    {
        if (Time.time < nextDamageTime || IsDead || IsControlLocked || gameOver)
        {
            return;
        }

        nextDamageTime = Time.time + damageCooldown;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        UpdateUI();

        CameraShake.Instance?.Shake(damageShakeDuration, damageShakeMagnitude);

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || IsControlLocked || gameOver)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        UpdateUI();
    }

    public void ResetHealth()
    {
        ResetAllState();
        UpdateLivesUI();
    }

    private void HandleDeath()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        IsControlLocked = true;
        SetDeathOverlay(true);
        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            gameOver = true;
            Time.timeScale = 0f;
            ShowGameOverPanel();

            return;
        }

        StartCoroutine(RespawnCountdownRoutine());
    }

    private IEnumerator RespawnCountdownRoutine()
    {
        Time.timeScale = 0f;

        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(true);
            for (int i = 3; i >= 1; i--)
            {
                respawnCountdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }

            respawnCountdownText.gameObject.SetActive(false);
        }

        SetDeathOverlay(false);
        RespawnPlayer();
        CurrentHealth = maxHealth;
        nextDamageTime = Time.time + respawnInvulnerabilitySeconds;
        IsDead = false;
        IsControlLocked = false;
        Time.timeScale = 1f;
        UpdateUI();
    }

    private void RespawnPlayer()
    {
        transform.position = respawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnGameOverRestartClicked()
    {
        RestartLevel();
    }

    public void OnGameOverMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void ResetAllState()
    {
        gameOver = false;
        IsDead = false;
        IsControlLocked = false;
        currentLives = Mathf.Max(1, maxLives);
        CurrentHealth = maxHealth;
        nextDamageTime = 0f;
        Time.timeScale = 1f;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(false);
        }

        SetDeathOverlay(false);
        UpdateUI();
    }

    private void SetDeathOverlay(bool visible)
    {
        if (deathRedOverlay == null)
        {
            return;
        }

        deathRedOverlay.gameObject.SetActive(visible);
        if (visible)
        {
            Color c = deathRedOverlay.color;
            c.a = deathOverlayAlpha;
            deathRedOverlay.color = c;
        }
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"Health : {CurrentHealth}/{maxHealth}";
        }

        rageFlameMode?.Refresh(CurrentHealth, maxHealth);
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
    }

    private void ShowGameOverPanel()
    {
        int currentScore = scoreManager != null ? scoreManager.Score : 0;
        int highScore = Mathf.Max(PlayerPrefs.GetInt(HighScoreKey, 0), currentScore);
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"Score: {currentScore}";
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = $"High Score: {highScore}";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
