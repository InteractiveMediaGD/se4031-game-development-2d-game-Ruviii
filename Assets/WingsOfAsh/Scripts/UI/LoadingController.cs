using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [Tooltip("Must match the .unity file name (no extension). E.g. file 01_MainMenu.unity → type 01_MainMenu")]
    [SerializeField] private string nextSceneName = "01_MainMenu";
    [SerializeField] private float minDisplaySeconds = 1.5f;

    private void Start()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        StartCoroutine(LoadNextSceneRoutine());
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        if (loadOp == null)
        {
            Debug.LogError(
                $"LoadingController: could not start load for '{nextSceneName}'. " +
                "Check: (1) scene file name matches this string exactly, (2) scene is ticked in File → Build Profiles / Build Settings.",
                this);
            yield break;
        }

        loadOp.allowSceneActivation = false;

        float elapsed = 0f;
        while (elapsed < minDisplaySeconds || loadOp.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            float visual = Mathf.Clamp01(Mathf.Max(loadOp.progress / 0.9f, elapsed / minDisplaySeconds));
            if (progressBar != null)
            {
                progressBar.value = visual;
            }

            yield return null;
        }

        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        yield return new WaitForSecondsRealtime(0.15f);
        loadOp.allowSceneActivation = true;
    }
}
