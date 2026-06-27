// =============================================================================
// NeonCity — Tools / BuildScript.cs
// -----------------------------------------------------------------------------
// Static methods invoked by Unity in -batchmode from CI or local CLI.
// Build all platforms from a single command:
//   /opt/unity/Editor/Unity -batchmode -quit -executeMethod BuildScript.All
// =============================================================================

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonCity.Tools
{
    public static class BuildScript
    {
        private const string OUT = "build";

        public static void Linux64()   => Build("Linux64",   BuildTarget.StandaloneLinux64);
        public static void Windows64() => Build("Windows64", BuildTarget.StandaloneWindows64);
        public static void MacOS()     => Build("MacOS",     BuildTarget.StandaloneOSX);
        public static void Android()   => Build("Android",   BuildTarget.Android);
        public static void iOS()       => Build("iOS",       BuildTarget.iOS);
        public static void WebGL()     => Build("WebGL",     BuildTarget.WebGL);

        public static void All()
        {
            Linux64(); Windows64(); Android(); WebGL();
        }

        // -------------------------------------------------------------------
        private static void Build(string label, BuildTarget target)
        {
            var dir = Path.Combine(OUT, label);
            Directory.CreateDirectory(dir);
            var scenes = new[] {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/City.unity",
            };
            var opts = new BuildPlayerOptions {
                scenes           = scenes,
                locationPathName = Path.Combine(dir, "NeonCity"),
                target           = target,
                options          = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[Build] {label} -> {report.summary.result}");
        }
    }
}
#endif