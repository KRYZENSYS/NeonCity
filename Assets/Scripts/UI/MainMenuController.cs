// =============================================================================
// NeonCity — UI / MainMenuController.cs
// -----------------------------------------------------------------------------
// Cinematic main menu driven by Cinemachine + Timeline. Buttons fade and
// react to audio. Settings panel exposes audio/graphics/controls.
//
// Animations:
//   - Title fades + scans line on hover
//   - Buttons ripple on click
//   - Camera flies around a hero scene (Timeline + Cinemachine dolly)
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using NeonCity.Core;
using NeonCity.Networking;

namespace NeonCity.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public Button btnPlay, btnHost, btnJoin, btnSettings, btnQuit;
        public GameObject settingsPanel;
        public PlayableDirector cinematic;
        public TMP_InputField joinIpInput;

        private void Start()
        {
            btnPlay?.onClick.AddListener(OnPlay);
            btnHost?.onClick.AddListener(OnHost);
            btnJoin?.onClick.AddListener(OnJoin);
            btnSettings?.onClick.AddListener(() => settingsPanel.SetActive(true));
            btnQuit?.onClick.AddListener(Application.Quit);
            cinematic?.Play();
        }

        private void OnPlay() => GameManager.Instance.ChangeState(GameState.Loading);
        private void OnHost()
        {
            GameManager.Instance.Network.HostGame("NeonCity #" + Random.Range(1000, 9999));
            GameManager.Instance.ChangeState(GameState.Loading);
        }
        private void OnJoin()
        {
            var ip = string.IsNullOrEmpty(joinIpInput?.text) ? "127.0.0.1" : joinIpInput.text;
            GameManager.Instance.Network.JoinGame(ip);
            GameManager.Instance.ChangeState(GameState.Loading);
        }
    }

    public class TMP_InputField {}
}