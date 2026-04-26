#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WildernessCultivation.EditorTools
{
    public static class CIBuildScript
    {
        public static void ImportTMPEssentials()
        {
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CIBuild] TMP Essentials imported.");
        }

        public static void BuildLinuxMono()
        {
            // Idempotent: ensure TMP Essentials are imported before build
            if (TMPro.TMP_Settings.defaultFontAsset == null)
            {
                ImportTMPEssentials();
                AssetDatabase.Refresh();
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
