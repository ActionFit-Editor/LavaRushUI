using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ActionFit.Content;
using ActionFit.Time;
using NUnit.Framework;
using ReferenceBinding.Editor;
using TMPro;
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

        [TestCase("Main/UI_LavaRush.prefab", "ffae8bfdd6acf4657b158ff432e5a23b")]
        [TestCase("UI/UI_LavaRush_Difficulty.prefab", "00b6d23c9709d48b886d0f0dd8c5d34b")]
        [TestCase("UI/UI_LavaRush_EventEnd.prefab", "47b835ccb139f4a0b870cc9d43c78e7f")]
        [TestCase("UI/UI_LavaRush_EventStart.prefab", "a96c2faa50c004de5b005ae3109a10f3")]
        [TestCase("UI/UI_LavaRush_Match.prefab", "42d4520d10cf9424f9bc6a05448b13b7")]
        [TestCase("UI/UI_LavaRush_MatchEnd.prefab", "039f1d0f0ff5a4b34830ad671ac87ea0")]
        [TestCase("UI/UI_LavaRush_MatchLose.prefab", "ef162e13966984e259fc7f83bf048716")]
        [TestCase("UI/UI_LavaRush_MatchWin.prefab", "926cee58a7f7540f29183bd498d6a10d")]
        [TestCase("UI/UI_LavaRush_Tutorial.prefab", "4432f1a930dcd4b598e0e648668d983a")]
        public void ProductionRole_HasOnePackageOwnerAndPreservesOriginalGuid(
            string relativePath,
            string expectedGuid)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Prefabs/{relativePath}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/{relativePath}";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False, legacyPath);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False, legacyPath);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid), packagePath);
            Assert.That(AssetDatabase.GetDependencies(packagePath, true), Has.None.StartsWith("Assets/"), packagePath);

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, packagePath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo(expectedGuid));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(packagePath))));
        }

        [Test]
        public void CanonicalProductionPrefab_ComposesPresentationBootstrapAndFlowQueueOwner()
        {
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<LavaRushPresentation>(), Is.Not.Null);
            LavaRushBootstrap bootstrap = prefab.GetComponent<LavaRushBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.InitializeOnStart, Is.False,
                "The production prefab must be initialized by its package or product composition owner.");
            LavaRushFlowView flow = prefab.GetComponentInChildren<LavaRushFlowView>(true);
            Assert.That(flow, Is.Not.Null);
            Assert.That(flow.gameObject.activeSelf, Is.False,
                "The queue owner must remain closed until a caller requests the Lava Rush flow.");
        }

        [Test]
        public void PackagePrefabs_PassReadOnlyReferenceBindingValidation()
        {
            const string prefabRoot = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs";
            int screenViewCount = 0;
            int bindingOwnerCount = 0;

            using (ReferenceProcessingScope.EnterValidateOnly("LavaRushUI.PackagePrefabs"))
            {
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabRoot });
                Assert.That(prefabGuids, Is.Not.Empty);

                foreach (string prefabGuid in prefabGuids)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    Assert.That(prefab, Is.Not.Null, prefabPath);

                    foreach (LavaRushScreenView owner in prefab.GetComponentsInChildren<LavaRushScreenView>(true))
                    {
                        screenViewCount++;
                        var serializedOwner = new SerializedObject(owner);
                        SerializedProperty panel = serializedOwner.FindProperty("refs.production.panel");
                        Assert.That(panel, Is.Not.Null, prefabPath);
                        Assert.That(panel.objectReferenceValue, Is.Not.Null,
                            $"{prefabPath}: {GetTransformPath(owner.transform)} must keep its authored panel reference.");
                    }

                    foreach (LavaRushBootstrap owner in prefab.GetComponentsInChildren<LavaRushBootstrap>(true))
                    {
                        bindingOwnerCount++;
                        ReferenceBindingReport report = ReferenceBindingValidation.Validate(owner);
                        string diagnostics = string.Join(
                            "\n",
                            report.Issues.Select(issue =>
                                $"{issue.Severity} {issue.Code} {issue.PropertyPath}: {issue.Message}"));
                        Assert.That(report.Changed, Is.False, prefabPath);
                        Assert.That(report.Issues, Is.Empty,
                            $"{prefabPath}: {GetTransformPath(owner.transform)}\n{diagnostics}");
                    }

                    foreach (LavaRushAccessIconView owner in prefab.GetComponentsInChildren<LavaRushAccessIconView>(true))
                    {
                        bindingOwnerCount++;
                        ReferenceBindingReport report = ReferenceBindingValidation.Validate(owner);
                        string diagnostics = string.Join(
                            "\n",
                            report.Issues.Select(issue =>
                                $"{issue.Severity} {issue.Code} {issue.PropertyPath}: {issue.Message}"));
                        Assert.That(report.Changed, Is.False, prefabPath);
                        Assert.That(report.Issues, Is.Empty,
                            $"{prefabPath}: {GetTransformPath(owner.transform)}\n{diagnostics}");
                    }

                    foreach (LavaRushInGameCellView owner in prefab.GetComponentsInChildren<LavaRushInGameCellView>(true))
                    {
                        bindingOwnerCount++;
                        ReferenceBindingReport report = ReferenceBindingValidation.Validate(owner);
                        string diagnostics = string.Join(
                            "\n",
                            report.Issues.Select(issue =>
                                $"{issue.Severity} {issue.Code} {issue.PropertyPath}: {issue.Message}"));
                        Assert.That(report.Changed, Is.False, prefabPath);
                        Assert.That(report.Issues, Is.Empty,
                            $"{prefabPath}: {GetTransformPath(owner.transform)}\n{diagnostics}");
                    }

                    foreach (LavaRushBlockView owner in prefab.GetComponentsInChildren<LavaRushBlockView>(true))
                    {
                        bindingOwnerCount++;
                        ReferenceBindingReport report = ReferenceBindingValidation.Validate(owner);
                        string diagnostics = string.Join(
                            "\n",
                            report.Issues.Select(issue =>
                                $"{issue.Severity} {issue.Code} {issue.PropertyPath}: {issue.Message}"));
                        Assert.That(report.Changed, Is.False, prefabPath);
                        Assert.That(report.Issues, Is.Empty,
                            $"{prefabPath}: {GetTransformPath(owner.transform)}\n{diagnostics}");
                    }
                }
            }

            Assert.That(screenViewCount, Is.GreaterThan(0));
            Assert.That(bindingOwnerCount, Is.GreaterThan(0));
        }

        [Test]
        public void AccessIconPrefab_ProvidesRequiredTimerBinding()
        {
            const string legacyPath =
                "Assets/_Project/Content/LavaRush/Prefabs/Icon/UI_LavaRush_Icon.prefab";
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(prefabPath), Is.EqualTo("f7a017bca31e14a2eae90bc3a60cd5e3"));
            Assert.That(prefab, Is.Not.Null);
            LavaRushAccessIconView view = prefab.GetComponent<LavaRushAccessIconView>();
            Assert.That(view, Is.Not.Null);
            Assert.That(view.TimerText, Is.Not.Null);
            Assert.That(view.TimerText.name, Is.EqualTo("Txt_Timer"));
            Assert.That(view.TimerText.color, Is.EqualTo(Color.black));
            Transform timer = prefab.transform.Find("Txt_Timer");
            UI_Text timerFoundation = timer.GetComponent<UI_Text>();
            Assert.That(timerFoundation, Is.Not.Null);
            Assert.That(view.TimerText, Is.SameAs(timerFoundation.TMP));
            Assert.That(timer.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            AssertLocalFileIdentifier(timerFoundation, "f7a017bca31e14a2eae90bc3a60cd5e3", 211443928736582838L);

            var serializedTimer = new SerializedObject(timerFoundation);
            Assert.That(serializedTimer.FindProperty("isSettingOutline").boolValue, Is.True);
            Assert.That(serializedTimer.FindProperty("outlineColor").colorValue,
                Is.EqualTo(new Color(0.4528302f, 0.055626344f, 0f, 1f)));
            Assert.That(serializedTimer.FindProperty("outlineWidth").floatValue, Is.EqualTo(0.1f));

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, prefabPath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo("f7a017bca31e14a2eae90bc3a60cd5e3"));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(prefabPath))));
        }

        [Test]
        public void InGameCellPrefab_ProvidesRequiredBindings()
        {
            const string legacyPath =
                "Assets/_Project/Content/LavaRush/Prefabs/Icon/UI_LavaRush_Cell.prefab";
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(prefabPath), Is.EqualTo("800bfcd600b24494eb593e8f6ed492b1"));
            Assert.That(prefab, Is.Not.Null);

            LavaRushInGameCellView view = prefab.GetComponent<LavaRushInGameCellView>();
            Assert.That(view, Is.Not.Null);
            Assert.That(view.IsComplete, Is.True);
            Assert.That(view.TimerText.name, Is.EqualTo("Txt_Timer"));
            Assert.That(view.StatusText.name, Is.EqualTo("Txt_Status"));
            Assert.That(view.StatusGauge.name, Is.EqualTo("fill"));
            Assert.That(view.TargetProgress.name, Is.EqualTo("item"));
            Assert.That(view.Indicator.name, Is.EqualTo("Indicator"));
            Assert.That(view.RemainTextRoot.name, Is.EqualTo("Rect_RemainText"));
            Assert.That(view.RemainCountText.name, Is.EqualTo("Txt_ReaminCount"));
            Assert.That(view.AnimationDuration, Is.EqualTo(0.3f));

            Transform timer = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Txt_Timer");
            Transform status = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Txt_Status");
            Transform localizedStatus = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Text (TMP) (1)");
            Transform remainCount = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Txt_ReaminCount");
            Transform remainTitle = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Txt_RemainTitle");
            Transform indicator = prefab.transform.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Indicator");
            UI_Text timerFoundation = timer.GetComponent<UI_Text>();
            UI_Text statusFoundation = status.GetComponent<UI_Text>();
            UI_Text localizedStatusFoundation = localizedStatus.GetComponent<UI_Text>();
            UI_Text remainCountFoundation = remainCount.GetComponent<UI_Text>();
            UI_Text remainTitleFoundation = remainTitle.GetComponent<UI_Text>();
            ScalePulse scalePulse = indicator.GetComponent<ScalePulse>();

            Assert.That(timerFoundation, Is.Not.Null);
            Assert.That(statusFoundation, Is.Not.Null);
            Assert.That(localizedStatusFoundation, Is.Not.Null);
            Assert.That(remainCountFoundation, Is.Not.Null);
            Assert.That(remainTitleFoundation, Is.Not.Null);
            Assert.That(scalePulse, Is.Not.Null);
            Assert.That(indicator.GetComponent<Animator>(), Is.Null);
            Assert.That(view.TimerText, Is.SameAs(timerFoundation.TMP));
            Assert.That(view.StatusText, Is.SameAs(statusFoundation.TMP));
            Assert.That(view.Indicator, Is.SameAs(scalePulse));
            Assert.That(timer.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            Assert.That(status.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            Assert.That(localizedStatus.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            Assert.That(remainCount.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            Assert.That(remainTitle.GetComponents<UI_Text>(), Has.Length.EqualTo(1));
            Assert.That(prefab.GetComponentsInChildren<UI_Text>(true), Has.Length.EqualTo(5));
            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true).Any(component =>
                component != null
                && string.Equals(
                    component.GetType().FullName,
                    "UnityEngine.Localization.Components.LocalizeStringEvent",
                    StringComparison.Ordinal)), Is.False);
            Assert.That(indicator.GetComponents<ScalePulse>(), Has.Length.EqualTo(1));
            AssertLocalFileIdentifier(timerFoundation, "800bfcd600b24494eb593e8f6ed492b1", 5168693481671126200L);
            AssertLocalFileIdentifier(statusFoundation, "800bfcd600b24494eb593e8f6ed492b1", 7826278428625365531L);
            AssertLocalFileIdentifier(localizedStatusFoundation, "800bfcd600b24494eb593e8f6ed492b1", 2048006250720009917L);
            AssertLocalFileIdentifier(scalePulse, "800bfcd600b24494eb593e8f6ed492b1", 6560593665191783308L);
            Assert.That(indicator.localScale, Is.EqualTo(new Vector3(0.8f, 0.8f, 0.8f)));

            var serializedRemainCount = new SerializedObject(remainCountFoundation);
            Assert.That(serializedRemainCount.FindProperty("isSettingOutline").boolValue, Is.True);
            Assert.That(serializedRemainCount.FindProperty("outlineColor").colorValue, Is.EqualTo(Color.black));
            Assert.That(serializedRemainCount.FindProperty("outlineWidth").floatValue, Is.EqualTo(0.2f));

            var serializedRemainTitle = new SerializedObject(remainTitleFoundation);
            Assert.That(serializedRemainTitle.FindProperty("isLocalizeText").boolValue, Is.True);
            Assert.That(serializedRemainTitle.FindProperty(
                "localizedString.m_TableReference.m_TableCollectionName").stringValue,
                Is.EqualTo("GUID:b6a6f22bd10d54efd823886d5d5b1946"));
            Assert.That(serializedRemainTitle.FindProperty(
                "localizedString.m_TableEntryReference.m_KeyId").longValue,
                Is.EqualTo(44559608432287744L));
            Assert.That(serializedRemainTitle.FindProperty("isSettingOutline").boolValue, Is.True);
            Assert.That(serializedRemainTitle.FindProperty("outlineColor").colorValue, Is.EqualTo(Color.black));
            Assert.That(serializedRemainTitle.FindProperty("outlineWidth").floatValue, Is.EqualTo(0.2f));

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, prefabPath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo("800bfcd600b24494eb593e8f6ed492b1"));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(prefabPath))));
        }

        [Test]
        public void RequiredBehaviorSubstitutions_FailReferenceBindingValidation()
        {
            const string iconPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab";
            const string cellPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab";
            GameObject canvasRoot = new GameObject(
                "Lava Rush Required Binding Test Canvas",
                typeof(Canvas));
            GameObject icon = UnityEngine.Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>(iconPath),
                canvasRoot.transform,
                false);
            GameObject cell = UnityEngine.Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>(cellPath),
                canvasRoot.transform,
                false);

            try
            {
                LavaRushAccessIconView iconView = icon.GetComponent<LavaRushAccessIconView>();
                Transform timer = icon.transform.Find("Txt_Timer");
                UnityEngine.Object.DestroyImmediate(timer.GetComponent<UI_Text>());
                Assert.That(timer.GetComponent<TMPro.TextMeshProUGUI>(), Is.Not.Null,
                    "The negative fixture must retain the base TMP component.");

                LavaRushInGameCellView cellView = cell.GetComponent<LavaRushInGameCellView>();
                Transform indicator = cell.GetComponentsInChildren<Transform>(true)
                    .Single(child => child.name == "Indicator");
                UnityEngine.Object.DestroyImmediate(indicator.GetComponent<ScalePulse>());
                indicator.gameObject.AddComponent<Animator>();

                using (ReferenceProcessingScope.EnterValidateOnly("LavaRushUI.RequiredBehaviorSubstitutions"))
                {
                    ReferenceBindingReport iconReport = ReferenceBindingValidation.Validate(iconView);
                    ReferenceBindingReport cellReport = ReferenceBindingValidation.Validate(cellView);

                    Assert.That(iconReport.Changed, Is.False);
                    Assert.That(iconReport.Issues.Any(issue =>
                        issue.RequiredErrorCode == "LAVA_RUSH_UI_ICON_TIMER_MISSING"), Is.True);
                    Assert.That(cellReport.Changed, Is.False);
                    Assert.That(cellReport.Issues.Any(issue =>
                        issue.RequiredErrorCode == "LAVA_RUSH_UI_CELL_INDICATOR_MISSING"), Is.True);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasRoot);
            }
        }

        [Test]
        public void ContentLavaBlock_HasOnePackageOwnerAndPreservesCanonicalRoleContract()
        {
            const string originalGuid = "8107a7b8fccd249f4947f08aca662f01";
            const string retiredCopyGuid = "882e3f6cc75c241b8adeb2fbe685427f";
            const string legacyPath =
                "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab";
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab";
            const string matchPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(prefabPath), Is.EqualTo(originalGuid));
            Assert.That(AssetDatabase.GUIDToAssetPath(retiredCopyGuid), Is.Empty);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetDependencies(prefabPath, true), Has.None.StartsWith("Assets/"));
            LavaRushBlockView view = prefab.GetComponent<LavaRushBlockView>();
            Assert.That(view, Is.Not.Null);
            Assert.That(view.IsComplete, Is.True);
            AssertLocalFileIdentifier(view, originalGuid, 6810643454494422369L);

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, prefabPath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo(originalGuid));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(prefabPath))));

            GameObject match = AssetDatabase.LoadAssetAtPath<GameObject>(matchPath);
            Assert.That(match, Is.Not.Null, matchPath);
            Assert.That(AssetDatabase.GetDependencies(matchPath, true), Has.None.StartsWith("Assets/"));
            Assert.That(match.GetComponent<LavaRushScreenView>(), Is.Not.Null,
                "The canonical Match role must use the package-neutral screen binder.");
            StringAssert.DoesNotContain(retiredCopyGuid, File.ReadAllText(Path.GetFullPath(matchPath)));
        }

        [Test]
        public void ImgTitleVariant_HasOnePackageOwnerAndPreservesCanonicalVisualContract()
        {
            const string legacyPath =
                "Assets/_Project/Content/LavaRush/Prefabs/Base/Img_Title Variant.prefab";
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Img_Title Variant.prefab";
            const string imageBasePath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Internal/Img_LavaRush_TitleBase.prefab";
            const string textBasePath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Internal/Txt_LavaRush_TitleBase.prefab";
            const string fontPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/ProductionDependencies/_Project/_Common/Fonts/FontAssets/Maplestory Bold SDF.asset";
            const string baseEventPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab";
            const string matchPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";
            const string originalGuid = "faf6d9eda0d564250be884de1760886b";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(prefabPath), Is.EqualTo(originalGuid));
            Assert.That(prefab, Is.Not.Null);
            Assert.That(PrefabUtility.GetPrefabAssetType(prefab), Is.EqualTo(PrefabAssetType.Variant));
            Assert.That(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(prefab)),
                Is.EqualTo(imageBasePath));
            AssertLocalFileIdentifier(prefab, originalGuid, 7213435574878931672L);
            AssertLocalFileIdentifier(prefab.transform, originalGuid, 7002015167380612912L);
            Image rootImage = prefab.GetComponents<Image>().Single(component => component.GetType() == typeof(Image));
            AssertLocalFileIdentifier(rootImage, originalGuid, 6366274541015651441L);
            Component timerText = prefab.GetComponentsInChildren<Component>(true).Single(component =>
                component.GetType().Name == "UI_Text" && component.name == "Txt_Timer");
            AssertLocalFileIdentifier(timerText, originalGuid, 793904727504845543L);

            Transform timer = prefab.transform.Find("Img_Timer");
            Transform title = prefab.transform.Find("Txt_Title");
            Assert.That(timer, Is.Not.Null);
            Assert.That(title, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(timer.gameObject)),
                Is.EqualTo(imageBasePath));
            Assert.That(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(title.gameObject)),
                Is.EqualTo(textBasePath));

            UI_Text titleFoundation = title.GetComponent<UI_Text>();
            TextMeshProUGUI titleTmp = title.GetComponent<TextMeshProUGUI>();
            Assert.That(titleFoundation, Is.Not.Null);
            Assert.That(titleTmp, Is.Not.Null);
            Assert.That(titleTmp.fontSharedMaterial, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(titleTmp.fontSharedMaterial), Is.EqualTo(fontPath));
            var serializedTitle = new SerializedObject(titleFoundation);
            Assert.That(serializedTitle.FindProperty("isLocalizeText").boolValue, Is.True);
            Assert.That(serializedTitle.FindProperty(
                "localizedString.m_TableReference.m_TableCollectionName").stringValue,
                Is.EqualTo("GUID:b6a6f22bd10d54efd823886d5d5b1946"));
            Assert.That(serializedTitle.FindProperty(
                "localizedString.m_TableEntryReference.m_KeyId").longValue,
                Is.EqualTo(42680591513018368L));
            Assert.That(serializedTitle.FindProperty("isSettingOutline").boolValue, Is.True);
            Assert.That(serializedTitle.FindProperty("outlineColor").colorValue, Is.EqualTo(Color.black));
            Assert.That(serializedTitle.FindProperty("outlineWidth").floatValue, Is.EqualTo(0.1f));
            Assert.That(serializedTitle.FindProperty("isSettingUnderlay").boolValue, Is.True);
            Assert.That(serializedTitle.FindProperty("underlayColor").colorValue,
                Is.EqualTo(new Color(0.7924528f, 0.1981132f, 0.1981132f, 1f)));
            Assert.That(serializedTitle.FindProperty("underlayOffsetY").floatValue, Is.EqualTo(-0.5f));
            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true).Any(component =>
                component != null && component.GetType().Name == "LocalizeStringEvent"), Is.False);

            string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
            Assert.That(dependencies, Contains.Item(imageBasePath));
            Assert.That(dependencies, Contains.Item(textBasePath));
            Assert.That(dependencies, Contains.Item(fontPath));
            Assert.That(dependencies.Any(path => path.StartsWith("Assets/", StringComparison.Ordinal)), Is.False);

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform previewTitle = prefabContents.transform.Find("Txt_Title");
                UI_Text previewFoundation = previewTitle.GetComponent<UI_Text>();
                TextMeshProUGUI previewTmp = previewTitle.GetComponent<TextMeshProUGUI>();
                previewFoundation.ApplyOutline();

                Material previewMaterial = previewTmp.fontSharedMaterial;
                Assert.That(previewMaterial, Is.Not.Null);
                Assert.That(previewMaterial.shader.name,
                    Is.EqualTo("TextMeshPro/Mobile/Distance Field Shadow Outline"));
                Assert.That(previewMaterial.IsKeywordEnabled("OUTLINE_ON"), Is.True);
                Assert.That(previewMaterial.GetFloat("_OutlineWidth"), Is.EqualTo(0.1f).Within(0.0001f));
                Assert.That(previewMaterial.IsKeywordEnabled("UNDERLAY_ON"), Is.True);
                Assert.That(previewMaterial.GetColor("_UnderlayColor"),
                    Is.EqualTo(new Color(0.7924528f, 0.1981132f, 0.1981132f, 1f)));
                Assert.That(previewMaterial.GetFloat("_UnderlayOffsetY"), Is.EqualTo(-0.5f).Within(0.0001f));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, prefabPath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo(originalGuid));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(prefabPath))));

            GameObject baseEvent = AssetDatabase.LoadAssetAtPath<GameObject>(baseEventPath);
            GameObject match = AssetDatabase.LoadAssetAtPath<GameObject>(matchPath);
            Assert.That(baseEvent, Is.Not.Null, baseEventPath);

            Transform baseEventTitle = baseEvent.GetComponentsInChildren<Transform>(true).Single(child =>
                child.name == "Img_Title");
            Assert.That(baseEventTitle.GetComponent<Image>(), Is.Not.Null);
            Assert.That(baseEventTitle.GetComponent<RectTransform>().anchoredPosition.y, Is.EqualTo(603f));
            Assert.That(AssetDatabase.GetDependencies(baseEventPath, true),
                Contains.Item("Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Top_title.png"));
            Assert.That(AssetDatabase.GetDependencies(baseEventPath, true),
                Contains.Item("Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Ui_timer.png"));

            Assert.That(match, Is.Not.Null, matchPath);
            Transform matchTitle = match.GetComponentsInChildren<Transform>(true).Single(child =>
                child.name == "Img_Title Variant");
            Image matchTitleImage = matchTitle.GetComponents<Image>().Single(component =>
                component.GetType() == typeof(Image));
            Assert.That(matchTitleImage.raycastTarget, Is.False);
            Assert.That(match.GetComponentsInChildren<Transform>(true).Any(child => child.name == "Txt_Timer"), Is.True);
            Assert.That(match.GetComponent<LavaRushScreenView>(), Is.Not.Null);
            Assert.That(AssetDatabase.GetDependencies(matchPath, true), Has.None.StartsWith("Assets/"));
        }

        [Test]
        public void BaseEventPrefab_HasOnePackageOwnerAndPreservesCanonicalConsumerVisuals()
        {
            const string originalGuid = "db969225b48c74c929a40f9143f44288";
            const string retiredCopyGuid = "b7ad343e3e4534bf8a290b0cbcd7792e";
            const string legacyPath =
                "Assets/_Project/Content/LavaRush/Prefabs/Base/UI_LavaRush_BaseEvent.prefab";
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab";
            const string ledgerPath =
                "Packages/com.actionfit.lava-rush.ui/Documentation~/AssetOwnership.json";
            string[] consumerPaths =
            {
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab",
            };

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(prefabPath), Is.EqualTo(originalGuid));

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, prefabPath);
            Assert.That(AssetDatabase.GetDependencies(prefabPath, true), Has.None.StartsWith("Assets/"));
            Assert.That(prefab.GetComponentsInChildren<Transform>(true), Has.Length.EqualTo(10));
            Assert.That(prefab.GetComponentsInChildren<Transform>(true).All(child => child.gameObject.activeSelf), Is.True);

            Transform panel = prefab.transform.Find("Img_Panel");
            Transform title = panel.Find("Img_Title");
            Transform description = panel.Find("Img_Desc");
            Transform button = panel.Find("Btn_EventStart");
            Transform buttonText = button.Find("Txt_EventStart");
            Transform timerText = title.Find("Img_Timer/Txt_Timer");
            Component titleTmp = title.Find("Txt_Title").GetComponents<Component>().Single(component =>
                component.GetType().Name == "TextMeshProUGUI");
            Component buttonTmp = buttonText.GetComponents<Component>().Single(component =>
                component.GetType().Name == "TextMeshProUGUI");

            AssertLocalFileIdentifier(prefab, originalGuid, 6912730226108308614L);
            AssertLocalFileIdentifier(prefab.transform, originalGuid, 3140789211477238392L);
            AssertLocalFileIdentifier(panel.gameObject, originalGuid, 3280440648099692891L);
            AssertLocalFileIdentifier(panel, originalGuid, 2932148738215472307L);
            AssertLocalFileIdentifier(panel.GetComponent<Image>(), originalGuid, 1280358180495745010L);
            AssertLocalFileIdentifier(description, originalGuid, 4925702008732976080L);
            AssertLocalFileIdentifier(description.GetComponent<Image>(), originalGuid, 3927699638381269058L);
            AssertLocalFileIdentifier(description.Find("Txt_Desc").GetComponent<UI_Text>(), originalGuid, 3345207925210735764L);
            AssertLocalFileIdentifier(title, originalGuid, 7295250248055179557L);
            AssertLocalFileIdentifier(titleTmp, originalGuid, 896903118523409931L);
            AssertLocalFileIdentifier(timerText.GetComponent<UI_Text>(), originalGuid, 1086859427771536626L);
            AssertLocalFileIdentifier(button.gameObject, originalGuid, 8155925728529109318L);
            AssertLocalFileIdentifier(button, originalGuid, 8365658277760941230L);
            AssertLocalFileIdentifier(button.GetComponent<Image>(), originalGuid, 5579223970474397679L);
            AssertLocalFileIdentifier(button.GetComponent<UI_Button>(), originalGuid, 736557205927443034L);
            AssertLocalFileIdentifier(buttonText.gameObject, originalGuid, 8793999869417706670L);
            AssertLocalFileIdentifier(buttonTmp, originalGuid, 6828813741718080478L);
            AssertLocalFileIdentifier(buttonText.GetComponent<UI_Text>(), originalGuid, 2175171421629307882L);

            var serializedButtonText = new SerializedObject(buttonText.GetComponent<UI_Text>());
            Assert.That(serializedButtonText.FindProperty("isSettingOutline").boolValue, Is.True);
            Assert.That(serializedButtonText.FindProperty("outlineColor").colorValue,
                Is.EqualTo(new Color(0.3787079f, 0.5660378f, 0.3337487f, 1f)));
            Assert.That(serializedButtonText.FindProperty("outlineWidth").floatValue, Is.EqualTo(0.1f));

            string packageYaml = File.ReadAllText(Path.GetFullPath(prefabPath));
            StringAssert.DoesNotContain(retiredCopyGuid, packageYaml);
            foreach (string staleLocalId in new[]
                     {
                         "775524696328212203",
                         "6923480937244319326",
                         "7723706689444764598",
                     })
            {
                StringAssert.DoesNotContain(staleLocalId, packageYaml,
                    "Legacy no-op overrides must remain unbound instead of changing runtime behavior.");
            }

            AssetOwnershipLedger ledger = JsonUtility.FromJson<AssetOwnershipLedger>(
                File.ReadAllText(Path.GetFullPath(ledgerPath)));
            AssetOwnershipEntry ownership = ledger.assets.Single(entry =>
                string.Equals(entry.packagePath, prefabPath, StringComparison.Ordinal));
            Assert.That(ownership.legacyPath, Is.EqualTo(legacyPath));
            Assert.That(ownership.guid, Is.EqualTo(originalGuid));
            Assert.That(ownership.sha256, Is.EqualTo(ComputeSha256Hex(Path.GetFullPath(prefabPath))));

            foreach (string consumerPath in consumerPaths)
            {
                GameObject consumer = AssetDatabase.LoadAssetAtPath<GameObject>(consumerPath);
                Assert.That(consumer, Is.Not.Null, consumerPath);
                Assert.That(AssetDatabase.GetDependencies(consumerPath, true), Has.None.StartsWith("Assets/"), consumerPath);
                Assert.That(consumer.GetComponent<LavaRushScreenView>(), Is.Not.Null, consumerPath);
                Transform nestedRoot = consumer.GetComponentsInChildren<Transform>(true).Single(child =>
                    child != consumer.transform && child.name == "UI_LavaRush_BaseEvent");
                Assert.That(nestedRoot.Find("Img_Panel"), Is.Not.Null, consumerPath);

                string consumerYaml = File.ReadAllText(Path.GetFullPath(consumerPath));
                StringAssert.DoesNotContain(retiredCopyGuid, consumerYaml, consumerPath);
            }
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
        public void Background_HasOnePackageOwnerAndBothMatchPrefabsResolveIt()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/BG.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/BG.png";
            const string projectPrefab = "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab";
            const string packagePrefab = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab";
            const string expectedGuid = "0f355d53e68f947c79a9800513063f75";

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
        public void LevelDifficultyEasy_HasOnePackageOwnerAndBothDifficultyPrefabsResolveIt()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/Level_difficulty_easy.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Level_difficulty_easy.png";
            const string projectPrefab = "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string packagePrefab = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string expectedGuid = "5e5e3299202e744b7ab2283ad63344e7";

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
        public void CanonicalDifficulty_UsesPopupTextboardForDescription()
        {
            const string prefabPath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string expectedSpritePath =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Popup_textboard.png";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null);

            Transform baseEvent = prefab.GetComponentsInChildren<Transform>(true).Single(child =>
                child != prefab.transform && child.name == "UI_LavaRush_BaseEvent");
            Image description = baseEvent.Find("Img_Panel/Img_Desc").GetComponent<Image>();

            Assert.That(description, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(description.sprite), Is.EqualTo(expectedSpritePath));
            Assert.That(AssetDatabase.GetDependencies(prefabPath, true), Contains.Item(expectedSpritePath));
        }

        [Test]
        public void LevelDifficultyNormal_HasOnePackageOwnerAndBothDifficultyPrefabsResolveIt()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/Level_difficulty_normal.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Level_difficulty_normal.png";
            const string projectPrefab = "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string packagePrefab = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string expectedGuid = "a22972e94407c4f5a8e5c264844b6d97";

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
        public void LevelDifficultyHard_HasOnePackageOwnerAndBothDifficultyPrefabsResolveIt()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/Level_difficulty_hard.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/Level_difficulty_hard.png";
            const string projectPrefab = "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string packagePrefab = "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            const string expectedGuid = "c717057e376d141d8a1c585f074e8c5d";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
            Assert.That(AssetDatabase.GetDependencies(packagePrefab, true), Contains.Item(packagePath));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(projectPrefab) != null)
            {
                Assert.That(AssetDatabase.GetDependencies(projectPrefab, true), Contains.Item(packagePath));
            }
        }

        [TestCase("Level_Select.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "2ac5671ecb31a487b82a7dc93d48ed17")]
        [TestCase("Level_board.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "6a178fe1a68c94758ac2edeaf8b67463")]
        [TestCase("Badge.png", "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab", "48d36a3e90cd64d7985660672ab3bf1c")]
        [TestCase("Bottom_board.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab", "7979f91217194423292e9b0057e7b6da")]
        [TestCase("Box_final.png", "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab", "d265ef6711d764293b59cc4a78043836")]
        [TestCase("Bridge.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab", "ce5c8009648e94ae5a0ecb5982fa19be")]
        [TestCase("Bridge_shadow.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab", "1f5f27c1ce46d4fdeb34793fca43a303")]
        [TestCase("Cat_person.png", "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab", "02cd48396bc1c432bafd32e487344918")]
        [TestCase("Chest_easy.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "3415f3d1b58764997a3b4276b970d6c5")]
        [TestCase("Chest_hard.png", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab", "f458ad39b8072464c8fba15e05804201")]
        public void ApprovedBatchImage_HasOnePackageOwnerAndBothPrefabsResolveIt(
            string imageName,
            string projectPrefab,
            string packagePrefab,
            string expectedGuid)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Images/resource/{imageName}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/{imageName}";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
            Assert.That(AssetDatabase.GetDependencies(packagePrefab, true), Contains.Item(packagePath));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(projectPrefab) != null)
            {
                Assert.That(AssetDatabase.GetDependencies(projectPrefab, true), Contains.Item(packagePath));
            }
        }

        [TestCase("Grand_board.png", "a83aeee66ae8f4909bd398cf95c600ea", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab")]
        [TestCase("btn_i.png", "600f7420b0a9345179e6d38c67a962f9", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Match.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab")]
        [TestCase("icon_lava.png", "21be725b242314973b9a3ec287081124", "Assets/_Data/_LavaRush/Resources/SO/LavaRushSO.asset;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Tutorial.prefab;Assets/_Project/Core/Profile/Prefabs/UI/RewardGroup.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Tutorial.prefab")]
        [TestCase("Chest_normal.png", "a5da00eac6e064655a48bad88c036f6d", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Tutorial.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Tutorial.prefab")]
        [TestCase("Popup_image_B.png", "91f9d25e744874ad5b70981a6cf12bc1", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("Reward_box_combined.png", "bcf58d8db20f345a0ab588b5debbc360", "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab")]
        [TestCase("Btn_yellow.png", "d3ff1a4f53e824b0dbad1583d17f623c", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("Lava_block.png", "82343840846c942fa83cc10da1618d2a", "Assets/_Project/Content/LavaRush/Prefabs/Base/Content_LavaBlock.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Content_LavaBlock.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("Popup_image_A.png", "a540c3b209b5847a8b0d35e5e2882fb6", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab")]
        [TestCase("Jewel.png", "39bab98363ac64a7b82be04cf0fef562", "Assets/_Project/Content/LavaRush/Prefabs/Icon/UI_LavaRush_Cell.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Tutorial.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Tutorial.prefab")]
        public void ApprovedConsumerBatchImage_HasOnePackageOwnerAndAllRecordedConsumersResolveIt(
            string imageName,
            string expectedGuid,
            string consumerPaths)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Images/resource/{imageName}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/{imageName}";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
            foreach (string consumerPath in consumerPaths.Split(';'))
            {
                if (AssetDatabase.LoadMainAssetAtPath(consumerPath) != null)
                {
                    Assert.That(AssetDatabase.GetDependencies(consumerPath, true), Contains.Item(packagePath), consumerPath);
                }
            }
        }

        [TestCase("resource/Btn_green.png", "8c611bf6ec8f04b279cee4014924fdaa", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("resource/Top_title.png", "5f38da4879f424f909e500db5032d476", "Assets/_Project/Content/LavaRush/Prefabs/Base/Img_Title Variant.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Img_Title Variant.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("resource/Ui_timer.png", "75c6bdaaa60e74d48bec66e1d54ba88a", "Assets/_Project/Content/LavaRush/Prefabs/Base/Img_Title Variant.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/Img_Title Variant.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("resource/Popup_textboard.png", "4950207656f024c8886ed0b9dcbb82a4", "Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Assets/_Project/Content/LavaRush/Prefabs/UI/UI_LavaRush_MatchWin.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Base/UI_LavaRush_BaseEvent.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_EventStart.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchEnd.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchLose.prefab;Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_MatchWin.prefab")]
        [TestCase("DP/004.png", "902c4d078c9bf44a19293143da6bc71e", "")]
        [TestCase("DP/002_1.png", "50ca68a9823cc4f6bb67b86669109437", "")]
        [TestCase("DP/003_2.png", "82bd24343fae74e16af0ecf9e0de33a2", "")]
        [TestCase("resource/Stack_bar.png", "1990ddb8b3aed43d48a64c3514b8c962", "Assets/_Project/Core/Profile/Prefabs/UI/RewardGroup.prefab")]
        [TestCase("resource/Stack_in.png", "6bb6ab173e40241fa80080bb2bee2e0f", "Assets/_Project/Core/Profile/Prefabs/UI/RewardGroup.prefab")]
        [TestCase("DP/001_1.png", "5b3af4571d05a4e1da8520b47983b84f", "")]
        public void ApprovedMixedFolderBatchImage_HasOnePackageOwnerAndAllRecordedConsumersResolveIt(
            string relativeImagePath,
            string expectedGuid,
            string consumerPaths)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Images/{relativeImagePath}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/{relativeImagePath}";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
            foreach (string consumerPath in consumerPaths.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(consumerPath)
                    && AssetDatabase.LoadMainAssetAtPath(consumerPath) != null)
                {
                    Assert.That(AssetDatabase.GetDependencies(consumerPath, true), Contains.Item(packagePath), consumerPath);
                }
            }
        }

        [Test]
        public void LavaRushPrefabs_DoNotDependOnFullScreenDpPreviews()
        {
            const string dpRoot = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/DP/";
            string[] searchRoots = new[]
            {
                "Assets/_Project/Content/LavaRush/Prefabs",
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs"
            }.Where(AssetDatabase.IsValidFolder).ToArray();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchRoots);

            string[] previewDependencies = prefabGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .SelectMany(path => AssetDatabase.GetDependencies(path, true)
                    .Where(dependency => dependency.StartsWith(dpRoot, StringComparison.Ordinal))
                    .Select(dependency => $"{path} -> {dependency}"))
                .OrderBy(dependency => dependency, StringComparer.Ordinal)
                .ToArray();

            Assert.That(previewDependencies, Is.Empty,
                "Production Lava Rush prefabs must not use full-screen DP preview captures as sprites.");
        }

        [TestCase("Colorcode/001_1_C.png", "61b8393d52f0949df9a00dcc5ed559cb")]
        [TestCase("Colorcode/001_2_C.png", "13b3e7e4eb2e649eea45d92e20c50817")]
        [TestCase("Colorcode/001_3_C.png", "fddc0621e21c8481a92788cf0d8443cf")]
        [TestCase("Colorcode/002_C.png", "096aed1fd3d244410a4dec80fbfc624e")]
        [TestCase("Colorcode/003_1_C.png", "f251dfe6252a7447d9843bab92ada3f5")]
        [TestCase("Colorcode/003_2_C.png", "ef93448046d8b438a857f4bb0114a574")]
        [TestCase("Colorcode/004_C.png", "b344d8c8a0fcc488f8afe617fcd0bd23")]
        [TestCase("DP/001_2.png", "a4869dbf9cc9144b7bcbf218642ba6f1")]
        [TestCase("DP/001_3.png", "f7be97be3168c4ed3abbdefebd097c21")]
        [TestCase("DP/002_2.png", "c2c0fe4960b6e4be2ae59b9dbb9c7f63")]
        public void ApprovedUnreferencedBaselineImage_HasOnePackageOwner(
            string relativeImagePath,
            string expectedGuid)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Images/{relativeImagePath}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/{relativeImagePath}";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
        }

        [TestCase("DP/003_1.png", "c0f85ca7499ee42fc89e05afe4cc391e")]
        [TestCase("resource/Reward_box_B.png", "b34c00e8727e24ce58976c0775590b6a")]
        [TestCase("resource/Reward_box_F.png", "0c295ec6cae024097af76362913f5509")]
        [TestCase("resource/Title_CN.png", "dcc9740a4135547388bf291985e52764")]
        [TestCase("resource/Title_EN.png", "54020325b2fac423dabd41139e1b29d8")]
        [TestCase("resource/Title_JP.png", "34a675b86f7364d8e85f9428b37326c9")]
        [TestCase("resource/Title_KR.png", "47bdec92a9b6c4970b9dd7b8fa151f0e")]
        [TestCase("resource/Title_TW.png", "48293e196caba4e7ebc9a50538146cb2")]
        [TestCase("resource/Tutorial_box.png", "63fcbbbf9af584116abdd65d82feb302")]
        [TestCase("resource/Tutorial_cha.png", "995adcebf68aa42cbbf584ebe4a2f5c2")]
        public void LatestApprovedUnreferencedBaselineImage_HasOnePackageOwner(
            string relativeImagePath,
            string expectedGuid)
        {
            string legacyPath = $"Assets/_Project/Content/LavaRush/Images/{relativeImagePath}";
            string packagePath = $"Packages/com.actionfit.lava-rush.ui/Runtime/Art/{relativeImagePath}";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
        }

        [Test]
        public void FinalApprovedBaselineImage_HasOnePackageOwner()
        {
            const string legacyPath = "Assets/_Project/Content/LavaRush/Images/resource/icon_i.png";
            const string packagePath = "Packages/com.actionfit.lava-rush.ui/Runtime/Art/resource/icon_i.png";

            Assert.That(File.Exists(Path.GetFullPath(legacyPath)), Is.False);
            Assert.That(File.Exists(Path.GetFullPath(legacyPath + ".meta")), Is.False);
            Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo("66b3a082678114e078c14a2fd39f5c4d"));
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
        public void Bootstrap_HiddenCanonicalPresentationDefersStateRenderUntilShow()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                Assert.That(instance, Is.Not.Null);
                LavaRushBootstrap bootstrap = instance.GetComponent<LavaRushBootstrap>();
                LavaRushPresentation presentation = instance.GetComponent<LavaRushPresentation>();
                LavaRushScreenView[] screens = instance.GetComponentsInChildren<LavaRushScreenView>(true);
                bootstrap.InitializeDefault(presentation);

                bootstrap.Hide();
                Assert.That(bootstrap.IsVisible, Is.False);
                Assert.That(screens.All(screen => !screen.gameObject.activeSelf), Is.True);

                Assert.That(bootstrap.Engine.TryStartEvent(), Is.True);
                Assert.That(bootstrap.IsVisible, Is.False);
                Assert.That(screens.All(screen => !screen.gameObject.activeSelf), Is.True);

                bootstrap.Show();
                Assert.That(bootstrap.IsVisible, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Difficulty));
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
        public void CanonicalProductionPrefab_StandaloneEngineFlowCanReachCompletion()
        {
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            const string path =
                "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                Assert.That(instance, Is.Not.Null);
                LavaRushBootstrap bootstrap = instance.GetComponent<LavaRushBootstrap>();
                LavaRushPresentation presentation = instance.GetComponent<LavaRushPresentation>();
                Assert.That(bootstrap, Is.Not.Null);
                Assert.That(presentation, Is.Not.Null);
                bootstrap.InitializeDefault(presentation);
                Click(presentation, "Primary");
                Click(presentation, "Primary");
                Click(presentation, "Primary");

                for (int stage = 1; stage <= 3; stage++)
                {
                    Click(presentation, "Primary");
                    Assert.That(bootstrap.Engine.IsStagePlaying, Is.True);
                    Assert.That(bootstrap.Engine.AddProgress(bootstrap.Engine.RequiredProgress),
                        Is.EqualTo(LavaRushResult.Win));

                    Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Result));
                    Click(presentation, "Primary");
                }

                Assert.That(bootstrap.Engine.AllStagesComplete, Is.True);
                Assert.That(bootstrap.Engine.State.FinalRewardClaimed, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(LavaRushUIScreen.Complete));
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

        private static void AssertLocalFileIdentifier(
            UnityEngine.Object asset,
            string expectedGuid,
            long expectedLocalId)
        {
            Assert.That(
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId),
                Is.True,
                asset.name);
            Assert.That(guid, Is.EqualTo(expectedGuid), asset.name);
            Assert.That(localId, Is.EqualTo(expectedLocalId), asset.name);
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

            string propertyPath = buttonName switch
            {
                "Primary" => "refs.production.primaryButton",
                "Secondary" => "refs.production.secondaryButton",
                "Tertiary" => "refs.production.tertiaryButton",
                _ => string.Empty,
            };
            LavaRushScreenView activeView = presentation.GetComponentsInChildren<LavaRushScreenView>(false)
                .SingleOrDefault();
            SerializedProperty authoredTarget = string.IsNullOrEmpty(propertyPath) || activeView == null
                ? null
                : new SerializedObject(activeView).FindProperty(propertyPath);
            if (authoredTarget?.objectReferenceValue is LavaRushActionTarget target)
            {
                Assert.That(target.gameObject.activeSelf, Is.True, $"{buttonName} is hidden.");
                UI_Button foundationButton = target.GetComponent<UI_Button>();
                Button uguiButton = target.GetComponent<Button>();
                if (foundationButton != null)
                {
                    foundationButton.OnPointerClick(null);
                }
                else
                {
                    Assert.That(uguiButton, Is.Not.Null);
                    Assert.That(uguiButton.interactable, Is.True, $"{buttonName} is disabled.");
                    uguiButton.onClick.Invoke();
                }
                return;
            }

            Assert.Fail($"Button '{buttonName}' was not available on the active presentation.");
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
