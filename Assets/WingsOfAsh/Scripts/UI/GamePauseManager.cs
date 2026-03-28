using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Player state (optional)")]
    [SerializeField] private FlameHealth playerHealth;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "01_MainMenu";
    [SerializeField] private string gameplaySceneName = "MainGame";

    private void Awake()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        IsPaused = false;
    }

    private void OnDestroy()
    {
        if (IsPaused)
        {
            Time.timeScale = 1f;
            IsPaused = false;
        }
    }

    public void OpenPause()
    {
        if (pausePanel == null)
        {
            return;
        }

        ResolveHealth();
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        pausePanel.SetActive(true);
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        IsPaused = false;
        ResolveHealth();
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public void GoToMainMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void ResolveHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        playerHealth = FindFirstObjectByType<FlameHealth>();
    }
}
