// =============================================================================
// NeonCity — Networking / BackendConfig.cs
// -----------------------------------------------------------------------------
// Single source of truth for backend URLs. Edit once, compile everywhere.
// Auto-detects platform: WebGL uses HTTPS REST, others use Socket.IO via WSS.
// =============================================================================

namespace NeonCity.Networking
{
    public static class BackendConfig
    {
        // Production endpoints (deployed on Render.com)
        public const string PRODUCTION_API  = "https://neoncity-server.onrender.com";
        public const string PRODUCTION_WS   = "wss://neoncity-server.onrender.com";

        // Staging
        public const string STAGING_API     = "https://neoncity-server-staging.onrender.com";
        public const string STAGING_WS      = "wss://neoncity-server-staging.onrender.com";

        // Local development
        public const string LOCAL_API       = "http://localhost:4000";
        public const string LOCAL_WS        = "ws://localhost:4000";

#if UNITY_EDITOR
        public static string CurrentApi => LOCAL_API;
        public static string CurrentWs  => LOCAL_WS;
#elif DEVELOPMENT_BUILD
        public static string CurrentApi => STAGING_API;
        public static string CurrentWs  => STAGING_WS;
#else
        public static string CurrentApi => PRODUCTION_API;
        public static string CurrentWs  => PRODUCTION_WS;
#endif
    }
}