using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ActionFit.Content;
using NUnit.Framework;
using ReferenceBinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushStateControllerParityTests
    {
        private const string PackageRoot = "Packages/com.actionfit.lava-rush.ui";
        private const string MainPrefabPath =
            PackageRoot + "/Runtime/Prefabs/Main/UI_LavaRush.prefab";

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

        [TestCase(typeof(UI_LavaRush_EventStart), typeof(UI_LavaRush_EventStart.Refs))]
        [TestCase(typeof(UI_LavaRush_Difficulty), typeof(UI_LavaRush_Difficulty.Refs))]
        [TestCase(typeof(UI_LavaRush_EventEnd), typeof(UI_LavaRush_EventEnd.Refs))]
        [TestCase(typeof(UI_LavaRush_MatchWin), typeof(UI_LavaRush_MatchWin.Refs))]
        [TestCase(typeof(UI_LavaRush_MatchLose), typeof(UI_LavaRush_MatchLose.Refs))]
        [TestCase(typeof(UI_LavaRush_MatchEnd), typeof(UI_LavaRush_MatchEnd.Refs))]
        public void StateController_KeepsFlatOriginalRefsAndSeparateGenericController(
            Type controllerType,
            Type refsType)
        {
            FieldInfo refs = controllerType.GetField("refs", BindingFlags.Instance | BindingFlags.Public);
            FieldInfo controller = controllerType.GetField(
                "controller",
                BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo genericActions = controllerType.GetProperty(
                "BindGenericActions",
                BindingFlags.Instance | BindingFlags.NonPublic);
            GameObject root = Track(new GameObject(controllerType.Name));
            var instance = (LavaRushControllerView)root.AddComponent(controllerType);

            Assert.That(refs, Is.Not.Null);
            Assert.That(refs.FieldType, Is.EqualTo(refsType));
            Assert.That(refsType.BaseType, Is.EqualTo(typeof(object)));
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.FieldType, Is.EqualTo(typeof(LavaRushControllerRefs)));
            Assert.That(controller.GetCustomAttribute<SerializeField>(), Is.Not.Null);
            Assert.That(genericActions, Is.Not.Null);
            Assert.That(genericActions.GetValue(instance), Is.False);
        }

        [Test]
        public void OriginalRefs_KeepExactFieldNamesAndPackageSafeTypes()
        {
            AssertFields(
                typeof(UI_LavaRush_EventStart.Refs),
                ("btnStart", typeof(UI_Button)),
                ("txtTimer", typeof(UI_Text)));
            AssertFields(
                typeof(UI_LavaRush_Difficulty.DifficultyOption),
                ("btnDifficulty", typeof(UI_Button)),
                ("imgEffect", typeof(UI_Image)));
            AssertFields(
                typeof(UI_LavaRush_Difficulty.Refs),
                ("easy", typeof(UI_LavaRush_Difficulty.DifficultyOption)),
                ("normal", typeof(UI_LavaRush_Difficulty.DifficultyOption)),
                ("hard", typeof(UI_LavaRush_Difficulty.DifficultyOption)),
                ("btnStart", typeof(UI_Button)),
                ("gridRewardPanel", typeof(LavaRushRewardGridView)),
                ("imgRewardPanel", typeof(UI_Image)),
                ("txtTimer", typeof(UI_Text)),
                ("mask", typeof(UI_Mask)));
            AssertFields(
                typeof(UI_LavaRush_EventEnd.Refs),
                ("btnConfirm", typeof(UI_Button)),
                ("txtTimer", typeof(UI_Text)));
            AssertFields(
                typeof(UI_LavaRush_MatchWin.Refs),
                ("btnMatchStart", typeof(UI_Button)),
                ("btnMatchLater", typeof(UI_Button)),
                ("btnClose", typeof(UI_Button)),
                ("txtTimer", typeof(UI_Text)),
                ("txtRank", typeof(UI_Text)));
            AssertFields(
                typeof(UI_LavaRush_MatchLose.Refs),
                ("btnMatchStart", typeof(UI_Button)),
                ("btnMatchLater", typeof(UI_Button)),
                ("btnClose", typeof(UI_Button)),
                ("txtTimer", typeof(UI_Text)),
                ("localizeDesc2", typeof(UI_Text)));
            AssertFields(
                typeof(UI_LavaRush_MatchEnd.Refs),
                ("btnConfirm", typeof(UI_Button)),
                ("btnClose", typeof(UI_Button)));
        }

        [Test]
        public void CompatibilityAttributes_KeepDifficultyRenamesAndRequiredEventEndTimer()
        {
            FieldInfo normal = typeof(UI_LavaRush_Difficulty.Refs).GetField("normal");
            FieldInfo hard = typeof(UI_LavaRush_Difficulty.Refs).GetField("hard");
            FieldInfo timer = typeof(UI_LavaRush_EventEnd.Refs).GetField(
                "txtTimer",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(
                normal.GetCustomAttribute<FormerlySerializedAsAttribute>().oldName,
                Is.EqualTo("hard"));
            Assert.That(
                hard.GetCustomAttribute<FormerlySerializedAsAttribute>().oldName,
                Is.EqualTo("veryHard"));
            Assert.That(timer.GetCustomAttribute<SerializeField>(), Is.Not.Null);
            Assert.That(
                timer.GetCustomAttribute<RequiredReferenceAttribute>().ErrorCode,
                Is.EqualTo("LAVA_RUSH_EVENT_END_TIMER_MISSING"));
            Assert.That(
                timer.GetCustomAttribute<AutoWireChildAttribute>().ObjectName,
                Is.EqualTo("Txt_Timer"));
        }

        [TestCase(
            "UI_LavaRush_EventStart.prefab",
            typeof(UI_LavaRush_EventStart),
            "refs.btnStart",
            "refs.txtTimer")]
        [TestCase(
            "UI_LavaRush_Difficulty.prefab",
            typeof(UI_LavaRush_Difficulty),
            "refs.easy.btnDifficulty",
            "refs.easy.imgEffect",
            "refs.normal.btnDifficulty",
            "refs.normal.imgEffect",
            "refs.hard.btnDifficulty",
            "refs.hard.imgEffect",
            "refs.btnStart",
            "refs.gridRewardPanel",
            "refs.imgRewardPanel",
            "refs.txtTimer",
            "refs.mask")]
        [TestCase(
            "UI_LavaRush_MatchWin.prefab",
            typeof(UI_LavaRush_MatchWin),
            "refs.btnMatchStart",
            "refs.btnMatchLater",
            "refs.btnClose",
            "refs.txtTimer",
            "refs.txtRank")]
        [TestCase(
            "UI_LavaRush_MatchLose.prefab",
            typeof(UI_LavaRush_MatchLose),
            "refs.btnMatchStart",
            "refs.btnMatchLater",
            "refs.btnClose",
            "refs.txtTimer",
            "refs.localizeDesc2")]
        [TestCase(
            "UI_LavaRush_MatchEnd.prefab",
            typeof(UI_LavaRush_MatchEnd),
            "refs.btnConfirm",
            "refs.btnClose")]
        public void StatePrefab_MapsOriginalRefsAndKeepsGenericRenderBindings(
            string prefabName,
            Type controllerType,
            params string[] requiredRefs)
        {
            string path = $"{PackageRoot}/Runtime/Prefabs/UI/{prefabName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Component controller = prefab.GetComponent(controllerType);
            var serialized = new SerializedObject(controller);

            Assert.That(controller, Is.Not.Null, path);
            foreach (string propertyPath in requiredRefs)
            {
                SerializedProperty property = serialized.FindProperty(propertyPath);
                Assert.That(property, Is.Not.Null, $"{path}: {propertyPath}");
                Assert.That(property.objectReferenceValue, Is.Not.Null, $"{path}: {propertyPath}");
            }

            Assert.That(
                serialized.FindProperty("controller.production.panel")?.objectReferenceValue,
                Is.Not.Null,
                path);
            Assert.That(AssetDatabase.GetDependencies(path, true), Has.None.StartsWith("Assets/"));
        }

        [Test]
        public void EventEndPrefab_MapsConfirmAndRequiredTimer()
        {
            const string path =
                PackageRoot + "/Runtime/Prefabs/UI/UI_LavaRush_EventEnd.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            UI_LavaRush_EventEnd controller = prefab.GetComponent<UI_LavaRush_EventEnd>();
            var serialized = new SerializedObject(controller);

            Assert.That(serialized.FindProperty("refs.btnConfirm").objectReferenceValue, Is.Not.Null);
            UnityEngine.Object timer =
                serialized.FindProperty("refs.txtTimer").objectReferenceValue;
            Assert.That(timer, Is.Not.Null);
            Assert.That(
                serialized.FindProperty("controller.production.timerText").objectReferenceValue,
                Is.SameAs(timer));
        }

        [Test]
        public void DirectButtons_DriveNeutralOwnerFlowWithoutGenericDoubleDispatch()
        {
            global::UI_LavaRush owner = CreateController();
            var bootstrapRoot = Track(new GameObject("Lava Rush State Parity Bootstrap"));
            LavaRushBootstrap bootstrap = bootstrapRoot.AddComponent<LavaRushBootstrap>();
            bootstrap.InitializeDefault(owner, restoreEngine: false, showOnInitialize: true);

            InvokeClick(owner.refs.uiEventStart.refs.btnStart);
            Assert.That(owner.Engine.IsEventStarted, Is.True);
            Assert.That(owner.refs.uiDifficulty.gameObject.activeSelf, Is.True);

            InvokeClick(owner.refs.uiDifficulty.refs.easy.btnDifficulty);
            Assert.That(owner.refs.uiDifficulty.SelectedDifficulty, Is.Zero);
            Assert.That(owner.refs.uiDifficulty.refs.btnStart.IsDisabled, Is.False);

            InvokeClick(owner.refs.uiDifficulty.refs.btnStart);
            Assert.That(owner.Engine.SelectedDifficulty, Is.EqualTo(1));

            owner.CloseActiveScreen();
            owner.refs.uiMatchWin.gameObject.SetActive(true);
            InvokeClick(owner.refs.uiMatchWin.refs.btnClose);
            Assert.That(ActiveScreens(owner), Is.Zero);

            owner.OpenMatchFlow();
            owner.refs.uiMatchLose.gameObject.SetActive(true);
            InvokeClick(owner.refs.uiMatchLose.refs.btnMatchLater);
            Assert.That(owner.refs.uiMatch.gameObject.activeSelf, Is.True);
            Assert.That(ActiveScreens(owner), Is.EqualTo(1));
        }

        [Test]
        public void TutorialHelper_KeepsEnumValuesClickGateAndLifecycleCallbacks()
        {
            Assert.That((int)TutorialFocusSprite.Default, Is.Zero);
            Assert.That((int)TutorialFocusSprite.Circle, Is.EqualTo(1));
            Assert.That(
                Enum.GetValues(typeof(TutorialFocusSprite)).Cast<TutorialFocusSprite>(),
                Is.EqualTo(new[] { TutorialFocusSprite.Default, TutorialFocusSprite.Circle }));

            UI_LavaRush_Match match = CreateTutorialMatch();
            var tutorial = new UI_LavaRush_MatchTutorial(match);
            var shown = new List<int>();
            int completed = 0;
            tutorial.OnStepShown += shown.Add;
            tutorial.OnGuideCompleted += () => completed++;
            SetPrivateField(tutorial, "_willStart", true);

            Assert.That(tutorial.StartGuide(), Is.True);
            Assert.That(tutorial.IsActive, Is.True);
            Assert.That(shown, Is.EqualTo(new[] { 0 }));
            Assert.That(match.refs.tutorial1.gameObject.activeSelf, Is.True);

            tutorial.UpdateClick(false);
            Assert.That(shown, Is.EqualTo(new[] { 0 }), "No mouse-up must not advance.");

            tutorial.SetLocked(true);
            tutorial.UpdateClick(true);
            Assert.That(shown, Is.EqualTo(new[] { 0 }), "Locked guide must not advance.");

            tutorial.SetLocked(false);
            tutorial.UpdateClick(true);
            tutorial.UpdateClick(true);
            tutorial.UpdateClick(true);
            Assert.That(tutorial.IsActive, Is.False);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(shown, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(match.refs.rectMatchTutorial.gameObject.activeSelf, Is.False);

            SetPrivateField(tutorial, "_willStart", true);
            Assert.That(tutorial.StartGuide(), Is.True);
            tutorial.ForceStop();
            Assert.That(tutorial.IsActive, Is.False);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(match.refs.tutorial1.gameObject.activeSelf, Is.False);
        }

        private static void AssertFields(Type owner, params (string Name, Type Type)[] expected)
        {
            FieldInfo[] fields = owner.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => !field.IsStatic)
                .ToArray();

            Assert.That(
                fields.Select(field => (field.Name, field.FieldType)),
                Is.EquivalentTo(expected),
                owner.FullName);
        }

        private global::UI_LavaRush CreateController()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);
            GameObject instance = Track(PrefabUtility.InstantiatePrefab(prefab) as GameObject);
            Assert.That(instance, Is.Not.Null);
            return instance.GetComponent<global::UI_LavaRush>();
        }

        private UI_LavaRush_Match CreateTutorialMatch()
        {
            var root = Track(new GameObject(
                "Tutorial Match",
                typeof(RectTransform),
                typeof(UI_LavaRush_Match)));
            UI_LavaRush_Match match = root.GetComponent<UI_LavaRush_Match>();
            match.refs = new UI_LavaRush_Match.Refs
            {
                rectMatchTutorial = CreateRect("Tutorial Root", root.transform),
                tutorial1 = CreateRect("Tutorial 1", root.transform),
                tutorial2 = CreateRect("Tutorial 2", root.transform),
                tutorial3 = CreateRect("Tutorial 3", root.transform),
            };
            match.refs.rectMatchTutorial.gameObject.SetActive(true);
            match.refs.tutorial1.gameObject.SetActive(false);
            match.refs.tutorial2.gameObject.SetActive(false);
            match.refs.tutorial3.gameObject.SetActive(false);
            return match;
        }

        private UI_Rect CreateRect(string name, Transform parent)
        {
            var root = Track(new GameObject(name, typeof(RectTransform), typeof(UI_Rect)));
            root.transform.SetParent(parent, false);
            return root.GetComponent<UI_Rect>();
        }

        private static int ActiveScreens(global::UI_LavaRush owner) =>
            owner.GetComponentsInChildren<LavaRushControllerView>(true)
                .Count(screen => screen.gameObject.activeSelf);

        private static void InvokeClick(UI_Button button)
        {
            FieldInfo clickField = typeof(UI_Button).GetField(
                "m_OnClick",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var clickEvent = (UnityEvent)clickField.GetValue(button);
            clickEvent.Invoke();
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private GameObject Track(GameObject value)
        {
            _objects.Add(value);
            return value;
        }
    }
}
