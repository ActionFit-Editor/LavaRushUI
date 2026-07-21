using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ActionFit.Content;
using ActionFit.Time;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushUITests
    {
        [TearDown]
        public void TearDown()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            foreach (LavaRushPresentation presentation in UnityEngine.Object.FindObjectsByType<LavaRushPresentation>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(presentation.gameObject);
            }
            foreach (LavaRushBootstrap bootstrap in UnityEngine.Object.FindObjectsByType<LavaRushBootstrap>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(bootstrap.gameObject);
            }
        }

        [Test]
        public void ViewModel_ClampsPresentationOnlyValues()
        {
            var model = new LavaRushUIViewModel(
                LavaRushUIScreen.Match,
                null,
                1,
                1,
                4,
                -3,
                100,
                -4,
                -5,
                0,
                TimeSpan.FromSeconds(-1d),
                TimeSpan.FromSeconds(-1d),
                LavaRushResult.None,
                null,
                null,
                null,
                null);

            Assert.That(model.Progress, Is.Zero);
            Assert.That(model.OccupiedSeats, Is.Zero);
            Assert.That(model.SeatCapacity, Is.Zero);
            Assert.That(model.Rank, Is.EqualTo(1));
            Assert.That(model.EventRemaining, Is.EqualTo(TimeSpan.Zero));
            Assert.That(model.StageRemaining, Is.EqualTo(TimeSpan.Zero));
            Assert.That(model.Primary.Visible, Is.False);
        }

        [Test]
        public void Presentation_GeneratesFallbackWithoutMutatingInspectorReferences()
        {
            var root = new GameObject("Lava Rush Presentation Test");
            try
            {
                var presentation = root.AddComponent<PresentationProbe>();
                Assert.That(presentation.InspectorReferences.IsComplete, Is.False);

                presentation.Initialize();

                Assert.That(presentation.IsInitialized, Is.True);
                Assert.That(presentation.InspectorReferences.IsComplete, Is.False);
                Assert.That(root.GetComponentInChildren<Canvas>(true), Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PackageDefaults_UseCompleteProductionPrefabAndPackageOwnedVisuals()
        {
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushPresentation.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetDependencies(prefabPath, true),
                Has.None.StartsWith("Assets/"),
                "The production package prefab must not reference consuming-project assets.");

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                Assert.That(instance, Is.Not.Null);
                LavaRushPresentation presentation = instance.GetComponent<LavaRushPresentation>();
                Assert.That(presentation, Is.Not.Null);
                Assert.That(instance.GetComponentsInChildren<Canvas>(true), Has.Length.EqualTo(1));

                presentation.Initialize();

                Assert.That(instance.GetComponentsInChildren<Canvas>(true), Has.Length.EqualTo(1),
                    "A complete authored prefab must not build a fallback Canvas.");
                Image[] images = instance.GetComponentsInChildren<Image>(true);
                Assert.That(images, Has.Length.GreaterThanOrEqualTo(8));
                foreach (Image image in images)
                {
                    string spritePath = AssetDatabase.GetAssetPath(image.sprite);
                    Assert.That(image.sprite == null || IsPackageVisualPath(spritePath),
                        Is.True,
                        $"{GetTransformPath(image.transform)} -> {spritePath}");
                }
            }
            finally
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void ProductionRoleCounterparts_AreCompleteModularPackagePrefabs()
        {
            bool visualEffectDependenciesAvailable = ExternalVisualEffectDependenciesAvailable();
            string[] paths =
            {
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Img_Title Variant.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Tutorial.prefab",
            };

            Assert.That(paths, Has.Length.EqualTo(14));
            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(AssetDatabase.GetDependencies(path, true), Has.None.StartsWith("Assets/"), path);
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                {
                    if (visualEffectDependenciesAvailable)
                    {
                        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject),
                            Is.Zero,
                            $"{path}: {GetTransformPath(child)}");
                    }
                }

                Image[] images = prefab.GetComponentsInChildren<Image>(true);
                Assert.That(images, Is.Not.Empty, path);
                foreach (Image image in images)
                {
                    string spritePath = AssetDatabase.GetAssetPath(image.sprite);
                    Assert.That(image.sprite == null || IsPackageVisualPath(spritePath),
                        Is.True,
                        $"{path}: {GetTransformPath(image.transform)} -> {spritePath}");
                }
            }

            const string mainPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab";
            string[] mainDependencies = AssetDatabase.GetDependencies(mainPath, true);
            Assert.That(mainDependencies.Count(path => path.EndsWith(".prefab", StringComparison.Ordinal)
                    && !string.Equals(path, mainPath, StringComparison.Ordinal)),
                Is.EqualTo(8),
                "The canonical main prefab must compose all eight package state prefabs as nested instances.");
        }

        [Test]
        public void MatchTutorialTexts_BypassSoftMaskWithoutLosingAuthoredTextSettings()
        {
            AssertTutorialTextMaskContract(
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab",
                3);
        }

        [Test]
        public void MigrationCoverage_RecordsEveryInventoriedPrefabAndImageRole()
        {
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/MigrationCoverage.md";
            string[] lines = File.ReadAllLines(Path.GetFullPath(path));

            Assert.That(lines.Count(line => line.StartsWith("| `Prefabs/", StringComparison.Ordinal)),
                Is.EqualTo(14));
            Assert.That(lines.Count(line => line.StartsWith("| `Images/", StringComparison.Ordinal)),
                Is.EqualTo(56));
        }

        [Test]
        public void OriginalImages_AreEitherSourceEquivalentOrRecordedAsSingleOwner()
        {
            const string sourceRoot = "Assets/_Project/Content/LavaRush/Images";
            const string packageRoot = "Packages/com.actionfit.lava-rush.ui/Runtime/Art";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";
            string[] packageImages = Directory.GetFiles(
                    Path.GetFullPath(packageRoot),
                    "*.png",
                    SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(packageImages, Has.Length.EqualTo(56));

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            Assert.That(ledger, Is.Not.Null);
            Assert.That(ledger.schemaVersion, Is.EqualTo(1));
            Assert.That(ledger.content, Is.EqualTo("LavaRush"));
            Dictionary<string, AssetOwnershipEntry> imageOwnership = ledger.assets
                .Where(entry => entry.packagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(entry => entry.packagePath, StringComparer.Ordinal);
            foreach (KeyValuePair<string, AssetOwnershipEntry> pair in imageOwnership)
            {
                Assert.That(File.Exists(Path.GetFullPath(pair.Value.legacyPath)), Is.False, pair.Value.legacyPath);
                Assert.That(File.Exists(Path.GetFullPath(pair.Value.legacyPath + ".meta")), Is.False, pair.Value.legacyPath);
                Assert.That(AssetDatabase.AssetPathToGUID(pair.Key), Is.EqualTo(pair.Value.guid), pair.Key);
                Assert.That(ComputeSha256Hex(Path.GetFullPath(pair.Key)), Is.EqualTo(pair.Value.sha256), pair.Key);
            }

            string absoluteSourceRoot = Path.GetFullPath(sourceRoot);
            if (!Directory.Exists(absoluteSourceRoot))
            {
                Assert.Ignore("Remaining source parity is verified in the consuming project; single-owner evidence was verified in the isolated package fixture.");
            }

            using SHA256 sha256 = SHA256.Create();
            int migratedImageCount = 0;
            string absolutePackageRoot = Path.GetFullPath(packageRoot);
            foreach (string packageImage in packageImages)
            {
                string relative = packageImage.Substring(absolutePackageRoot.Length + 1).Replace('\\', '/');
                string packagePath = $"{packageRoot}/{relative}";
                string source = Path.Combine(absoluteSourceRoot, relative);
                if (!File.Exists(source))
                {
                    Assert.That(imageOwnership.ContainsKey(packagePath), Is.True, packagePath);
                    migratedImageCount++;
                    continue;
                }

                CollectionAssert.AreEqual(
                    sha256.ComputeHash(File.ReadAllBytes(source)),
                    sha256.ComputeHash(File.ReadAllBytes(packageImage)),
                    relative);
                Assert.That(NormalizeImporterMeta(source + ".meta"),
                    Is.EqualTo(NormalizeImporterMeta(packageImage + ".meta")),
                    relative);
            }

            Assert.That(migratedImageCount, Is.EqualTo(imageOwnership.Count));
        }

        [Test]
        public void MainIcon_HasOnePackageOwnerAndBothPrefabsResolveIt()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/Main_icon.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Main_icon.png";
            const string projectPrefab = "Assets/_Project/Content/LavaRush/Prefabs/Icon/UI_LavaRush_Icon.prefab";
            const string packagePrefab = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab";
            const string expectedGuid = "756239e4572274b17b3fcae6f4964bdb";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
            Assert.That(AssetDatabase.GetDependencies(packagePrefab, true), Contains.Item(packagePath));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(projectPrefab) != null)
            {
                Assert.That(AssetDatabase.GetDependencies(projectPrefab, true), Contains.Item(packagePath));
            }
        }

        [Test]
        public void SharedProductionImages_AreCopiedByteForByteWithImporterMetadata()
        {
            const string packageRoot =
                "Packages/com.actionfit.lava-rush.ui/Runtime/ProductionDependencies";
            string absolutePackageRoot = Path.GetFullPath(packageRoot);
            string[] packageImages = Directory.GetFiles(absolutePackageRoot, "*.png", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(packageImages, Has.Length.EqualTo(19));

            if (!Directory.Exists(Path.GetFullPath("Assets/_Project")))
            {
                Assert.Ignore("Shared production source parity is verified in the consuming project; the isolated package fixture contains only the copied baseline.");
            }

            using SHA256 sha256 = SHA256.Create();
            foreach (string packageImage in packageImages)
            {
                string relative = packageImage.Substring(absolutePackageRoot.Length + 1).Replace('\\', '/');
                string sourcePath = Path.GetFullPath($"Assets/{relative}");
                Assert.That(File.Exists(sourcePath), Is.True, relative);
                CollectionAssert.AreEqual(
                    sha256.ComputeHash(File.ReadAllBytes(sourcePath)),
                    sha256.ComputeHash(File.ReadAllBytes(packageImage)),
                    relative);
                Assert.That(NormalizeImporterMeta(sourcePath + ".meta"),
                    Is.EqualTo(NormalizeImporterMeta(packageImage + ".meta")),
                    relative);
            }
        }

        [Test]
        public void ProductionTmpShaderIncludes_AreCompleteAndByteIdentical()
        {
            string[] includeFiles =
            {
                "TMPro.cginc",
                "TMPro_Mobile.cginc",
                "TMPro_Properties.cginc",
                "TMPro_Surface.cginc",
            };
            const string packageRoot =
                "Packages/com.actionfit.lava-rush.ui/Runtime/ProductionDependencies/TextMesh Pro/Shaders";
            const string sourceRoot = "Assets/TextMesh Pro/Shaders";

            foreach (string includeFile in includeFiles)
            {
                string packagePath = Path.Combine(packageRoot, includeFile);
                Assert.That(File.Exists(packagePath), Is.True, packagePath);
            }

            Assert.That(File.ReadAllText(Path.Combine(packageRoot, "TMP_SDF.shader")),
                Does.Contain("#include \"TMPro_Properties.cginc\"")
                    .And.Contain("#include \"TMPro.cginc\""));
            Assert.That(File.ReadAllText(Path.Combine(packageRoot, "TMP_SDF-Mobile-ShadowOutline.shader")),
                Does.Contain("#include \"TMPro_Properties.cginc\""));

            if (!Directory.Exists(sourceRoot))
            {
                Assert.Ignore("TMP include source parity is verified in the production source project.");
            }

            foreach (string includeFile in includeFiles)
            {
                CollectionAssert.AreEqual(
                    File.ReadAllBytes(Path.Combine(sourceRoot, includeFile)),
                    File.ReadAllBytes(Path.Combine(packageRoot, includeFile)),
                    includeFile);
            }
        }

        [Test]
        public void GeneratedOrSubstitutedPackageArt_IsAbsent()
        {
            Assert.That(Directory.GetFiles(
                    Path.GetFullPath("Packages/com.actionfit.lava-rush.ui/Runtime/Art"),
                    "*.png",
                    SearchOption.TopDirectoryOnly),
                Is.Empty,
                "Only original images in their production-relative subfolders may exist in Runtime/Art.");

            string[] prohibitedNames =
            {
                "LavaRushAccent.png",
                "LavaRushBackdrop.png",
                "LavaRushExplorer.png",
                "LavaRushPanel.png",
                "LavaRushRewardBadge.png",
            };

            foreach (string name in prohibitedNames)
            {
                Assert.That(File.Exists(Path.GetFullPath(
                    $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/{name}")), Is.False, name);
            }
        }

        [Test]
        public void ExternalVisualDependencies_AreDocumentedAtImmutableBundlePins()
        {
            const string dependencyPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/ExternalVisualDependencies.md";
            string contract = File.ReadAllText(Path.GetFullPath(dependencyPath));

            StringAssert.Contains("com.coffee.ui-effect@5.10.8", contract);
            StringAssert.Contains("com.coffee.ui-particle@4.12.1", contract);
            StringAssert.Contains("com.coffee.softmask-for-ugui@3.5.0", contract);
            StringAssert.Contains("com.actionfit.uilighteffector@1.0.0", contract);
            StringAssert.Contains("7dab46ec2378209bd1e524c8336b976eccb3df05", contract);
            StringAssert.Contains("jp.hadashikick.vcontainer@1.16.8", contract);
        }

        [Test]
        public void PublishedPresentationAndDemoGuids_ArePreserved()
        {
            var expected = new Dictionary<string, string>
            {
                ["Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushPresentation.prefab"] = "aa7e020def3ea479e9f1d1d57198f417",
                ["Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushDemo.prefab"] = "23f0e508d6e5c4021ad148af7e107406",
            };

            foreach (KeyValuePair<string, string> pair in expected)
            {
                Assert.That(AssetDatabase.AssetPathToGUID(pair.Key), Is.EqualTo(pair.Value), pair.Key);
            }
        }

        [Test]
        public void ProductionEventStartButton_RoutesTheConfiguredCallback()
        {
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushPresentation.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                LavaRushPresentation presentation = instance.GetComponent<LavaRushPresentation>();
                var actions = new List<LavaRushUIAction>();
                presentation.ActionRequested += actions.Add;
                presentation.Initialize();
                presentation.Present(CreateViewModel(LavaRushUIScreen.EventStart, LavaRushResult.None));

                LavaRushActionTarget target = instance.GetComponentsInChildren<LavaRushActionTarget>(false).Single();
                UI_Button foundationButton = target.GetComponent<UI_Button>();
                Button uguiButton = target.GetComponent<Button>();
                if (foundationButton != null)
                {
                    foundationButton.OnPointerClick(null);
                }
                else
                {
                    Assert.That(uguiButton, Is.Not.Null);
                    uguiButton.onClick.Invoke();
                }

                Assert.That(actions, Is.EqualTo(new[] { LavaRushUIAction.StartStage }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [TestCase(LavaRushUIScreen.EventStart, LavaRushResult.None, "UI_LavaRush_EventStart")]
        [TestCase(LavaRushUIScreen.Difficulty, LavaRushResult.None, "UI_LavaRush_Difficulty")]
        [TestCase(LavaRushUIScreen.Tutorial, LavaRushResult.None, "UI_LavaRush_Tutorial")]
        [TestCase(LavaRushUIScreen.Match, LavaRushResult.None, "UI_LavaRush_Match")]
        [TestCase(LavaRushUIScreen.Result, LavaRushResult.Win, "UI_LavaRush_MatchWin")]
        [TestCase(LavaRushUIScreen.Result, LavaRushResult.Lose, "UI_LavaRush_MatchLose")]
        [TestCase(LavaRushUIScreen.Complete, LavaRushResult.Win, "UI_LavaRush_MatchEnd")]
        [TestCase(LavaRushUIScreen.EventEnd, LavaRushResult.None, "UI_LavaRush_EventEnd")]
        public void AuthoredPresentation_ActivatesTheMatchingStatePrefab(
            LavaRushUIScreen screen,
            LavaRushResult result,
            string expectedName)
        {
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushPresentation.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                LavaRushPresentation presentation = instance.GetComponent<LavaRushPresentation>();
                presentation.Initialize();
                presentation.Present(CreateViewModel(screen, result));

                LavaRushScreenView[] views = instance.GetComponentsInChildren<LavaRushScreenView>(true);
                Assert.That(views, Has.Length.EqualTo(8));
                Assert.That(views.Count(view => view.gameObject.activeSelf), Is.EqualTo(1));
                Assert.That(views.Single(view => view.gameObject.activeSelf).name, Is.EqualTo(expectedName));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void PackageDemoAndCatDetectiveStarter_ReferenceCanonicalPresentation()
        {
            const string presentationPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushPresentation.prefab";
            const string demoPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushDemo.prefab";
            const string starterPath =
                "Packages/com.actionfit.lava-rush.ui/Samples~/CatDetective Starter/Prefabs/UI_LavaRush.prefab";

            LavaRushBootstrap demo = AssetDatabase.LoadAssetAtPath<GameObject>(demoPath)
                ?.GetComponent<LavaRushBootstrap>();
            Assert.That(demo, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(demo.PresentationPrefab), Is.EqualTo(presentationPath));
            Assert.That(demo.InitializeOnStart, Is.True);

            string starterYaml = File.ReadAllText(Path.GetFullPath(starterPath));
            string presentationGuid = AssetDatabase.AssetPathToGUID(presentationPath);
            Assert.That(starterYaml, Does.Contain($"guid: {presentationGuid}"));
            Assert.That(starterYaml, Does.Contain("initializeOnStart: 0"),
                "The imported project adapter must initialize the starter with its project-owned engine.");
        }

        [Test]
        public void Presentation_RejectsThemeOverrideAfterInitialization()
        {
            var root = new GameObject("Lava Rush Theme Timing Test");
            try
            {
                var presentation = root.AddComponent<LavaRushPresentation>();
                presentation.Initialize();

                Assert.Throws<InvalidOperationException>(() => presentation.ApplyThemeOverride(new LavaRushUITheme()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Bootstrap_DefaultFlowUsesEngineCommandsAndImmutablePresentation()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Lava Rush Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Presentation Flow Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bootstrap.InitializeDefault(presentation);

                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.EventStart));
                Assert.That(bootstrap.Engine.TryStartEvent(), Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Difficulty));
                Assert.That(bootstrap.Engine.SelectDifficulty(1), Is.True);
                bootstrap.Engine.SetTutorialDone(true);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Match));
                Assert.That(bootstrap.Engine.StartStage(), Is.True);
                Assert.That(presentation.CurrentModel.StageRemaining, Is.GreaterThan(TimeSpan.Zero));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_GeneratedButtonsRouteTheStandaloneFlow()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Lava Rush Routed Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Routed Presentation Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bootstrap.InitializeDefault(presentation);

                Click(presentation, "Primary");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Difficulty));
                Click(presentation, "Primary");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Tutorial));
                Click(presentation, "Primary");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Match));
                Click(presentation, "Primary");
                Assert.That(bootstrap.Engine.IsStagePlaying, Is.True);

                for (int index = 0; index < 10 && presentation.CurrentModel.Screen == LavaRushUIScreen.Match; index++)
                {
                    Click(presentation, "Primary");
                }

                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));
                Assert.That(bootstrap.Engine.PendingResult, Is.EqualTo(LavaRushResult.Win));
                Click(presentation, "Primary");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Match));
                Assert.That(bootstrap.Engine.PendingResult, Is.EqualTo(LavaRushResult.None));
                Assert.That(bootstrap.Engine.IsStageRewardClaimed(2), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_CloseAndShowPreserveTheLatestEngineSnapshot()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Lava Rush Reopen Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Reopen Presentation Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bool closeRequested = false;
                bootstrap.CloseRequested += () => closeRequested = true;
                bootstrap.InitializeDefault(presentation);

                Click(presentation, "Tertiary");

                Assert.That(closeRequested, Is.True);
                Assert.That(bootstrap.IsVisible, Is.False);
                Assert.That(bootstrap.Engine.IsEventStarted, Is.False);

                bootstrap.Show();

                Assert.That(bootstrap.IsVisible, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.EventStart));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_TimerResolutionRoutesLoseAndRetryWithoutAdvancingStage()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Lava Rush Lose Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Lose Presentation Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bootstrap.InitializeDefault(presentation);
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");

                Click(presentation, "Secondary");

                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));
                Assert.That(bootstrap.Engine.PendingResult, Is.EqualTo(LavaRushResult.Lose));
                Assert.That(bootstrap.Engine.Stage, Is.EqualTo(1));

                Click(presentation, "Primary");

                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Match));
                Assert.That(bootstrap.Engine.PendingResult, Is.EqualTo(LavaRushResult.None));
                Assert.That(bootstrap.Engine.Stage, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_StandaloneFlowCanReachCompletion()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Lava Rush Completion Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Completion Presentation Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bootstrap.InitializeDefault(presentation);
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");

                for (int stage = 1; stage <= 3; stage++)
                {
                    Click(presentation, "Primary");
                    for (int index = 0; index < 20 && presentation.CurrentModel.Screen == LavaRushUIScreen.Match; index++)
                    {
                        Click(presentation, "Primary");
                    }

                    Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));
                    Click(presentation, "Primary");
                }

                Assert.That(bootstrap.Engine.AllStagesComplete, Is.True);
                Assert.That(bootstrap.Engine.State.FinalRewardClaimed, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Complete));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Presentation_UsesInjectedLocalizationProfileRewardAndAudioServices()
        {
            var root = new GameObject("Lava Rush Service Test");
            try
            {
                var presentation = root.AddComponent<LavaRushPresentation>();
                var audio = new RecordingAudio();
                presentation.Initialize(
                    new KeyLocalizer(),
                    audio,
                    new FixedRewardRenderer(),
                    new FixedProfileProvider());
                presentation.Present(new LavaRushUIViewModel(
                    LavaRushUIScreen.Complete,
                    null,
                    1,
                    4,
                    4,
                    10,
                    10,
                    0,
                    0,
                    1,
                    TimeSpan.FromMinutes(5d),
                    TimeSpan.Zero,
                    LavaRushResult.Win,
                    new[] { new ContentReward("coin", 100) },
                    LavaRushUIButtonModel.Hidden,
                    LavaRushUIButtonModel.Hidden,
                    LavaRushUIButtonModel.Hidden));

                Assert.That(FindText(presentation, "Title").text, Is.EqualTo($"[{LavaRushUIKeys.Title}]"));
                Assert.That(FindText(presentation, "Profile").text, Is.EqualTo($"[{LavaRushUIKeys.FormatProfile}]"));
                Assert.That(FindText(presentation, "Rewards").text, Is.EqualTo("rendered-reward"));
                Assert.That(FindText(presentation, "Profile").color, Is.EqualTo(Color.cyan));
                Assert.That(audio.Cues, Does.Contain(LavaRushUIKeys.AudioScreen));
                Assert.That(audio.Cues, Does.Contain(LavaRushUIKeys.AudioReward));

                presentation.Present(new LavaRushUIViewModel(
                    LavaRushUIScreen.Complete,
                    null,
                    1,
                    4,
                    4,
                    11,
                    12,
                    0,
                    0,
                    1,
                    TimeSpan.FromMinutes(5d),
                    TimeSpan.Zero,
                    LavaRushResult.Win,
                    Array.Empty<ContentReward>(),
                    LavaRushUIButtonModel.Hidden,
                    LavaRushUIButtonModel.Hidden,
                    LavaRushUIButtonModel.Hidden));
                Assert.That(audio.Cues, Does.Contain(LavaRushUIKeys.AudioProgress));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Bootstrap_UnavailableRewardKeepsPendingResultForRetry()
        {
            var bootstrapObject = new GameObject("Lava Rush Unavailable Reward Bootstrap Test");
            var presentationObject = new GameObject("Lava Rush Unavailable Reward Presentation Test");
            try
            {
                var engine = CreateUnavailableRewardEngine();
                var bootstrap = bootstrapObject.AddComponent<LavaRushBootstrap>();
                var presentation = presentationObject.AddComponent<LavaRushPresentation>();
                bootstrap.Initialize(engine, presentation, new KeyLocalizer());

                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Assert.That(engine.ForceWin(), Is.EqualTo(LavaRushResult.Win));
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));

                Click(presentation, "Primary");

                Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.Win));
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));
                Assert.That(
                    presentation.CurrentModel.Message,
                    Is.EqualTo($"[{LavaRushUIKeys.StatusRewardUnavailable}]"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        private static LavaRushEngine CreateUnavailableRewardEngine()
        {
            var stages = new[]
            {
                new LavaRushStageDefinition(1, 0, 10, 60, 60, Array.Empty<ContentReward>()),
                new LavaRushStageDefinition(2, 3, 20, 60, 60, new[] { new ContentReward("coin", 5) }),
                new LavaRushStageDefinition(3, 0, 0, 0, 0, new[] { new ContentReward("dia", 1) }),
            };
            var catalog = new LavaRushCatalog(
                "ui-test-catalog-v1",
                "ui-test-balance-v1",
                new[] { new LavaRushDifficultyDefinition(1, stages) });
            var clock = new FixedClock(new DateTime(2026, 7, 18, 3, 0, 0, DateTimeKind.Utc));
            return new LavaRushEngine(
                new MemoryStore(),
                new UnavailableRewards(),
                new SingleCatalogResolver(catalog),
                clock,
                TimeZoneInfo.Utc,
                clock,
                new FixedRandom(),
                new LinearLavaRushSeatCurveProvider(),
                "tests/lava-rush-ui-unavailable",
                new AllowLavaRushAccessPolicy(),
                new WeekendSchedule());
        }

        private static LavaRushUIViewModel CreateViewModel(LavaRushUIScreen screen, LavaRushResult result)
        {
            return new LavaRushUIViewModel(
                screen,
                null,
                2,
                2,
                4,
                70,
                100,
                2,
                8,
                3,
                TimeSpan.FromHours(2d),
                TimeSpan.FromSeconds(36d),
                result,
                new[] { new ContentReward("coin", 100) },
                new LavaRushUIButtonModel(LavaRushUIAction.StartStage, "Continue"),
                new LavaRushUIButtonModel(LavaRushUIAction.AddProgress, "Boost"),
                new LavaRushUIButtonModel(LavaRushUIAction.Close, "Close"));
        }

        private static bool ExternalVisualEffectDependenciesAvailable()
        {
            string[] representativeScriptGuids =
            {
                "938fce054c42f40a3969e27a88d9bdd8",
                "00e55ae1441ff4583859c55384964d86",
                "385b7d1277b6c4007a84c065696e0f8c",
                "ab40d795edefe49df9aaba2f4fba474c",
            };

            return representativeScriptGuids.All(guid =>
                !string.IsNullOrWhiteSpace(AssetDatabase.GUIDToAssetPath(guid)));
        }

        private static bool IsPackageVisualPath(string path)
        {
            return string.Equals(path, "Resources/unity_builtin_extra", StringComparison.Ordinal)
                || string.Equals(path, "Library/unity default resources", StringComparison.Ordinal)
                || path.StartsWith(
                "Packages/com.actionfit.lava-rush.ui/Runtime/Art/",
                StringComparison.Ordinal)
                || path.StartsWith(
                    "Packages/com.actionfit.lava-rush.ui/Runtime/ProductionDependencies/",
                    StringComparison.Ordinal);
        }

        private static string NormalizeImporterMeta(string path)
        {
            return string.Join("\n", File.ReadAllLines(path)
                .Where(line => !line.StartsWith("guid:", StringComparison.Ordinal)
                    && !line.StartsWith("timeCreated:", StringComparison.Ordinal)));
        }

        private static string ComputeSha256Hex(string path)
        {
            using SHA256 sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(File.ReadAllBytes(path)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string GetTransformPath(Transform transform)
        {
            var names = new Stack<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", names);
        }

        private static void AssertTutorialTextMaskContract(string prefabPath, int expectedCount)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, prefabPath);

            bool canResolveSoftMask = CanResolveSoftMaskType();
            if (!canResolveSoftMask)
                AssertSerializedSoftMaskMarker(prefabPath);

            MaskableGraphic[] tutorialTexts = prefab.GetComponentsInChildren<MaskableGraphic>(true)
                .Where(candidate => candidate.name.StartsWith("Txt_Tutorial", StringComparison.Ordinal))
                .OrderBy(candidate => candidate.name, StringComparer.Ordinal)
                .ToArray();
            Assert.That(tutorialTexts, Has.Length.EqualTo(expectedCount), prefabPath);
            CollectionAssert.AreEquivalent(
                Enumerable.Range(1, expectedCount).Select(index => $"Txt_Tutorial{index}"),
                tutorialTexts.Select(candidate => candidate.name),
                prefabPath);

            foreach (MaskableGraphic tutorialText in tutorialTexts)
            {
                Assert.That(tutorialText.GetType().FullName, Is.EqualTo("TMPro.TextMeshProUGUI"), tutorialText.name);
                Assert.That(tutorialText.maskable, Is.False, tutorialText.name);
                UI_Text authoredText = tutorialText.GetComponent<UI_Text>();
                Assert.That(authoredText, Is.Not.Null, tutorialText.name);
                Assert.That(authoredText.IsLocalized, Is.False, tutorialText.name);
                Assert.That(authoredText.IsOutlineOn, Is.False, tutorialText.name);
                AssertLegacyLocalizationEvent(tutorialText);
                if (canResolveSoftMask)
                    Assert.That(HasSoftMaskInParents(tutorialText.transform), Is.True, tutorialText.name);
            }
        }

        private static bool CanResolveSoftMaskType()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                assembly.GetType("Coffee.UISoftMask.SoftMask", false) != null);
        }

        private static void AssertSerializedSoftMaskMarker(string prefabPath)
        {
            const string softMaskScriptGuid = "guid: 385b7d1277b6c4007a84c065696e0f8c";
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string serializedPrefab = File.ReadAllText(Path.Combine(projectRoot, prefabPath));
            int markerCount = serializedPrefab.Split(
                new[] { softMaskScriptGuid },
                StringSplitOptions.None).Length - 1;
            Assert.That(markerCount, Is.EqualTo(1), prefabPath);
        }

        private static void AssertLegacyLocalizationEvent(MaskableGraphic tutorialText)
        {
            MonoBehaviour localizer = tutorialText.GetComponents<MonoBehaviour>().SingleOrDefault(component =>
                component != null
                && string.Equals(
                    component.GetType().FullName,
                    "UnityEngine.Localization.Components.LocalizeStringEvent",
                    StringComparison.Ordinal));
            Assert.That(localizer, Is.Not.Null, tutorialText.name);

            var serializedLocalizer = new SerializedObject(localizer);
            Assert.That(
                serializedLocalizer.FindProperty(
                    "m_StringReference.m_TableReference.m_TableCollectionName").stringValue,
                Is.Not.Empty,
                tutorialText.name);
            Assert.That(
                serializedLocalizer.FindProperty(
                    "m_StringReference.m_TableEntryReference.m_KeyId").longValue,
                Is.GreaterThan(0),
                tutorialText.name);
            Assert.That(
                serializedLocalizer.FindProperty(
                    "m_UpdateString.m_PersistentCalls.m_Calls.Array.size").intValue,
                Is.EqualTo(1),
                tutorialText.name);
            Assert.That(
                serializedLocalizer.FindProperty(
                    "m_UpdateString.m_PersistentCalls.m_Calls.Array.data[0].m_Target").objectReferenceValue,
                Is.SameAs(tutorialText),
                tutorialText.name);
            Assert.That(
                serializedLocalizer.FindProperty(
                    "m_UpdateString.m_PersistentCalls.m_Calls.Array.data[0].m_MethodName").stringValue,
                Is.EqualTo("SetText"),
                tutorialText.name);
        }

        private static bool HasSoftMaskInParents(Transform transform)
        {
            for (Transform current = transform.parent; current != null; current = current.parent)
            {
                if (current.GetComponents<MonoBehaviour>().Any(component =>
                        component != null
                        && string.Equals(
                            component.GetType().FullName,
                            "Coffee.UISoftMask.SoftMask",
                            StringComparison.Ordinal)))
                    return true;
            }

            return false;
        }

        private static void Click(LavaRushPresentation presentation, string buttonName)
        {
            foreach (Button button in presentation.GetComponentsInChildren<Button>(true))
            {
                if (!string.Equals(button.gameObject.name, buttonName, StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.That(button.gameObject.activeSelf, Is.True, $"{buttonName} is hidden.");
                Assert.That(button.interactable, Is.True, $"{buttonName} is disabled.");
                button.onClick.Invoke();
                return;
            }

            Assert.Fail($"Button '{buttonName}' was not generated.");
        }

        private static Text FindText(LavaRushPresentation presentation, string objectName)
        {
            foreach (Text text in presentation.GetComponentsInChildren<Text>(true))
            {
                if (string.Equals(text.gameObject.name, objectName, StringComparison.Ordinal))
                {
                    return text;
                }
            }

            Assert.Fail($"Text '{objectName}' was not generated.");
            return null;
        }

        private sealed class PresentationProbe : LavaRushPresentation
        {
            public LavaRushUIViewReferences InspectorReferences => InspectorView;
        }

        [Serializable]
        private sealed class AssetOwnershipLedger
        {
            public int schemaVersion;
            public string content;
            public AssetOwnershipEntry[] assets;
        }

        [Serializable]
        private sealed class AssetOwnershipEntry
        {
            public string legacyPath;
            public string packagePath;
            public string guid;
            public string sha256;
        }

        private sealed class KeyLocalizer : ILavaRushUILocalizer
        {
            public string Get(string key, string fallback) => $"[{key}]";
        }

        private sealed class RecordingAudio : ILavaRushUIAudio
        {
            public List<string> Cues { get; } = new();
            public void Play(string cue) => Cues.Add(cue);
        }

        private sealed class FixedRewardRenderer : ILavaRushUIRewardRenderer
        {
            public string Render(IReadOnlyList<ContentReward> rewards, ILavaRushUILocalizer localizer)
            {
                return "rendered-reward";
            }
        }

        private sealed class FixedProfileProvider : ILavaRushUIProfileProvider
        {
            public LavaRushUIProfile GetProfile() => new("Package Runner", Color.cyan);
        }

        private sealed class MemoryStore : IContentStateStore
        {
            private string _json;

            public bool TryLoad(string contentId, out string json)
            {
                json = _json;
                return !string.IsNullOrWhiteSpace(json);
            }

            public void Save(string contentId, string json) => _json = json;
            public void Delete(string contentId) => _json = null;
        }

        private sealed class UnavailableRewards : IContentRewardService
        {
            public bool IsAvailable => false;
            public bool HasGranted(string transactionId) => false;
            public bool GrantOnce(string transactionId, IReadOnlyList<ContentReward> rewards) => false;
        }

        private sealed class FixedClock : IClock, ILavaRushLegacyLocalClock
        {
            public FixedClock(DateTime utcNow)
            {
                UtcNow = utcNow;
            }

            public DateTime UtcNow { get; }
            public DateTime Now => DateTime.SpecifyKind(UtcNow.AddHours(9d), DateTimeKind.Unspecified);
        }

        private sealed class SingleCatalogResolver : ILavaRushCatalogResolver
        {
            private readonly LavaRushCatalog _catalog;

            public SingleCatalogResolver(LavaRushCatalog catalog)
            {
                _catalog = catalog;
            }

            public LavaRushCatalog Current => _catalog;

            public bool TryResolve(string catalogVersion, string balanceRevision, out LavaRushCatalog catalog)
            {
                bool matches = string.Equals(catalogVersion, _catalog.CatalogVersion, StringComparison.Ordinal)
                    && string.Equals(balanceRevision, _catalog.BalanceRevision, StringComparison.Ordinal);
                catalog = matches ? _catalog : null;
                return matches;
            }
        }

        private sealed class FixedRandom : ILavaRushRandom
        {
            public int Range(int minInclusive, int maxExclusive) => minInclusive;
        }

        private sealed class WeekendSchedule : ILavaRushSchedulePolicy
        {
            public bool IsEnabled => true;
            public bool IsActiveDay(DayOfWeek dayOfWeek) => dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        }
    }
}
