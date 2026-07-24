using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushAccessRewardParityTests
    {
        private const string IconPrefabPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Icon.prefab";
        private const string CellPrefabPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/Icon/UI_LavaRush_Cell.prefab";

        private readonly List<UnityEngine.Object> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
                if (_objects[index] != null)
                    UnityEngine.Object.DestroyImmediate(_objects[index]);
            _objects.Clear();
        }

        [Test]
        public void OriginalAccessControllerSurface_KeepsConcreteTypesAndTargetCompatibility()
        {
            Assert.That(
                typeof(UI_LavaRush_Icon.Refs).GetField(nameof(UI_LavaRush_Icon.Refs.txtTimer))?.FieldType,
                Is.EqualTo(typeof(TextMeshProUGUI)));
            Assert.That(
                typeof(UI_LavaRush_Cell.Refs).GetField(nameof(UI_LavaRush_Cell.Refs.txtTimer))?.FieldType,
                Is.EqualTo(typeof(TextMeshProUGUI)));
            Assert.That(
                typeof(UI_LavaRush_Cell.Refs).GetField(nameof(UI_LavaRush_Cell.Refs.txtStatus))?.FieldType,
                Is.EqualTo(typeof(TextMeshProUGUI)));
            Assert.That(
                typeof(UI_LavaRush_Cell).GetProperty(nameof(UI_LavaRush_Cell.TargetProgress))?.PropertyType,
                Is.EqualTo(typeof(UI_Image)));
            Assert.That(
                typeof(UI_LavaRush_OrderReward)
                    .GetMethod(nameof(UI_LavaRush_OrderReward.PlayMergeEffect))
                    ?.IsStatic,
                Is.True);
        }

        [Test]
        public void Icon_GatesOpenAndOwnsOneCountdownLifetime()
        {
            UI_LavaRush_Icon icon = InstantiatePrefab<UI_LavaRush_Icon>(IconPrefabPath);
            var access = new TestAccessService();
            var countdown = new TestCountdownScheduler();
            int expired = 0;

            icon.Initialize(access, countdown, () => expired++);

            Assert.That(countdown.RegisterCount, Is.EqualTo(1));
            Assert.That(countdown.Target, Is.SameAs(icon.TimerText));
            Assert.That(countdown.Formatter(TimeSpan.FromHours(28).Add(TimeSpan.FromMinutes(5))),
                Is.EqualTo("28:05:00"));
            Assert.That(icon.TryOpen(), Is.False);
            Assert.That(access.OpenCount, Is.Zero);

            access.IsEventActive = true;
            access.IsEventStarted = true;
            Assert.That(icon.TryOpen(), Is.True);
            Assert.That(access.OpenCount, Is.EqualTo(1));

            icon.gameObject.SetActive(false);
            InvokeLifecycle(icon, "OnDisable");
            Assert.That(countdown.Token.IsCancellationRequested, Is.True);
            icon.gameObject.SetActive(true);
            InvokeLifecycle(icon, "OnEnable");
            Assert.That(countdown.RegisterCount, Is.EqualTo(2));

            access.EventRemainTime = TimeSpan.Zero;
            icon.Initialize(access, countdown, () => expired++);
            Assert.That(expired, Is.EqualTo(1));
            Assert.That(countdown.RegisterCount, Is.EqualTo(2));
        }

        [Test]
        public void Cell_RestoresPlayingIndicatorProgressAndRemainingSeatSemantics()
        {
            var canvasRoot = Track(new GameObject(
                "Cell Canvas",
                typeof(RectTransform),
                typeof(Canvas)));
            UI_LavaRush_Cell cell =
                InstantiatePrefab<UI_LavaRush_Cell>(CellPrefabPath, canvasRoot.transform);
            var access = new TestAccessService
            {
                IsEventActive = true,
                IsEventStarted = true,
            };
            var audio = new TestAudio();
            bool playing = false;
            int current = 7;
            int required = 10;
            int remain = 3;

            cell.Initialize(
                access,
                () => (current, required, remain),
                isStagePlaying: () => playing,
                audio: audio);
            InvokeLifecycle(cell, "OnEnable");

            Assert.That(cell.refs.StatusText.text, Is.EqualTo("00/00"));
            Assert.That(cell.refs.StatusGauge.fillAmount, Is.Zero);
            Assert.That(cell.refs.Indicator.gameObject.activeSelf, Is.True);
            Assert.That(cell.refs.RemainTextRoot.gameObject.activeSelf, Is.False);

            playing = true;
            cell.Tick(0.01f);
            Assert.That(cell.refs.StatusText.text, Is.EqualTo("07/10"));
            Assert.That(cell.refs.StatusGauge.fillAmount, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(cell.refs.Indicator.gameObject.activeSelf, Is.False);
            Assert.That(cell.refs.RemainTextRoot.gameObject.activeSelf, Is.True);
            Assert.That(cell.refs.RemainCountText.Text, Is.EqualTo("3"));

            remain = 1;
            cell.Tick(0.25f);
            Assert.That(cell.refs.RemainCountText.Text, Is.EqualTo("1"));
            Assert.That(UI_LavaRush_Cell.Primary, Is.SameAs(cell));

            cell.gameObject.SetActive(false);
            InvokeLifecycle(cell, "OnDisable");
            Assert.That(UI_LavaRush_Cell.Primary, Is.Null);
            cell.gameObject.SetActive(true);
            InvokeLifecycle(cell, "OnEnable");
            Assert.That(UI_LavaRush_Cell.Primary, Is.SameAs(cell));
        }

        [Test]
        public void OrderReward_RefreshAllCanReenableARegisteredDisabledView()
        {
            var root = Track(new GameObject("Order Reward Registry Test"));
            UI_LavaRush_OrderReward reward = root.AddComponent<UI_LavaRush_OrderReward>();
            TextMeshProUGUI label = CreateText("Reward", root.transform);
            reward.refs.txtReward = label;
            int amount = 0;
            bool visible = false;

            reward.Configure(() => amount, () => visible);
            Assert.That(root.activeSelf, Is.False);

            amount = 15;
            visible = true;
            UI_LavaRush_OrderReward.RefreshAll();

            Assert.That(root.activeSelf, Is.True);
            Assert.That(label.text, Is.EqualTo("15"));
        }

        [UnityTest]
        public IEnumerator OrderReward_UsesOneSpawnAndOneArrivalPerExpectedClone()
        {
            var canvasRoot = Track(new GameObject(
                "Order Reward Canvas",
                typeof(RectTransform),
                typeof(Canvas)));
            var host = Track(new GameObject("Order Reward Host", typeof(RectTransform)));
            host.transform.SetParent(canvasRoot.transform, false);
            UI_LavaRush_OrderReward reward = host.AddComponent<UI_LavaRush_OrderReward>();
            var serialized = new SerializedObject(reward);
            serialized.FindProperty("settings.duration").floatValue = 0.01f;
            serialized.FindProperty("settings.stagger").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Image source = CreateImage("Source", host.transform);
            RectTransform target = (RectTransform)Track(new GameObject(
                "Target",
                typeof(RectTransform))).transform;
            target.SetParent(canvasRoot.transform, false);
            target.position = new Vector3(200f, 100f, 0f);
            var audio = new TestAudio();
            reward.ConfigureAudio(audio);
            int spawned = 0;
            int arrived = 0;

            reward.PlayEffect(
                source,
                target,
                11,
                () => spawned++,
                () => arrived++);

            Assert.That(spawned, Is.EqualTo(1));
            Assert.That(audio.Count(LavaRushAudioCue.RewardSpawn), Is.EqualTo(1));
            Assert.That(CountFlights(canvasRoot.transform, "LavaRushRewardFlight"), Is.EqualTo(3));

            yield return WaitUntil(
                () => arrived == 3,
                2f);
            yield return null;

            Assert.That(arrived, Is.EqualTo(3));
            Assert.That(CountFlights(canvasRoot.transform, "LavaRushRewardFlight"), Is.Zero);
        }

        [UnityTest]
        public IEnumerator StaticMergeEffect_UsesConfiguredNeutralSpriteAudioAndCellArrival()
        {
            var canvasRoot = Track(new GameObject(
                "Merge Reward Canvas",
                typeof(RectTransform),
                typeof(Canvas)));
            UI_LavaRush_Cell cell =
                InstantiatePrefab<UI_LavaRush_Cell>(CellPrefabPath, canvasRoot.transform);
            var access = new TestAccessService
            {
                IsEventActive = true,
                IsEventStarted = true,
            };
            var audio = new TestAudio();
            cell.Initialize(
                access,
                () => (1, 10, 4),
                isStagePlaying: () => true,
                audio: audio);
            InvokeLifecycle(cell, "OnEnable");

            var host = Track(new GameObject("Merge Reward Host", typeof(RectTransform)));
            host.transform.SetParent(canvasRoot.transform, false);
            UI_LavaRush_OrderReward reward = host.AddComponent<UI_LavaRush_OrderReward>();
            var serialized = new SerializedObject(reward);
            serialized.FindProperty("settings.duration").floatValue = 0.01f;
            serialized.FindProperty("settings.stagger").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var texture = Track(new Texture2D(2, 2));
            var sprite = Track(Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f)));
            reward.ConfigureMergeEffect(sprite, audio);

            UI_LavaRush_OrderReward.PlayMergeEffect(Vector3.zero, 6);

            Assert.That(audio.Count(LavaRushAudioCue.RewardSpawn), Is.EqualTo(1));
            Assert.That(CountFlights(canvasRoot.transform, "LavaRushMergeFly"), Is.EqualTo(2));

            yield return WaitUntil(
                () => audio.Count(LavaRushAudioCue.RewardArrive) == 2,
                2f);
            yield return null;

            Assert.That(audio.Count(LavaRushAudioCue.RewardArrive), Is.EqualTo(2));
            Assert.That(CountFlights(canvasRoot.transform, "LavaRushMergeFly"), Is.Zero);
        }

        private T InstantiatePrefab<T>(string path, Transform parent = null)
            where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            GameObject instance = parent == null
                ? Track(UnityEngine.Object.Instantiate(prefab))
                : Track(UnityEngine.Object.Instantiate(prefab, parent, false));
            T component = instance.GetComponent<T>();
            Assert.That(component, Is.Not.Null, path);
            return component;
        }

        private static void InvokeLifecycle(MonoBehaviour component, string methodName)
        {
            MethodInfo method = component.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(component, null);
        }

        private T Track<T>(T value)
            where T : UnityEngine.Object
        {
            _objects.Add(value);
            return value;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent)
        {
            var textObject = Track(new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<TextMeshProUGUI>();
        }

        private Image CreateImage(string name, Transform parent)
        {
            var imageObject = Track(new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)));
            imageObject.transform.SetParent(parent, false);
            return imageObject.GetComponent<Image>();
        }

        private static int CountFlights(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Count(child => child.name == name);
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds)
        {
            float deadline = UnityEngine.Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && UnityEngine.Time.realtimeSinceStartup < deadline)
                yield return new WaitForSecondsRealtime(0.01f);
        }

        private sealed class TestAccessService : ILavaRushAccessService
        {
            public bool IsEventActive { get; set; }
            public bool IsEventStarted { get; set; }
            public DateTime EventEndTime { get; set; } = DateTime.UtcNow.AddHours(2);
            public TimeSpan EventRemainTime { get; set; } = TimeSpan.FromHours(2);
            public int OpenCount { get; private set; }

            public void OpenContent() => OpenCount++;
        }

        private sealed class TestCountdownScheduler : ILavaRushCountdownScheduler
        {
            public DateTime Now { get; set; } = DateTime.UtcNow;
            public bool Trusted { get; set; } = true;
            public int RegisterCount { get; private set; }
            public TMP_Text Target { get; private set; }
            public CancellationToken Token { get; private set; }
            public Func<TimeSpan, string> Formatter { get; private set; }

            public bool TryGetNow(out DateTime now)
            {
                now = Now;
                return Trusted;
            }

            public void Register(
                TMP_Text target,
                DateTime endTime,
                CancellationToken cancellationToken,
                Action onExpired = null,
                Func<TimeSpan, string> formatter = null)
            {
                RegisterCount++;
                Target = target;
                Token = cancellationToken;
                Formatter = formatter;
            }
        }

        private sealed class TestAudio : ILavaRushAudio
        {
            private readonly List<LavaRushAudioCue> _calls = new();

            public void Play(LavaRushAudioCue cue) => _calls.Add(cue);

            public void PlayPitched(
                LavaRushAudioCue cue,
                float volume,
                float pitchMin,
                float pitchMax)
            {
                _calls.Add(cue);
            }

            public int Count(LavaRushAudioCue cue) => _calls.Count(call => call == cue);
        }
    }
}
