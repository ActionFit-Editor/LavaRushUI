#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ActionFit.LavaRush.UI.Editor
{
    internal sealed class CatDetectiveStarterInstallPlan
    {
        internal CatDetectiveStarterInstallPlan(string sourceRoot, string targetRoot)
        {
            SourceRoot = sourceRoot;
            TargetRoot = targetRoot;
        }

        internal string SourceRoot { get; }
        internal string TargetRoot { get; }
        internal List<string> NewFiles { get; } = new();
        internal List<string> UnchangedFiles { get; } = new();
        internal List<string> ConflictingFiles { get; } = new();
        internal List<string> DependencyIssues { get; } = new();
        internal bool CanInstall => ConflictingFiles.Count == 0 && DependencyIssues.Count == 0;
        internal bool HasChanges => NewFiles.Count > 0;
    }

    internal static class CatDetectiveStarterInstaller
    {
        internal const string TargetAssetRoot = "Assets/Contents/LavaRush";

        private const string PackageId = "com.actionfit.lava-rush.ui";
        private const string SampleRelativePath = "Samples~/CatDetective Starter";
        private const string MenuRoot = "Tools/Package/ActionFit Lava Rush UI/";
        private const int DetailLimit = 12;

        private static readonly IReadOnlyDictionary<string, string> RequiredPackages =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "com.actionfit.content-core", "0.2.1" },
                { "com.actionfit.lava-rush", "0.1.3" },
                { "com.actionfit.lava-rush.ui", "0.1.3" },
                { "com.actionfit.time", "1.0.3" },
                { "com.unity.addressables", "2.8.1" },
                { "com.unity.ugui", "2.0.0" },
            };

        [MenuItem(MenuRoot + "Preview CatDetective Starter", false, 90)]
        private static void Preview()
        {
            CatDetectiveStarterInstallPlan plan = BuildProjectPlan();
            UnityEngine.Debug.Log(BuildReport(plan));
            EditorUtility.DisplayDialog("CatDetective Starter Preview", BuildReport(plan), "OK");
        }

        [MenuItem(MenuRoot + "Install CatDetective Starter", false, 91)]
        private static void Install()
        {
            CatDetectiveStarterInstallPlan plan = BuildProjectPlan();
            string report = BuildReport(plan);
            UnityEngine.Debug.Log(report);

            if (!plan.CanInstall)
            {
                EditorUtility.DisplayDialog(
                    "CatDetective Starter Blocked",
                    report + "\n\nNo project files were changed.",
                    "OK");
                return;
            }

            if (!plan.HasChanges)
            {
                EditorUtility.DisplayDialog(
                    "CatDetective Starter",
                    report + "\n\nThe installed starter already matches this package version.",
                    "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Install CatDetective Starter",
                report + "\n\nOnly new files will be copied. Addressables are not changed by this step.",
                "Install",
                "Cancel");
            if (!confirmed)
            {
                return;
            }

            Apply(plan);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log(
                $"[CatDetectiveStarterInstaller] Installed {plan.NewFiles.Count} files under {TargetAssetRoot}.");
        }

        [MenuItem(MenuRoot + "Run CatDetective Starter Preflight", false, 92)]
        private static void RunPreflight()
        {
            CatDetectiveStarterInstallPlan plan = BuildProjectPlan();
            string report = BuildReport(plan);
            if (plan.DependencyIssues.Count > 0 || plan.ConflictingFiles.Count > 0)
            {
                UnityEngine.Debug.LogError(report);
                EditorUtility.DisplayDialog("CatDetective Starter Preflight", report, "OK");
                return;
            }

            if (plan.NewFiles.Count > 0)
            {
                UnityEngine.Debug.LogWarning(report);
                EditorUtility.DisplayDialog(
                    "CatDetective Starter Preflight",
                    report + "\n\nThe starter is not fully imported yet.",
                    "OK");
                return;
            }

            UnityEngine.Debug.Log(report);
            EditorUtility.DisplayDialog("CatDetective Starter Preflight", report, "OK");
        }

        internal static CatDetectiveStarterInstallPlan BuildPlan(
            string sourceRoot,
            string targetRoot,
            bool validateDependencies)
        {
            if (string.IsNullOrWhiteSpace(sourceRoot))
            {
                throw new ArgumentException("A sample source root is required.", nameof(sourceRoot));
            }
            if (string.IsNullOrWhiteSpace(targetRoot))
            {
                throw new ArgumentException("A starter target root is required.", nameof(targetRoot));
            }

            sourceRoot = Path.GetFullPath(sourceRoot);
            targetRoot = Path.GetFullPath(targetRoot);
            if (!Directory.Exists(sourceRoot))
            {
                throw new DirectoryNotFoundException($"CatDetective Starter source was not found: {sourceRoot}");
            }

            var plan = new CatDetectiveStarterInstallPlan(sourceRoot, targetRoot);
            foreach (string sourcePath in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                if (string.Equals(Path.GetFileName(sourcePath), ".DS_Store", StringComparison.Ordinal))
                {
                    continue;
                }

                string relativePath = NormalizeRelativePath(Path.GetRelativePath(sourceRoot, sourcePath));
                string targetPath = Path.Combine(targetRoot, relativePath);
                if (!File.Exists(targetPath))
                {
                    plan.NewFiles.Add(relativePath);
                }
                else if (FilesMatch(sourcePath, targetPath))
                {
                    plan.UnchangedFiles.Add(relativePath);
                }
                else
                {
                    plan.ConflictingFiles.Add(relativePath);
                }
            }

            if (validateDependencies)
            {
                plan.DependencyIssues.AddRange(GetDependencyIssues());
            }
            return plan;
        }

        internal static void Apply(CatDetectiveStarterInstallPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }
            if (!plan.CanInstall)
            {
                throw new InvalidOperationException("A blocked CatDetective Starter plan cannot be applied.");
            }

            foreach (string relativePath in plan.NewFiles)
            {
                string sourcePath = Path.Combine(plan.SourceRoot, relativePath);
                string targetPath = Path.Combine(plan.TargetRoot, relativePath);
                string targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                File.Copy(sourcePath, targetPath, false);
            }
        }

        internal static string BuildReport(CatDetectiveStarterInstallPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var builder = new StringBuilder();
            builder.AppendLine("[CatDetectiveStarterInstaller] Preview");
            builder.AppendLine($"Target: {TargetAssetRoot}");
            builder.AppendLine($"New: {plan.NewFiles.Count}");
            builder.AppendLine($"Unchanged: {plan.UnchangedFiles.Count}");
            builder.AppendLine($"Conflicts: {plan.ConflictingFiles.Count}");
            builder.AppendLine($"Dependency issues: {plan.DependencyIssues.Count}");
            AppendDetails(builder, "New files", plan.NewFiles);
            AppendDetails(builder, "Conflicts", plan.ConflictingFiles);
            AppendDetails(builder, "Dependency issues", plan.DependencyIssues);
            builder.Append("Serialized operations: none. Addressables registration is a separate opt-in menu after import.");
            return builder.ToString();
        }

        internal static CatDetectiveStarterInstallPlan BuildProjectPlan()
        {
            PackageManagerPackageInfo package = PackageManagerPackageInfo.FindForAssetPath($"Packages/{PackageId}");
            if (package == null || string.IsNullOrEmpty(package.resolvedPath))
            {
                throw new InvalidOperationException($"Installed package information is unavailable for {PackageId}.");
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            string sourceRoot = Path.Combine(package.resolvedPath, SampleRelativePath);
            string targetRoot = Path.Combine(projectRoot, TargetAssetRoot);
            return BuildPlan(sourceRoot, targetRoot, true);
        }

        private static IEnumerable<string> GetDependencyIssues()
        {
            var installed = PackageManagerPackageInfo.GetAllRegisteredPackages()
                .Where(package => package != null && !string.IsNullOrEmpty(package.name))
                .GroupBy(package => package.name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First().version, StringComparer.Ordinal);

            foreach (KeyValuePair<string, string> requirement in RequiredPackages)
            {
                if (!installed.TryGetValue(requirement.Key, out string version))
                {
                    yield return $"Missing {requirement.Key}@{requirement.Value}";
                    continue;
                }
                if (!string.Equals(version, requirement.Value, StringComparison.Ordinal))
                {
                    yield return $"Expected {requirement.Key}@{requirement.Value}, found {version}";
                }
            }
        }

        private static bool FilesMatch(string firstPath, string secondPath)
        {
            var firstInfo = new FileInfo(firstPath);
            var secondInfo = new FileInfo(secondPath);
            if (firstInfo.Length != secondInfo.Length)
            {
                return false;
            }

            using SHA256 sha256 = SHA256.Create();
            using FileStream first = File.OpenRead(firstPath);
            byte[] firstHash = sha256.ComputeHash(first);
            using FileStream second = File.OpenRead(secondPath);
            byte[] secondHash = sha256.ComputeHash(second);
            return firstHash.SequenceEqual(secondHash);
        }

        private static void AppendDetails(StringBuilder builder, string heading, IReadOnlyList<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(heading + ":");
            int count = Math.Min(values.Count, DetailLimit);
            for (int index = 0; index < count; index++)
            {
                builder.AppendLine("- " + values[index]);
            }
            if (values.Count > count)
            {
                builder.AppendLine($"- ... and {values.Count - count} more");
            }
        }

        private static string NormalizeRelativePath(string path) => path.Replace('\\', '/');
    }

    /// <summary>Batchmode entry point used only by disposable consumer validation.</summary>
    public static class CatDetectiveStarterAutomation
    {
        public static void Install()
        {
            CatDetectiveStarterInstallPlan plan = CatDetectiveStarterInstaller.BuildProjectPlan();
            string report = CatDetectiveStarterInstaller.BuildReport(plan);
            UnityEngine.Debug.Log(report);
            if (!plan.CanInstall)
            {
                throw new InvalidOperationException("CatDetective Starter automation was blocked by preflight.");
            }
            if (!plan.HasChanges)
            {
                UnityEngine.Debug.Log("[CatDetectiveStarterAutomation] Starter is already current.");
                return;
            }

            CatDetectiveStarterInstaller.Apply(plan);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log(
                $"[CatDetectiveStarterAutomation] Installed {plan.NewFiles.Count} files under {CatDetectiveStarterInstaller.TargetAssetRoot}.");
        }
    }
}
#endif
