using UnityEngine;
using UnityEngine.UI;

public class MenuSettingsController : MonoBehaviour
{
    private const string KeyMusicOn = "WingsOfAsh_MusicOn";
    private const string KeySfxOn = "WingsOfAsh_SfxOn";
    private const string KeyMusicVol = "WingsOfAsh_MusicVol";
    private const string KeySfxVol = "WingsOfAsh_SfxVol";

    [Header("Music")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicSlider;

    [Header("SFX")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (musicToggle != null)
        {
            musicToggle.isOn = PlayerPrefs.GetInt(KeyMusicOn, 1) == 1;
            musicToggle.onValueChanged.AddListener(OnMusicToggle);
        }

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat(KeyMusicVol, 0.75f);
            musicSlider.onValueChanged.AddListener(OnMusicVolume);
        }

        if (sfxToggle != null)
        {
            sfxToggle.isOn = PlayerPrefs.GetInt(KeySfxOn, 1) == 1;
            sfxToggle.onValueChanged.AddListener(OnSfxToggle);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat(KeySfxVol, 0.9f);
            sfxSlider.onValueChanged.AddListener(OnSfxVolume);
        }
    }

    private void OnMusicToggle(bool on)
    {
        PlayerPrefs.SetInt(KeyMusicOn, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnMusicVolume(float v)
    {
        PlayerPrefs.SetFloat(KeyMusicVol, v);
        PlayerPrefs.Save();
    }

    private void OnSfxToggle(bool on)
    {
        PlayerPrefs.SetInt(KeySfxOn, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnSfxVolume(float v)
    {
        PlayerPrefs.SetFloat(KeySfxVol, v);
        PlayerPrefs.Save();
    }
}
