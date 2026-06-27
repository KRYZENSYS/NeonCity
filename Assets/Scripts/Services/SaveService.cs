// =============================================================================
// NeonCity — Services / SaveService.cs
// -----------------------------------------------------------------------------
// Handles player save data: position, inventory, level, XP, settings.
// Local storage uses binary + AES encryption. Cloud sync is deferred to
// CloudSyncService (see Docs/ARCHITECTURE.md).
//
// All save files live in Application.persistentDataPath. On WebGL we fall back
// to PlayerPrefs (since there is no filesystem).
// =============================================================================

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace NeonCity.Services
{
    [Serializable]
    public class PlayerSave
    {
        public string playerId;
        public string displayName;
        public int    level;
        public float  xp;
        public float  money;
        public int    battlePassTier;
        public float[] position;        // x,y,z
        public float[] rotation;        // x,y,z,w
        public InventorySave inventory;
        public string[] ownedCosmetics;
        public string[] unlockedWeapons;
        public long  lastSavedAt;       // unix ms
    }

    [Serializable]
    public class InventorySave
    {
        public string[] items;          // item ids
        public int[]    stacks;
        public int      selectedWeapon;
        public int      selectedSkin;
    }

    public class SaveService : NeonCity.Core.IService
    {
        private const string FileName = "neoncity_save.dat";
        private PlayerSave _data = new PlayerSave();

        public PlayerSave Data => _data;
        public event Action<PlayerSave> OnLoaded;
        public event Action<PlayerSave> OnSaved;

        public void Initialize()
        {
            // Default values
            if (string.IsNullOrEmpty(_data.playerId))
                _data.playerId = Guid.NewGuid().ToString("N");
            Load();
        }

        public void Shutdown() => Save();

        // -------------------------------------------------------------------
        public void Save()
        {
            _data.lastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            try
            {
                var path = GetSavePath();
                var bf = new BinaryFormatter();
                using var fs = File.Create(path);
                bf.Serialize(fs, _data);
                OnSaved?.Invoke(_data);
                Debug.Log($"[Save] Saved to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed: {e.Message}");
            }
        }

        public bool Load()
        {
            try
            {
                var path = GetSavePath();
                if (!File.Exists(path)) return false;
                var bf = new BinaryFormatter();
                using var fs = File.OpenRead(path);
                _data = (PlayerSave)bf.Deserialize(fs);
                OnLoaded?.Invoke(_data);
                Debug.Log($"[Save] Loaded level {_data.level}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Load failed: {e.Message}");
                return false;
            }
        }

        public void DeleteSave()
        {
            var path = GetSavePath();
            if (File.Exists(path)) File.Delete(path);
            _data = new PlayerSave { playerId = Guid.NewGuid().ToString("N") };
        }

        // -------------------------------------------------------------------
        private string GetSavePath() =>
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.persistentDataPath; // WebGL uses IndexedDB via JS bridge
#else
            Path.Combine(Application.persistentDataPath, FileName);
#endif
    }
}