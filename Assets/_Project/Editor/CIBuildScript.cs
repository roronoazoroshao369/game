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

        public static void BuildLinuxMono()
        {
            // Idempotent: probe for TMP Essentials by file presence (not via
            // TMP_Settings.defaultFontAsset, which calls TMP_Settings.instance
            // and triggers an Editor GUI window that NullRefs in batchmode).
            if (!TmpEssentialsImported)
            {
                ImportTMPEssentials();
            }

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
