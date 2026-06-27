// =============================================================================
// NeonCity — Core / GameManager.cs
// -----------------------------------------------------------------------------
// The SINGLETON BRAIN of the game. Survives scene loads (DontDestroyOnLoad).
// Owns every Service and exposes them via simple accessors.
//
// What lives here:
//   - Service registration (Save, Audio, Input, Network, UI)
//   - Game state machine (Boot → Menu → Loading → Playing → Paused)
//   - Global time-scale and pause handling
//   - Quick-exit handling
//
// Why a single manager? Easier to reason about lifecycle in a Unity project
// where everything can be a MonoBehaviour. We keep it lightweight — services
// themselves do the heavy lifting.
//
// Author: NeonCity Team
// Unity 6 (6000.0.x)
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using NeonCity.Services;
using NeonCity.Networking;

namespace NeonCity.Core
{
    /// <summary>High-level game state.</summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>Application-level manager. One per process.</summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        // ---- Singleton ----------------------------------------------------
        public static GameManager Instance { get; private set; }

        // ---- Services -----------------------------------------------------
        private readonly Dictionary<Type, IService> _services = new();
        public SaveService Save { get; private set; }
        public AudioService Audio { get; private set; }
        public InputService Input { get; private set; }
        public NetworkService Network { get; private set; }
        public UIService UI { get; private set; }

        // ---- State --------------------------------------------------------
        public GameState State { get; private set; } = GameState.Boot;
        public event Action<GameState, GameState> OnStateChanged;

        // -------------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount  = 1;

            RegisterServices();
        }

        private void RegisterServices()
        {
            // Order matters: Save first (others may persist state).
            Save   = Register(new SaveService());
            Audio  = Register(new AudioService());
            Input  = Register(new InputService());
            Network = Register(new NetworkService());
            UI     = Register(new UIService());

            foreach (var svc in _services.Values) svc.Initialize();
        }

        private T Register<T>(T svc) where T : IService
        {
            _services[typeof(T)] = svc;
            return svc;
        }

        public T Get<T>() where T : IService => (T)_services[typeof(T)];

        // -------------------------------------------------------------------
        public void ChangeState(GameState next)
        {
            if (State == next) return;
            var prev = State;
            State = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;
            OnStateChanged?.Invoke(prev, next);
            Debug.Log($"[GameManager] State {prev} → {next}");
        }

        // -------------------------------------------------------------------
        private void Update()
        {
            if (Input == null) return;

            // Quick pause toggle
            if (Input.PausePressed())
            {
                if (State == GameState.Playing) ChangeState(GameState.Paused);
                else if (State == GameState.Paused) ChangeState(GameState.Playing);
            }

            // Quick quit
            if (Input.QuitPressed()) Application.Quit();
        }

        private void OnDestroy()
        {
            foreach (var svc in _services.Values) svc.Shutdown();
            if (Instance == this) Instance = null;
        }
    }
}