using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    [Header("Panels (only one visible at a time)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Play")]
    [SerializeField] private string gameSceneName = "MainGame";

    private void Awake()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(storyPanel, false);
        SetPanelActive(howToPlayPanel, false);
        SetPanelActive(settingsPanel, false);
    }

    public void ShowStory()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(storyPanel, true);
        SetPanelActive(howToPlayPanel, false);
        SetPanelActive(settingsPanel, false);
    }

    public void ShowHowToPlay()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(storyPanel, false);
        SetPanelActive(howToPlayPanel, true);
        SetPanelActive(settingsPanel, false);
    }

    public void ShowSettings()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(storyPanel, false);
        SetPanelActive(howToPlayPanel, false);
        SetPanelActive(settingsPanel, true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
