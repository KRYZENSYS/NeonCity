// =============================================================================
// NeonCity — Networking / NetworkService.cs
// -----------------------------------------------------------------------------
// Wraps Netcode for GameObjects: host/join/leave server, lobby creation,
// matchmaking. Exposes a simple API for gameplay code to use without
// scattering NGO calls across the codebase.
//
// This service talks to BOTH Unity NGO (peer-to-peer transport) AND the
// authoritative Node.js backend for matchmaking, accounts, leaderboards.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace NeonCity.Networking
{
    /// <summary>Netcode base we inherit from to keep API surface minimal.</summary>
    public class NetworkBehaviour : Unity.Netcode.NetworkBehaviour { }

    public enum NetworkRole { None, Host, Client, Server }

    public class NetworkService : NeonCity.Core.IService
    {
        public NetworkRole Role { get; private set; } = NetworkRole.None;
        public string      PlayerId { get; private set; }
        public string      DisplayName { get; set; } = "Player";
        public event Action<ulong> OnClientConnected;
        public event Action OnHostStarted;
        public event Action<string> OnError;

        public void Initialize() { /* set up hooks on NetworkManager if present */ }
        public void Shutdown()   => Leave();

        // -------------------------------------------------------------------
        public async void HostGame(string roomName, int maxPlayers = 16)
        {
            try
            {
                var nm = NetworkManager.Singleton;
                if (nm == null) { OnError?.Invoke("NetworkManager missing"); return; }

                var transport = nm.GetComponent<UnityTransport>();
                transport.SetConnectionData("0.0.0.0", 7777);

                nm.OnClientConnectedCallback += id => OnClientConnected?.Invoke(id);
                if (nm.StartHost())
                {
                    Role = NetworkRole.Host;
                    OnHostStarted?.Invoke();
                    Debug.Log($"[Network] Hosting room '{roomName}' for {maxPlayers}");
                    await BackendApi.CreateRoom(roomName, maxPlayers, PlayerId);
                }
            }
            catch (Exception e) { OnError?.Invoke(e.Message); }
        }

        public async void JoinGame(string ip, ushort port = 7777)
        {
            try
            {
                var nm = NetworkManager.Singleton;
                if (nm == null) { OnError?.Invoke("NetworkManager missing"); return; }

                var transport = nm.GetComponent<UnityTransport>();
                transport.SetConnectionData(ip, port);

                if (nm.StartClient())
                {
                    Role = NetworkRole.Client;
                    Debug.Log($"[Network] Joining {ip}:{port}");
                }
            }
            catch (Exception e) { OnError?.Invoke(e.Message); }
        }

        public void Leave()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.Shutdown();
            Role = NetworkRole.None;
        }

        // -------------------------------------------------------------------
        public void SetPlayerIdentity(string id, string name)
        {
            PlayerId = id;
            DisplayName = name;
        }
    }

    /// <summary>Thin HTTP client to talk to our Node.js backend.</summary>
    public static class BackendApi
    {
        public static string BaseUrl = "https://api.neoncity.game";

        public static async Task CreateRoom(string name, int maxPlayers, string hostId)
        {
            var body = $"{{\"name\":\"{name}\",\"maxPlayers\":{maxPlayers},\"hostId\":\"{hostId}\"}}";
            var resp = await HttpPost("/rooms", body);
            Debug.Log($"[Backend] Room created: {resp}");
        }

        private static async Task<string> HttpPost(string path, string body)
        {
            using var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            var resp = await client.PostAsync(path,
                new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            return await resp.Content.ReadAsStringAsync();
        }
    }
}