using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionFit.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushUITests
    {
        private const string PackageRoot = "Packages/com.actionfit.lava-rush.ui";
        private const string MainPrefabPath =
            PackageRoot + "/Runtime/Prefabs/Main/UI_LavaRush.prefab";

        private static readonly (string Source, string Guid)[] ControllerSources =
        {
            ("UI_LavaRush.cs", "b8f0708c402304a7087e43d4b930d12a"),
            ("UI_LavaRush_Cell.cs", "2db37422dd1864fefaa69ed775f9e807"),
            ("UI_LavaRush_Difficulty.cs", "6f91dfe1651694fb8bbc9b1dffef0ea4"),
            ("UI_LavaRush_EventEnd.cs", "0976452bde9274fd08702b5d05d1a7d2"),
            ("UI_LavaRush_EventStart.cs", "37bd621f275f1436694a779d1974b663"),
            ("UI_LavaRush_Icon.cs", "09c9e7a68eaea4eb5be61dff1f30f436"),
            ("UI_LavaRush_Match.cs", "d3c9b87dac7124afc998b27a8742837d"),
            ("UI_LavaRush_MatchEnd.cs", "8b224abe585d041b887134910671d9d4"),
            ("UI_LavaRush_MatchLose.cs", "d421df0b41b834ee795b12e63f348fe3"),
            ("UI_LavaRush_MatchTutorial.cs", "47c3b950b3fa4fd3979b79946363758d"),
            ("UI_LavaRush_MatchWin.cs", "fbbad9b8b8081415e97fc0fc05830ca0"),
            ("UI_LavaRush_OrderReward.cs", "477865a5341cf4b88888428c7e302dcb"),
        };

        private static readonly (string Prefab, Type Controller)[] StatePrefabs =
        {
            ("UI_LavaRush_EventStart.prefab", typeof(UI_LavaRush_EventStart)),
            ("UI_LavaRush_Difficulty.prefab", typeof(UI_LavaRush_Difficulty)),
            ("UI_LavaRush_Tutorial.prefab", typeof(LavaRushTutorialView)),
            ("UI_LavaRush_Match.prefab", typeof(UI_LavaRush_Match)),
            ("UI_LavaRush_MatchWin.prefab", typeof(UI_LavaRush_MatchWin)),
            ("UI_LavaRush_MatchLose.prefab", typeof(UI_LavaRush_MatchLose)),
            ("UI_LavaRush_MatchEnd.prefab", typeof(UI_LavaRush_MatchEnd)),
            ("UI_LavaRush_EventEnd.prefab", typeof(UI_LavaRush_EventEnd)),
        };

        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
                if (_objects[index] != null)
                    UnityEngine.Object.DestroyImmediate(_objects[index]);
            _objects.Clear();
            new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
        }

        [Test]
        public void OriginalControllerSources_MoveWithTheirExactGuidsAndNoLocalDuplicates()
        {
            foreach ((string source, string expectedGuid) in ControllerSources)
            {
                string packagePath = $"{PackageRoot}/Runtime/Controllers/{source}";
                string localPath = $"Assets/_Project/Content/LavaRush/Scripts/UI/{source}";

                Assert.That(File.Exists(Path.GetFullPath(packagePath)), Is.True, packagePath);
                Assert.That(AssetDatabase.AssetPathToGUID(packagePath), Is.EqualTo(expectedGuid));
                Assert.That(File.Exists(Path.GetFullPath(localPath)), Is.False, localPath);
                Assert.That(File.Exists(Path.GetFullPath(localPath + ".meta")), Is.False, localPath);
                Assert.That(AssetDatabase.FindAssets($"glob:\"{source}\""), Has.Length.EqualTo(1), source);
            }
        }

        [Test]
        public void CanonicalMain_UsesTheRestoredRootAndEightDirectAuthoredControllers()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<global::UI_LavaRush>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<LavaRushPresentation>(), Is.Null);
            Assert.That(prefab.GetComponent<LavaRushBootstrap>(), Is.Null);
            Assert.That(prefab.GetComponentInChildren<LavaRushFlowView>(true), Is.Null);
            Assert.That(prefab.GetComponentsInChildren<LavaRushControllerView>(true), Has.Length.EqualTo(8));
            Assert.That(AssetDatabase.GetDependencies(MainPrefabPath, true), Has.None.StartsWith("Assets/"));
        }

        [TestCaseSource(nameof(StatePrefabCases))]
        public void StatePrefab_SerializesItsDirectController(
            string prefabName,
            Type controllerType)
        {
            string path = $"{PackageRoot}/Runtime/Prefabs/UI/{prefabName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.That(prefab, Is.Not.Null, path);
            Component controller = prefab.GetComponent(controllerType);
            Assert.That(controller, Is.Not.Null, path);
            Assert.That(controller, Is.InstanceOf<LavaRushControllerView>());
            Assert.That(AssetDatabase.GetDependencies(path, true), Has.None.StartsWith("Assets/"));

            var serialized = new SerializedObject(controller);
            string renderPath = controllerType == typeof(LavaRushTutorialView)
                ? "refs.production.panel"
                : "controller.production.panel";
            Assert.That(serialized.FindProperty(renderPath)?.objectReferenceValue,
                Is.Not.Null,
                path);
        }

        [Test]
        public void IconAndCell_SerializeOriginalControllerShapes()
        {
            GameObject icon = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PackageRoot}/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab");
            GameObject cell = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PackageRoot}/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab");

            UI_LavaRush_Icon iconController = icon.GetComponent<UI_LavaRush_Icon>();
            UI_LavaRush_Cell cellController = cell.GetComponent<UI_LavaRush_Cell>();

            Assert.That(iconController, Is.Not.Null);
            Assert.That(iconController.refs.txtTimer, Is.Not.Null);
            Assert.That(iconController.refs.txtTimer.name, Is.EqualTo("Txt_Timer"));
            Assert.That(cellController, Is.Not.Null);
            Assert.That(cellController.refs.txtTimer, Is.Not.Null);
            Assert.That(cellController.refs.txtStatus, Is.Not.Null);
            Assert.That(cellController.refs.imgStatusGauge, Is.Not.Null);
            Assert.That(cellController.refs.imgTargetProgress, Is.Not.Null);
            Assert.That(cellController.refs.animIndicator, Is.Not.Null);
            Assert.That(cellController.refs.rectRemainText, Is.Not.Null);
            Assert.That(cellController.refs.txtRemainCount, Is.Not.Null);
            Assert.That(cellController.refs.animDuration, Is.EqualTo(0.3f));
        }

        [UnityTest]
        public IEnumerator CanonicalControllerFlow_UsesEngineStateAndKeepsExactlyOneScreenActive()
        {
            global::UI_LavaRush controller = CreateController();
            controller.refs.uiMatch.refs.winJumpDuration = 0f;
            controller.refs.uiMatch.refs.blockClearDuration = 0f;
            controller.refs.uiMatch.refs.rewardRevealHold = 0f;
            var bootstrapRoot = Track(new GameObject("Lava Rush Bootstrap Test"));
            LavaRushBootstrap bootstrap = bootstrapRoot.AddComponent<LavaRushBootstrap>();
            bootstrap.InitializeDefault(
                controller,
                restoreEngine: false,
                showOnInitialize: true);

            AssertActive(controller, typeof(UI_LavaRush_EventStart));

            controller.HandleAction(LavaRushUIAction.StartEvent);
            Assert.That(controller.Engine.IsEventStarted, Is.True);
            AssertActive(controller, typeof(UI_LavaRush_Difficulty));

            controller.HandleAction(LavaRushUIAction.SelectEasy);
            Assert.That(controller.Engine.SelectedDifficulty, Is.EqualTo(1));
            Assert.That(controller.refs.uiMatch.IsInTutorial, Is.True);
            AssertActive(controller, typeof(UI_LavaRush_Match));

            controller.HandleAction(LavaRushUIAction.CompleteTutorial);
            Assert.That(controller.Engine.TutorialDone, Is.True);
            AssertActive(controller, typeof(UI_LavaRush_Match));

            controller.HandleAction(LavaRushUIAction.StartStage);
            Assert.That(controller.Engine.IsStagePlaying, Is.True);

            controller.OpenTutorial();
            AssertActive(controller, typeof(LavaRushTutorialView));
            controller.HandleAction(LavaRushUIAction.CompleteTutorial);
            Assert.That(controller.Engine.TutorialDone, Is.True);
            AssertActive(controller, typeof(UI_LavaRush_Match));

            controller.DevForceLose();
            Assert.That(controller.Engine.PendingResult, Is.EqualTo(LavaRushResult.Lose));
            yield return null;
            AssertActive(controller, typeof(UI_LavaRush_MatchLose));

            controller.HandleAction(LavaRushUIAction.Close);
            Assert.That(ActiveScreens(controller), Is.Zero);
            controller.OpenMatchFlow();
            yield return null;
            AssertActive(controller, typeof(UI_LavaRush_MatchLose));

            controller.StartNextOrRetryStage();
            Assert.That(controller.Engine.IsStagePlaying, Is.True);
            while (!controller.Engine.AllStagesComplete)
            {
                controller.DevForceWin();
                yield return null;
                yield return null;

                if (controller.Engine.AllStagesComplete)
                {
                    Assert.That(controller.Engine.PendingResult, Is.EqualTo(LavaRushResult.None));
                    Assert.That(controller.Engine.PendingEnd, Is.True);
                    AssertActive(controller, typeof(UI_LavaRush_EventEnd));
                    break;
                }

                Assert.That(controller.Engine.PendingResult, Is.EqualTo(LavaRushResult.None));
                AssertActive(controller, typeof(UI_LavaRush_MatchWin));
                controller.StartNextOrRetryStage();
                Assert.That(controller.Engine.IsStagePlaying, Is.True);
            }

            controller.CloseActiveScreen();
            Assert.That(ActiveScreens(controller), Is.Zero);
            controller.OpenMatchFlow();
            AssertActive(controller, typeof(UI_LavaRush_EventEnd));

            controller.HandleAction(LavaRushUIAction.EndEvent);
            Assert.That(controller.Engine.IsEventStarted, Is.False);
            Assert.That(ActiveScreens(controller), Is.Zero);
        }

        [UnityTest]
        public IEnumerator FailedRewardClaim_RetainsPendingRewardUntilExplicitRetrySucceeds()
        {
            const string contentId = "lava-rush-ui-claim-retry-test";
            global::UI_LavaRush controller = CreateController();
            controller.refs.uiMatch.refs.winJumpDuration = 0f;
            controller.refs.uiMatch.refs.blockClearDuration = 0f;
            LavaRushEngine engine = CreateDemoEngine(contentId);
            bool rewardRuntimeAvailable = false;
            controller.Initialize(
                new LavaRushControllerContext(
                    engine,
                    claimPendingReward: _ =>
                        rewardRuntimeAvailable && engine.ClaimPendingReward()),
                restoreEngine: false);
            controller.gameObject.SetActive(true);
            controller.OpenMatchFlow();

            controller.HandleAction(LavaRushUIAction.StartEvent);
            controller.OnTutorialComplete();
            controller.StartMatch(1);
            controller.DevForceWin();
            yield return null;
            yield return null;

            int rewardStage = controller.CurrentStage;
            Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.Win));
            Assert.That(engine.IsStageRewardClaimed(rewardStage), Is.False);
            AssertActive(controller, typeof(UI_LavaRush_MatchWin));

            controller.StartNextOrRetryStage();
            Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.Win));
            Assert.That(engine.IsStageRewardClaimed(rewardStage), Is.False);
            Assert.That(engine.IsStagePlaying, Is.False);
            AssertActive(controller, typeof(UI_LavaRush_MatchWin));

            rewardRuntimeAvailable = true;
            controller.StartNextOrRetryStage();
            Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.None));
            Assert.That(engine.IsStageRewardClaimed(rewardStage), Is.True);
            AssertActive(controller, typeof(UI_LavaRush_MatchWin));

            controller.StartNextOrRetryStage();
            Assert.That(engine.IsStagePlaying, Is.True);
            AssertActive(controller, typeof(UI_LavaRush_Match));
            new PlayerPrefsContentStateStore().Delete(contentId);
        }

        [UnityTest]
        public IEnumerator ClosingDuringResultAnimation_ReleasesResolverAndReplaysPendingResult()
        {
            const string contentId = "lava-rush-ui-result-cancel-test";
            global::UI_LavaRush controller = CreateController();
            controller.refs.uiMatch.refs.winJumpDuration = 1f;
            controller.refs.uiMatch.refs.blockClearDuration = 1f;
            LavaRushEngine engine = CreateDemoEngine(contentId);
            controller.Initialize(new LavaRushControllerContext(engine), restoreEngine: false);
            controller.gameObject.SetActive(true);
            controller.OpenMatchFlow();

            controller.HandleAction(LavaRushUIAction.StartEvent);
            controller.OnTutorialComplete();
            controller.StartMatch(1);
            controller.DevForceWin();
            Assert.That(controller.refs.uiMatch.IsPlayingResult, Is.True);

            controller.CloseActiveScreen();
            Assert.That(ActiveScreens(controller), Is.Zero);
            Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.Win));
            Assert.That(controller.refs.uiMatch.IsPlayingResult, Is.False);

            controller.refs.uiMatch.refs.winJumpDuration = 0f;
            controller.refs.uiMatch.refs.blockClearDuration = 0f;
            controller.OpenMatchFlow();
            for (int frame = 0;
                 frame < 10 && controller.refs.uiMatch.IsPlayingResult;
                 frame++)
            {
                yield return null;
            }

            Assert.That(controller.refs.uiMatch.IsPlayingResult, Is.False);
            Assert.That(engine.PendingResult, Is.EqualTo(LavaRushResult.None));
            AssertActive(controller, typeof(UI_LavaRush_MatchWin));
            new PlayerPrefsContentStateStore().Delete(contentId);
        }

        [Test]
        public void RuntimeContainsNoRetiredGeneratedProductionPath()
        {
            string runtime = Path.GetFullPath($"{PackageRoot}/Runtime");
            string[] files = Directory.GetFiles(runtime, "*", SearchOption.AllDirectories);
            Assert.That(files.Select(Path.GetFileName), Has.None.EqualTo("LavaRushScreenView.cs"));
            Assert.That(files.Select(Path.GetFileName), Has.None.EqualTo("LavaRushUIViewModel.cs"));

            foreach (string prefab in Directory.GetFiles(runtime, "*.prefab", SearchOption.AllDirectories))
            {
                string yaml = File.ReadAllText(prefab);
                Assert.That(yaml, Does.Not.Contain("LavaRushScreenView"), prefab);
                Assert.That(yaml, Does.Not.Contain("LavaRushUIViewModel"), prefab);
                Assert.That(yaml, Does.Not.Contain("PackageFlow"), prefab);
            }
        }

        [Test]
        public void ProductionInventory_RemainsFourteenPrefabsAndFiftySixPngs()
        {
            string prefabRoot = Path.GetFullPath($"{PackageRoot}/Runtime/Prefabs");
            string artRoot = Path.GetFullPath($"{PackageRoot}/Runtime/Art");
            string[] production =
            {
                "Base/Content_LavaBlock.prefab",
                "Base/Img_Title Variant.prefab",
                "Base/UI_LavaRush_BaseEvent.prefab",
                "Icon/UI_LavaRush_Cell.prefab",
                "Icon/UI_LavaRush_Icon.prefab",
                "Main/UI_LavaRush.prefab",
                "UI/UI_LavaRush_Difficulty.prefab",
                "UI/UI_LavaRush_EventEnd.prefab",
                "UI/UI_LavaRush_EventStart.prefab",
                "UI/UI_LavaRush_Match.prefab",
                "UI/UI_LavaRush_MatchEnd.prefab",
                "UI/UI_LavaRush_MatchLose.prefab",
                "UI/UI_LavaRush_MatchWin.prefab",
                "UI/UI_LavaRush_Tutorial.prefab",
            };

            Assert.That(production, Has.Length.EqualTo(14));
            foreach (string role in production)
            {
                string path = Path.Combine(prefabRoot, role);
                Assert.That(File.Exists(path), Is.True, role);
            }
            Assert.That(Directory.GetFiles(artRoot, "*.png", SearchOption.AllDirectories),
                Has.Length.EqualTo(56));
        }

        [Test]
        public void StandaloneDemo_ReferencesCanonicalControllerWithoutPresentationPrefab()
        {
            const string demoPath =
                PackageRoot + "/Runtime/Prefabs/LavaRushDemo.prefab";
            string yaml = File.ReadAllText(Path.GetFullPath(demoPath));
            string mainGuid = AssetDatabase.AssetPathToGUID(MainPrefabPath);

            Assert.That(yaml, Does.Contain("controllerPrefab:"));
            Assert.That(yaml, Does.Contain($"guid: {mainGuid}"));
            Assert.That(yaml, Does.Not.Contain("presentationPrefab:"));
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(
                PackageRoot + "/Runtime/Prefabs/LavaRushPresentation.prefab"), Is.Null);
        }

        [Test]
        public void CatDetectiveSample_ReferencesCanonicalControllerWithoutPresentationPrefab()
        {
            const string samplePath =
                PackageRoot + "/Samples~/CatDetective Starter/Prefabs/UI_LavaRush.prefab";
            string yaml = File.ReadAllText(Path.GetFullPath(samplePath));
            string mainGuid = AssetDatabase.AssetPathToGUID(MainPrefabPath);

            Assert.That(yaml, Does.Contain("controllerPrefab:"));
            Assert.That(yaml, Does.Contain($"guid: {mainGuid}"));
            Assert.That(yaml, Does.Not.Contain("presentationPrefab:"));
        }

        [Test]
        public void PackagePrefabs_HaveNoMissingScripts()
        {
            if (!ExternalVisualEffectDependenciesAvailable())
                return;

            string[] guids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { PackageRoot + "/Runtime/Prefabs" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                    Assert.That(
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject),
                        Is.Zero,
                        $"{path}: {child.name}");
            }
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

        private static IEnumerable<TestCaseData> StatePrefabCases()
        {
            foreach ((string prefab, Type controller) in StatePrefabs)
                yield return new TestCaseData(prefab, controller).SetName(
                    $"StatePrefab_{Path.GetFileNameWithoutExtension(prefab)}");
        }

        private global::UI_LavaRush CreateController()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);
            GameObject instance = Track(PrefabUtility.InstantiatePrefab(prefab) as GameObject);
            Assert.That(instance, Is.Not.Null);
            return instance.GetComponent<global::UI_LavaRush>();
        }

        private static LavaRushEngine CreateDemoEngine(string contentId)
        {
            var clock = new LavaRushDemoClock();
            return new LavaRushEngine(
                new PlayerPrefsContentStateStore(),
                new PlayerPrefsContentRewardService(
                    $"com.actionfit.lava-rush.ui.tests.{contentId}.rewards"),
                new LavaRushDemoCatalogResolver(),
                clock,
                TimeZoneInfo.Local,
                clock,
                new SystemLavaRushRandom(),
                new LinearLavaRushSeatCurveProvider(),
                contentId,
                new AllowLavaRushAccessPolicy(),
                new LavaRushDemoSchedulePolicy());
        }

        private GameObject Track(GameObject value)
        {
            _objects.Add(value);
            return value;
        }

        private static int ActiveScreens(global::UI_LavaRush controller) =>
            controller.GetComponentsInChildren<LavaRushControllerView>(true)
                .Count(screen => screen.gameObject.activeSelf);

        private static void AssertActive(global::UI_LavaRush controller, Type expected)
        {
            LavaRushControllerView[] screens =
                controller.GetComponentsInChildren<LavaRushControllerView>(true);
            Assert.That(screens.Count(screen => screen.gameObject.activeSelf), Is.EqualTo(1));
            Assert.That(screens.Single(screen => screen.gameObject.activeSelf).GetType(),
                Is.EqualTo(expected));
        }
    }
}
