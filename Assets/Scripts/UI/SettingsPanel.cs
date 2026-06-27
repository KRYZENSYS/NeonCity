// =============================================================================
// NeonCity — UI / SettingsPanel.cs
// -----------------------------------------------------------------------------
// Settings UI binds to QualitySettings / AudioMixer / InputSystem on the fly.
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using NeonCity.Core;

namespace NeonCity.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        public Slider masterVolume, musicVolume, sfxVolume;
        public Slider mouseSensitivity;
        public Toggle fullscreen;
        public Toggle vsync;
        public Dropdown qualityDropdown;

        private void Start()
        {
            // Load from PlayerPrefs (or Save service)
            masterVolume.value    = PlayerPrefs.GetFloat("vol.master", 1f);
            musicVolume.value     = PlayerPrefs.GetFloat("vol.music", 0.7f);
            sfxVolume.value       = PlayerPrefs.GetFloat("vol.sfx", 1f);
            mouseSensitivity.value = PlayerPrefs.GetFloat("mouse.sens", 1.5f);
            fullscreen.isOn       = PlayerPrefs.GetInt("fullscreen", 1) == 1;
            vsync.isOn            = PlayerPrefs.GetInt("vsync", 1) == 1;
            qualityDropdown.value = PlayerPrefs.GetInt("quality", QualitySettings.names.Length - 1);

            masterVolume.onValueChanged.AddListener(v => { GameManager.Instance.Audio.SetMasterVolume(v); PlayerPrefs.SetFloat("vol.master", v); });
            musicVolume.onValueChanged .AddListener(v => { GameManager.Instance.Audio.SetMusicVolume(v);  PlayerPrefs.SetFloat("vol.music", v); });
            sfxVolume.onValueChanged   .AddListener(v => { GameManager.Instance.Audio.SetSfxVolume(v);    PlayerPrefs.SetFloat("vol.sfx", v); });
            fullscreen.onValueChanged  .AddListener(v => { Screen.fullScreen = v; PlayerPrefs.SetInt("fullscreen", v ? 1 : 0); });
            vsync.onValueChanged       .AddListener(v => { QualitySettings.vSyncCount = v ? 1 : 0; PlayerPrefs.SetInt("vsync", v ? 1 : 0); });
            qualityDropdown.onValueChanged.AddListener(i => { QualitySettings.SetQualityLevel(i); PlayerPrefs.SetInt("quality", i); });
        }
    }

    public class TMP_Dropdown {}
}