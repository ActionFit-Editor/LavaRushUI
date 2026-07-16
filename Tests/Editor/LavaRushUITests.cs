using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.Time;
using NUnit.Framework;
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
                clock,
                new FixedRandom(),
                new LinearLavaRushSeatCurveProvider(),
                "tests/lava-rush-ui-unavailable",
                new AllowLavaRushAccessPolicy(),
                new WeekendSchedule());
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
