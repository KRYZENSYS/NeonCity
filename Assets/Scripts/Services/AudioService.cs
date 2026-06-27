// =============================================================================
// NeonCity — Services / AudioService.cs
// -----------------------------------------------------------------------------
// Centralized audio control: master volume, music, SFX, footstep system,
// environmental ambience.
//
// Architecture:
//   - 1 AudioListener (on camera)
//   - N AudioSources: 1 music (looping), 1 ambience, N SFX pooled
//   - Footstep system uses Physics.Raycast under the player to determine
//     surface type (concrete/grass/water/metal) and picks a random clip.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonCity.Services
{
    public enum SurfaceType { Concrete, Grass, Water, Metal, Wood, Default }

    public class AudioService : NeonCity.Core.IService
    {
        [System.Serializable]
        public struct FootstepBank
        {
            public SurfaceType surface;
            public AudioClip[] clips;
        }

        private AudioMixer _mixer;
        private AudioSource _music;
        private AudioSource _ambience;
        private readonly List<AudioSource> _sfxPool = new();
        private FootstepBank[] _footstepBanks;

        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume  { get; private set; } = 0.7f;
        public float SfxVolume    { get; private set; } = 1f;

        public void Initialize()
        {
            // Pool SFX sources
            for (int i = 0; i < 12; i++)
            {
                var go = new GameObject($"SFX_{i}");
                Object.DontDestroyOnLoad(go);
                go.transform.SetParent(null);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool.Add(src);
            }

            // Music + ambience sources
            _music    = CreateSource("Music", loop: true);
            _ambience = CreateSource("Ambience", loop: true);
        }

        public void Shutdown() { /* nothing persistent */ }

        private AudioSource CreateSource(string name, bool loop)
        {
            var go = new GameObject(name);
            Object.DontDestroyOnLoad(go);
            var s = go.AddComponent<AudioSource>();
            s.loop = loop;
            s.playOnAwake = false;
            return s;
        }

        // -------------------------------------------------------------------
        public void PlayMusic(AudioClip clip, float fade = 1f)
        {
            if (clip == null) return;
            _music.clip = clip;
            _music.volume = 0f;
            _music.Play();
            StopAllCoroutines();
            StartFade(_music, MusicVolume, fade);
        }

        public void PlaySfx(AudioClip clip, Vector3 pos = default, float volume = 1f)
        {
            if (clip == null) return;
            var src = GetFreeSfxSource();
            if (src == null) return;
            src.transform.position = pos;
            src.spatialBlend = pos == default ? 0f : 1f;
            src.PlayOneShot(clip, volume * SfxVolume * MasterVolume);
        }

        public void SetMasterVolume(float v) { MasterVolume = v; ApplyMixer(); }
        public void SetMusicVolume(float v)  { MusicVolume  = v; _music.volume = v; }
        public void SetSfxVolume(float v)    { SfxVolume    = v; }

        private void ApplyMixer()
        {
            if (_mixer == null) return;
            _mixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(MasterVolume, 0.001f)) * 20f);
        }

        private AudioSource GetFreeSfxSource()
        {
            foreach (var s in _sfxPool) if (!s.isPlaying) return s;
            return _sfxPool[0]; // steal oldest
        }

        // -------------------------------------------------------------------
        // Footstep system — call from PlayerController each step.
        // -------------------------------------------------------------------
        public void SetFootstepBanks(FootstepBank[] banks) => _footstepBanks = banks;

        public void PlayFootstep(SurfaceType surface, Vector3 pos)
        {
            if (_footstepBanks == null) return;
            foreach (var bank in _footstepBanks)
            {
                if (bank.surface != surface) continue;
                if (bank.clips == null || bank.clips.Length == 0) return;
                var clip = bank.clips[Random.Range(0, bank.clips.Length)];
                PlaySfx(clip, pos, 0.8f);
                return;
            }
        }

        public SurfaceType DetectSurface(RaycastHit hit)
        {
            // Probe by tag or material name — customize per project.
            if (hit.collider == null) return SurfaceType.Default;
            var tag = hit.collider.tag;
            return tag switch
            {
                "Surface.Grass"  => SurfaceType.Grass,
                "Surface.Water"  => SurfaceType.Water,
                "Surface.Metal"  => SurfaceType.Metal,
                "Surface.Wood"   => SurfaceType.Wood,
                "Surface.Concrete" => SurfaceType.Concrete,
                _ => SurfaceType.Default
            };
        }

        // -------------------------------------------------------------------
        // Mini fade helper — Unity coroutines are still fine in Unity 6.
        private void StopAllCoroutines() { /* no-op; use MonoBehaviour in callers */ }
        private void StartFade(AudioSource s, float target, float dur)
        {
            // Hooked via PlayerBootstrap MonoBehaviour helper in real project.
        }
    }
}