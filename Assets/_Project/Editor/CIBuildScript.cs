#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Headless / CI build entrypoints.
    ///
    /// Designed to be safe to invoke from <c>-batchmode</c> Unity, including
    /// the very first run on a fresh checkout where TMP Essentials have not
    /// yet been imported. The previous implementation triggered TMP's
    /// auto-importer GUI dialog (<c>TMP_PackageResourceImporterWindow</c>)
    /// during the missing-asset probe, which throws <c>NullReferenceException</c>
    /// in batchmode and aborts the build.
    ///
    /// Used by GameCI Action <c>game-ci/unity-builder</c> when CI is gated
    /// behind a valid <c>UNITY_LICENSE</c> repo secret.
    /// </summary>
    public static class CIBuildScript
    {
        const string TmpEssentialsAssetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        /// <summary>True if TMP Essentials have been imported into this
        /// project. Probed by file presence so we never touch
        /// <c>TMP_Settings.instance</c> (which fires the auto-importer GUI).
        /// </summary>
        public static bool TmpEssentialsImported =>
            File.Exists(TmpEssentialsAssetPath);

        public static void ImportTMPEssentials()
        {
            if (TmpEssentialsImported)
            {
                Debug.Log("[CIBuild] TMP Essentials already imported — skipping.");
                return;
            }
            // ImportResources(essentials, examples, extras). We only need
            // essentials for HUD / TMP labels at runtime.
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CIBuild] TMP Essentials imported.");
        }

        /// <summary>Ensure the MainScene exists on disk and is registered in
        /// EditorBuildSettings.scenes. The repo intentionally does not commit
        /// MainScene.unity (it is regenerated deterministically by
        /// <c>BootstrapWizard.Bootstrap()</c>), so on a fresh CI checkout we
        /// must run the bootstrap before <c>BuildPlayer</c> — otherwise the
        /// build aborts with "Cannot build untitled scene".</summary>
        static void EnsureMainSceneBootstrapped()
        {
            const string mainScenePath = "Assets/Scenes/MainScene.unity";
            if (File.Exists(mainScenePath))
            {
                Debug.Log("[CIBuild] MainScene already present — skipping bootstrap.");
                return;
            }
            Debug.Log("[CIBuild] MainScene not found — running BootstrapWizard.Bootstrap().");
            BootstrapWizard.Bootstrap();
            // BootstrapWizard.Bootstrap() wraps everything in try/catch and
            // logs exceptions via Debug.LogException, but returns normally.
            // Verify the scene was actually written so the build aborts with
            // a clear root-cause error instead of a confusing "missing scene"
            // failure in BuildPipeline.BuildPlayer downstream.
            if (!File.Exists(mainScenePath))
            {
                Debug.LogError("[CIBuild] BootstrapWizard.Bootstrap() finished but MainScene was NOT created. See earlier exceptions in log. Aborting.");
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAndroid()
        {
            if (!TmpEssentialsImported) ImportTMPEssentials();
            EnsureMainSceneBootstrapped();

            const string outDir = "build/Android";
            const string outName = "WildernessCultivation";
            Directory.CreateDirectory(outDir);

            var scenes = new[] { "Assets/Scenes/MainScene.unity" };
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(outDir, outName + ".apk"),
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.productName = "Wilderness Cultivation Demo";
            PlayerSettings.companyName = "Devin Demo";
            PlayerSettings.applicationIdentifier = "com.devindemo.wildernesscultivation";

            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            Debug.Log($"[CIBuild] Android Result={summary.result} Size={summary.totalSize} Errors={summary.totalErrors} Warnings={summary.totalWarnings}");
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildLinuxMono()
        {
            // Idempotent: probe for TMP Essentials by file presence (not via
            // TMP_Settings.defaultFontAsset, which calls TMP_Settings.instance
            // and triggers an Editor GUI window that NullRefs in batchmode).
            if (!TmpEssentialsImported)
            {
                ImportTMPEssentials();
            }
            EnsureMainSceneBootstrapped();

            const string outDir = "Builds/Linux64";
            const string outName = "WildernessCultivation";
            Directory.CreateDirectory(outDir);

            var scenes = new[] { "Assets/Scenes/MainScene.unity" };
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(outDir, outName + ".x86_64"),
                target = BuildTarget.StandaloneLinux64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.productName = "Wilderness Cultivation Demo";
            PlayerSettings.companyName = "Devin Demo";

            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            Debug.Log($"[CIBuild] Result={summary.result} Size={summary.totalSize} Errors={summary.totalErrors} Warnings={summary.totalWarnings}");
            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }
    }
}
#endif
