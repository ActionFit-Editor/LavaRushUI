using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ActionFit.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushMatchControllerParityTests
    {
        private const string ControllerPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Controllers/UI_LavaRush_Match.cs";
        private const string TutorialControllerPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Controllers/UI_LavaRush_MatchTutorial.cs";
        private const string MainPrefabPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Main/UI_LavaRush.prefab";
        private const string PrefabPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/UI/UI_LavaRush_Match.prefab";

        private static readonly string[] OriginalRefNames =
        {
            "lavaBlock",
            "content",
            "verContent",
            "scrollLavaBlock",
            "imgBridge",
            "imgShadow",
            "imgReward",
            "profileGroup",
            "introEnemyCount",
            "linearProfileSeats",
            "enemyGroupPrefab",
            "scrollSpeed",
            "rewardRevealHold",
            "blockClearDuration",
            "blockClearDropY",
            "blockClearShakeAngle",
            "winJumpPower",
            "winJumpDuration",
            "bridgeTopRatio",
            "rewardTopRatio",
            "btnClose",
            "btnInfo",
            "btnStart",
            "txtTimer",
            "gridRewardPanel",
            "rectStart",
            "rectMatchTutorial",
            "imgTutorialPrefab",
            "tutorial1",
            "tutorial2",
            "tutorial3",
            "txtTutorialDesc1",
            "txtTutorialDesc2",
            "txtTutorialDesc3",
            "tutorialScrollDuration",
        };

        [Test]
        public void Refs_PreserveOriginalFlatNamesAndNeutralizeProjectOnlyTypes()
        {
            string[] actual = typeof(UI_LavaRush_Match.Refs)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(field => field.Name)
                .ToArray();

            Assert.That(actual, Is.EqualTo(OriginalRefNames));
            Assert.That(
                typeof(UI_LavaRush_Match.Refs).GetField("profileGroup")?.FieldType,
                Is.EqualTo(typeof(MonoBehaviour)));
            Assert.That(
                typeof(UI_LavaRush_Match.Refs).GetField("enemyGroupPrefab")?.FieldType,
                Is.EqualTo(typeof(MonoBehaviour)));
            Assert.That(
                typeof(UI_LavaRush_Match.Refs).GetField("gridRewardPanel")?.FieldType,
                Is.EqualTo(typeof(LavaRushRewardGridView)));
        }

        [Test]
        public void Controller_KeepsGenericRendererSeparateAndExposesResultMilestones()
        {
            FieldInfo controller = typeof(UI_LavaRush_Match).GetField(
                "controller",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.FieldType, Is.EqualTo(typeof(LavaRushControllerRefs)));
            Assert.That(controller.GetCustomAttribute<SerializeField>(), Is.Not.Null);
            Assert.That(
                typeof(UI_LavaRush_Match).GetMethod(
                    "PlayWinResult",
                    new[] { typeof(Action), typeof(Action) }),
                Is.Not.Null);
            Assert.That(
                typeof(UI_LavaRush_Match).GetMethod(
                    "PlayLoseResult",
                    new[] { typeof(Action), typeof(Action) }),
                Is.Not.Null);
            Assert.That(
                typeof(UI_LavaRush_Match).GetMethod(
                    "GetStageRewardWorldPos",
                    new[] { typeof(int) }),
                Is.Not.Null);
        }

        [Test]
        public void Prefab_MapsOriginalAuthoredReferencesAndGenericRenderer()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            UI_LavaRush_Match match = prefab.GetComponent<UI_LavaRush_Match>();
            Assert.That(match, Is.Not.Null);

            var serialized = new SerializedObject(match);
            string[] requiredOriginalReferences =
            {
                "refs.lavaBlock",
                "refs.content",
                "refs.verContent",
                "refs.scrollLavaBlock",
                "refs.imgBridge",
                "refs.imgShadow",
                "refs.imgReward",
                "refs.btnClose",
                "refs.btnInfo",
                "refs.btnStart",
                "refs.txtTimer",
                "refs.gridRewardPanel",
                "refs.rectStart",
                "refs.rectMatchTutorial",
                "refs.tutorial1",
                "refs.tutorial2",
                "refs.tutorial3",
                "refs.txtTutorialDesc1",
                "refs.txtTutorialDesc2",
                "refs.txtTutorialDesc3",
            };
            for (int index = 0; index < requiredOriginalReferences.Length; index++)
            {
                SerializedProperty property = serialized.FindProperty(requiredOriginalReferences[index]);
                Assert.That(property, Is.Not.Null, requiredOriginalReferences[index]);
                Assert.That(
                    property.objectReferenceValue,
                    Is.Not.Null,
                    requiredOriginalReferences[index]);
            }

            Assert.That(
                serialized.FindProperty("controller.production.panel").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serialized.FindProperty("controller.production.primaryButton").objectReferenceValue,
                Is.Not.Null);
        }

        [Test]
        public void Source_UsesNeutralPortsAndDeterministicCoroutineMilestones()
        {
            string source = File.ReadAllText(Path.GetFullPath(ControllerPath));
            string tutorialSource = File.ReadAllText(Path.GetFullPath(TutorialControllerPath));

            StringAssert.DoesNotContain("Main.", source);
            StringAssert.DoesNotContain("DG.Tweening", source);
            StringAssert.DoesNotContain("DOTween", source);
            StringAssert.DoesNotContain("Cysharp.Threading.Tasks", source);
            StringAssert.DoesNotContain("UniTask", source);
            StringAssert.Contains("Owner?.ProfileRoster", source);
            StringAssert.Contains("Owner.FrameScheduler?.SubscribeUpdate", source);
            StringAssert.Contains("LavaRushAudioCue.WinJump", source);
            StringAssert.Contains("LavaRushAudioCue.BlockClear", source);
            StringAssert.Contains("yield return AnimateBlockClear(previousBlock)", source);
            StringAssert.Contains("onCompleted?.Invoke()", source);
            StringAssert.Contains("LavaRushBlockView block = BlockAtStage(stage)", source);
            StringAssert.Contains("return block.RewardWorldPosition", source);
            StringAssert.Contains("Owner?.ProfileGroupFactory", source);
            StringAssert.DoesNotContain("Resources.Load", tutorialSource);
        }

        [UnityTest]
        public IEnumerator ProfileFactory_CreatesBindsCancelsAndCleansUpPlayerAndOpponents()
        {
            GameObject instance = null;
            GameObject bootstrapRoot = null;
            var factory = new LavaRushProfileGroupFactoryProbe();
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Assert.That(instance, Is.Not.Null);
                global::UI_LavaRush controller = instance.GetComponent<global::UI_LavaRush>();
                Assert.That(controller, Is.Not.Null);

                bootstrapRoot = new GameObject("Lava Rush Profile Factory Bootstrap");
                LavaRushBootstrap bootstrap = bootstrapRoot.AddComponent<LavaRushBootstrap>();
                bootstrap.InitializeDefault(
                    controller,
                    restoreEngine: false,
                    showOnInitialize: true,
                    profileGroupFactory: factory);

                controller.HandleAction(LavaRushUIAction.StartEvent);
                controller.HandleAction(LavaRushUIAction.SelectEasy);
                controller.refs.uiMatch.PlayProfileIntro();
                yield return null;

                Assert.That(factory.PlayerCreated, Is.EqualTo(1));
                Assert.That(factory.OpponentCreated, Is.GreaterThanOrEqualTo(1));
                Assert.That(factory.PlayerBound, Is.GreaterThanOrEqualTo(1));
                Assert.That(factory.OpponentBound, Is.GreaterThanOrEqualTo(
                    factory.OpponentCreated));

                int created = factory.PlayerCreated + factory.OpponentCreated;
                controller.HandleAction(LavaRushUIAction.Close);
                MethodInfo onDisable = typeof(UI_LavaRush_Match).GetMethod(
                    "OnDisable",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(onDisable, Is.Not.Null);
                onDisable.Invoke(controller.refs.uiMatch, null);
                Assert.That(factory.Canceled, Is.GreaterThanOrEqualTo(created));

                MethodInfo onDestroy = typeof(UI_LavaRush_Match).GetMethod(
                    "OnDestroy",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(onDestroy, Is.Not.Null);
                onDestroy.Invoke(controller.refs.uiMatch, null);
                UnityEngine.Object.DestroyImmediate(instance);
                instance = null;
                Assert.That(factory.AliveCount, Is.Zero);
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance);
                if (bootstrapRoot != null)
                    UnityEngine.Object.DestroyImmediate(bootstrapRoot);
                new PlayerPrefsContentStateStore().Delete(
                    LavaRushBootstrap.DefaultDemoContentId);
            }
        }
    }

    internal sealed class LavaRushProfileGroupFactoryProbe : ILavaRushProfileGroupFactory
    {
        private readonly System.Collections.Generic.List<MonoBehaviour> _views = new();

        public int PlayerCreated { get; private set; }
        public int OpponentCreated { get; private set; }
        public int PlayerBound { get; private set; }
        public int OpponentBound { get; private set; }
        public int Canceled { get; private set; }
        public int AliveCount => _views.Count(view => view != null);

        public MonoBehaviour CreatePlayerProfileGroup(Transform parent)
        {
            PlayerCreated++;
            return Create("Player Profile Probe", parent);
        }

        public MonoBehaviour CreateOpponentProfileGroup(int stage, Transform parent)
        {
            OpponentCreated++;
            return Create($"Opponent Profile Probe {stage}", parent);
        }

        internal void RecordPlayerBound() => PlayerBound++;
        internal void RecordOpponentBound() => OpponentBound++;
        internal void RecordCanceled() => Canceled++;

        private LavaRushProfileGroupViewProbe Create(string name, Transform parent)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            LavaRushProfileGroupViewProbe view =
                root.AddComponent<LavaRushProfileGroupViewProbe>();
            view.Initialize(this);
            _views.Add(view);
            return view;
        }
    }

    internal sealed class LavaRushProfileGroupViewProbe :
        MonoBehaviour,
        ILavaRushProfileGroupView
    {
        private LavaRushProfileGroupFactoryProbe _factory;

        internal void Initialize(LavaRushProfileGroupFactoryProbe factory) =>
            _factory = factory;

        public void BindPlayer(LavaRushProfileSnapshot profile) =>
            _factory.RecordPlayerBound();

        public void BindOpponent(int slot, LavaRushProfileSnapshot profile) =>
            _factory.RecordOpponentBound();

        public void HidePlayer()
        {
        }

        public void ShowPlayerOnly()
        {
        }

        public void HideAll()
        {
        }

        public void SetOpponentCount(int count, bool animate, Action onAppear = null)
        {
        }

        public void PlayPlayerAppear(
            float delay,
            Action onComplete = null,
            Action onAppear = null)
        {
        }

        public void CancelAnimations() => _factory.RecordCanceled();
    }
}
