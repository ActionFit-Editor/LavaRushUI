using System;
using System.Collections.Generic;
using ActionFit.Content;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Standalone composition root and action router for the Lava Rush presentation.</summary>
    [AddComponentMenu("ActionFit/Lava Rush Bootstrap")]
    public sealed class LavaRushBootstrap : MonoBehaviour
    {
        public const string DefaultDemoContentId = "lava-rush-ui-demo";

        [Serializable]
        public sealed class Assets
        {
            [SerializeField] private LavaRushPresentation presentationPrefab;

            public LavaRushPresentation PresentationPrefab => presentationPrefab;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField] private string contentId = DefaultDemoContentId;
            [SerializeField] private bool initializeOnStart = true;

            public string ContentId => string.IsNullOrWhiteSpace(contentId) ? DefaultDemoContentId : contentId.Trim();
            public bool InitializeOnStart => initializeOnStart;
        }

        [SerializeField] private Assets assets = new();
        [SerializeField] private Settings settings = new();

        private LavaRushEngine _engine;
        private LavaRushPresentation _presentation;
        private ILavaRushUILocalizer _localizer;
        private ILavaRushUIViewHost _viewHost;
        private LavaRushDemoClock _demoClock;
        private bool _ownsPresentation;
        private bool _rendering;
        private bool _renderQueued;
        private float _nextRefreshTime;
        private string _message = string.Empty;

        public event Action CloseRequested;

        public LavaRushEngine Engine => _engine;
        public LavaRushPresentation Presentation => _presentation;
        public LavaRushPresentation PresentationPrefab => assets?.PresentationPrefab;
        public bool InitializeOnStart => settings?.InitializeOnStart ?? true;
        public bool IsInitialized => _engine != null && _presentation != null;
        public bool IsVisible => IsInitialized && _presentation.gameObject.activeSelf;

        private void Start()
        {
            if ((settings?.InitializeOnStart ?? true) && !IsInitialized)
            {
                InitializeDefault();
            }
        }

        private void Update()
        {
            if (!IsVisible || UnityEngine.Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = UnityEngine.Time.unscaledTime + _presentation.Config.RefreshIntervalSeconds;
            EvaluateTimers();
            Render();
        }

        private void OnDestroy()
        {
            if (_engine != null)
            {
                _engine.StateChanged -= HandleStateChanged;
            }
            if (_presentation != null)
            {
                _presentation.ActionRequested -= HandleActionRequested;
            }
            if (_ownsPresentation && _viewHost != null && _presentation != null)
            {
                _viewHost.Release(_presentation);
            }
        }

        /// <summary>Composes a complete demo engine with Content Core PlayerPrefs defaults.</summary>
        public void InitializeDefault(
            LavaRushPresentation presentation = null,
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIAudio audio = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushUIProfileProvider profileProvider = null,
            ILavaRushUIViewHost viewHost = null)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Lava Rush UI is already initialized.");
            }

            _demoClock = new LavaRushDemoClock();
            var resolver = new LavaRushDemoCatalogResolver();
            var engine = new LavaRushEngine(
                new PlayerPrefsContentStateStore(),
                new PlayerPrefsContentRewardService(),
                resolver,
                _demoClock,
                _demoClock,
                new SystemLavaRushRandom(),
                new LinearLavaRushSeatCurveProvider(),
                settings?.ContentId ?? DefaultDemoContentId,
                new AllowLavaRushAccessPolicy(),
                new LavaRushDemoSchedulePolicy());

            Initialize(engine, presentation, localizer, audio, rewardRenderer, profileProvider, viewHost);
        }

        /// <summary>Uses a caller-owned engine while retaining package presentation and action routing.</summary>
        public void Initialize(
            LavaRushEngine engine,
            LavaRushPresentation presentation = null,
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIAudio audio = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushUIProfileProvider profileProvider = null,
            ILavaRushUIViewHost viewHost = null)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }
            if (IsInitialized)
            {
                throw new InvalidOperationException("Lava Rush UI is already initialized.");
            }

            _viewHost = viewHost ?? DefaultLavaRushUIViewHost.Instance;
            _engine = engine;
            _presentation = presentation;
            if (_presentation == null)
            {
                _presentation = _viewHost.Create(assets?.PresentationPrefab, transform);
                _ownsPresentation = true;
            }
            if (_presentation == null)
            {
                _engine = null;
                throw new InvalidOperationException("The Lava Rush view host returned no presentation.");
            }

            _localizer = localizer ?? _presentation as ILavaRushUILocalizer ?? PassthroughLavaRushUILocalizer.Instance;
            _presentation.Initialize(_localizer, audio, rewardRenderer, profileProvider);
            _presentation.Show();
            _presentation.ActionRequested += HandleActionRequested;
            _engine.StateChanged += HandleStateChanged;
            _engine.Restore();
            _message = string.Empty;
            EvaluateTimers();
            Render();
        }

        /// <summary>Shows a previously closed presentation and renders the latest engine state.</summary>
        public void Show()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Lava Rush UI is not initialized.");
            }

            _presentation.Show();
            EvaluateTimers();
            Render();
        }

        private void HandleStateChanged(LavaRushState state)
        {
            if (_rendering)
            {
                _renderQueued = true;
                return;
            }
            Render();
        }

        private void HandleActionRequested(LavaRushUIAction action)
        {
            switch (action)
            {
                case LavaRushUIAction.StartEvent:
                    _message = _engine.TryStartEvent()
                        ? Localize(LavaRushUIKeys.StatusEventStarted, "The event was started and pinned to the demo catalog.")
                        : Localize(LavaRushUIKeys.StatusEventUnavailable, "The event cannot start in the current access window.");
                    break;
                case LavaRushUIAction.SelectEasy:
                    SelectDifficulty(1, "Easy");
                    break;
                case LavaRushUIAction.SelectNormal:
                    SelectDifficulty(2, "Normal");
                    break;
                case LavaRushUIAction.SelectHard:
                    SelectDifficulty(3, "Hard");
                    break;
                case LavaRushUIAction.CompleteTutorial:
                    _engine.SetTutorialDone(true);
                    _message = Localize(LavaRushUIKeys.StatusTutorialComplete, "Tutorial complete. The first stage is ready.");
                    break;
                case LavaRushUIAction.StartStage:
                    _message = _engine.StartStage()
                        ? Localize(LavaRushUIKeys.StatusStageStarted, "The stage timer is running.")
                        : Localize(LavaRushUIKeys.StatusStageUnavailable, "The stage could not be started.");
                    break;
                case LavaRushUIAction.AddProgress:
                    AddDemoProgress();
                    break;
                case LavaRushUIAction.EvaluateStage:
                    EvaluateStage();
                    break;
                case LavaRushUIAction.ConfirmResult:
                    ConfirmResult();
                    break;
                case LavaRushUIAction.EndEvent:
                    _engine.EndEvent();
                    _message = Localize(LavaRushUIKeys.StatusEventEnded, "The event was ended without deleting its historical window.");
                    break;
                case LavaRushUIAction.Close:
                    _message = string.Empty;
                    _presentation.Hide();
                    CloseRequested?.Invoke();
                    return;
                case LavaRushUIAction.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            Render();
        }

        private void SelectDifficulty(int difficulty, string fallbackName)
        {
            _message = _engine.SelectDifficulty(difficulty)
                ? FormatLocalized(
                    LavaRushUIKeys.StatusDifficultySelected,
                    "{0} difficulty selected.",
                    fallbackName)
                : Localize(LavaRushUIKeys.StatusDifficultyUnavailable, "That difficulty is unavailable.");
        }

        private void AddDemoProgress()
        {
            int amount = _presentation.Config.DemoProgressAmount;
            int previousProgress = _engine.StageProgress;
            LavaRushResult result = _engine.AddProgress(amount);
            bool accepted = result != LavaRushResult.None || _engine.StageProgress > previousProgress;
            _message = accepted
                ? FormatLocalized(LavaRushUIKeys.StatusProgressAdded, "Added {0} demo progress through the engine.", amount)
                : Localize(LavaRushUIKeys.StatusProgressUnavailable, "Progress is accepted only while a stage is running.");
        }

        private void EvaluateStage()
        {
            if (_demoClock != null && _engine.IsStagePlaying)
            {
                TimeSpan remaining = _engine.StageRemainingTime;
                _demoClock.Advance(remaining + TimeSpan.FromSeconds(1d));
            }

            LavaRushResult result = _engine.EvaluateStageResult();
            _message = result == LavaRushResult.None
                ? Localize(LavaRushUIKeys.StatusStagePending, "The stage has not reached a win or timeout condition.")
                : string.Empty;
        }

        private void ConfirmResult()
        {
            if (_engine.PendingResult == LavaRushResult.Win)
            {
                if (!_engine.ClaimPendingReward())
                {
                    _message = Localize(
                        LavaRushUIKeys.StatusRewardUnavailable,
                        "The saved reward is not claimable until the host provides an available idempotent reward service.");
                    return;
                }
                _message = Localize(LavaRushUIKeys.StatusRewardClaimed, "The saved reward transaction was confirmed.");
            }
            else
            {
                _message = Localize(LavaRushUIKeys.StatusResultCleared, "The stage result was confirmed.");
            }

            _engine.ClearPendingResult();
        }

        private void EvaluateTimers()
        {
            if (_engine == null)
            {
                return;
            }
            _engine.EvaluateEventTimeout();
            _engine.EvaluateStageResult();
        }

        private void Render()
        {
            if (!IsVisible)
            {
                return;
            }
            if (_rendering)
            {
                _renderQueued = true;
                return;
            }

            do
            {
                _renderQueued = false;
                _rendering = true;
                try
                {
                    _presentation.Present(BuildViewModel());
                }
                finally
                {
                    _rendering = false;
                }
            }
            while (_renderQueued);
        }

        private LavaRushUIViewModel BuildViewModel()
        {
            LavaRushUIScreen screen = DetermineScreen();
            IReadOnlyList<ContentReward> rewards = GetVisibleRewards(screen);
            BuildActions(screen, out LavaRushUIButtonModel primary, out LavaRushUIButtonModel secondary, out LavaRushUIButtonModel tertiary);
            return new LavaRushUIViewModel(
                screen,
                _message,
                _engine.SelectedDifficulty,
                _engine.Stage,
                _engine.StageCount,
                _engine.StageProgress,
                _engine.RequiredProgress,
                _engine.PendingResult == LavaRushResult.None ? _engine.FakeSeatCount : _engine.ResultSeatCount,
                _engine.PendingResult == LavaRushResult.None ? _engine.SeatCapacity : _engine.ResultSeatCapacity,
                _engine.WinRank,
                _engine.IsEventStarted ? _engine.EventRemainingTime : _engine.ExpectedRemainingTime,
                _engine.StageRemainingTime,
                _engine.PendingResult,
                rewards,
                primary,
                secondary,
                tertiary);
        }

        private LavaRushUIScreen DetermineScreen()
        {
            if (!_engine.IsEventStarted)
            {
                return LavaRushUIScreen.EventStart;
            }
            if (_engine.SelectedDifficulty == LavaRushEngine.NoDifficulty)
            {
                return LavaRushUIScreen.Difficulty;
            }
            if (!_engine.TutorialDone)
            {
                return LavaRushUIScreen.Tutorial;
            }
            if (_engine.PendingResult != LavaRushResult.None)
            {
                return LavaRushUIScreen.Result;
            }
            if (_engine.PendingEnd)
            {
                return LavaRushUIScreen.EventEnd;
            }
            return _engine.AllStagesComplete ? LavaRushUIScreen.Complete : LavaRushUIScreen.Match;
        }

        private IReadOnlyList<ContentReward> GetVisibleRewards(LavaRushUIScreen screen)
        {
            if (_engine.SelectedDifficulty <= 0
                || (screen != LavaRushUIScreen.Result && screen != LavaRushUIScreen.Complete))
            {
                return Array.Empty<ContentReward>();
            }

            return _engine.Catalog
                .GetDifficulty(_engine.SelectedDifficulty)
                .GetStage(_engine.Stage)
                .Rewards;
        }

        private void BuildActions(
            LavaRushUIScreen screen,
            out LavaRushUIButtonModel primary,
            out LavaRushUIButtonModel secondary,
            out LavaRushUIButtonModel tertiary)
        {
            primary = LavaRushUIButtonModel.Hidden;
            secondary = LavaRushUIButtonModel.Hidden;
            tertiary = Button(LavaRushUIAction.Close, LavaRushUIKeys.ActionClose, "Close");

            switch (screen)
            {
                case LavaRushUIScreen.EventStart:
                    primary = Button(LavaRushUIAction.StartEvent, LavaRushUIKeys.ActionStartEvent, "Start Event");
                    break;
                case LavaRushUIScreen.Difficulty:
                    primary = Button(LavaRushUIAction.SelectEasy, LavaRushUIKeys.ActionEasy, "Easy");
                    secondary = Button(LavaRushUIAction.SelectNormal, LavaRushUIKeys.ActionNormal, "Normal");
                    tertiary = Button(LavaRushUIAction.SelectHard, LavaRushUIKeys.ActionHard, "Hard");
                    break;
                case LavaRushUIScreen.Tutorial:
                    primary = Button(LavaRushUIAction.CompleteTutorial, LavaRushUIKeys.ActionContinue, "Continue");
                    break;
                case LavaRushUIScreen.Match when !_engine.IsStagePlaying:
                    primary = Button(LavaRushUIAction.StartStage, LavaRushUIKeys.ActionStartStage, "Start Stage");
                    break;
                case LavaRushUIScreen.Match when _presentation.Config.ShowDemoActions:
                    primary = Button(LavaRushUIAction.AddProgress, LavaRushUIKeys.ActionAddProgress, "+ Progress");
                    secondary = Button(LavaRushUIAction.EvaluateStage, LavaRushUIKeys.ActionEvaluateStage, "Resolve Timer");
                    break;
                case LavaRushUIScreen.Result:
                    string key = _engine.PendingResult == LavaRushResult.Win
                        ? LavaRushUIKeys.ActionClaim
                        : LavaRushUIKeys.ActionRetry;
                    string label = _engine.PendingResult == LavaRushResult.Win ? "Claim" : "Retry";
                    primary = Button(LavaRushUIAction.ConfirmResult, key, label);
                    break;
                case LavaRushUIScreen.Complete:
                case LavaRushUIScreen.EventEnd:
                    primary = Button(LavaRushUIAction.EndEvent, LavaRushUIKeys.ActionEndEvent, "End Event");
                    break;
            }
        }

        private LavaRushUIButtonModel Button(LavaRushUIAction action, string key, string fallback)
        {
            return new LavaRushUIButtonModel(action, Localize(key, fallback));
        }

        private string Localize(string key, string fallback)
        {
            return _localizer?.Get(key, fallback) ?? fallback ?? string.Empty;
        }

        private string FormatLocalized(string key, string fallback, params object[] arguments)
        {
            string format = Localize(key, fallback);
            try
            {
                return string.Format(format, arguments);
            }
            catch (FormatException)
            {
                return string.Format(fallback, arguments);
            }
        }
    }
}
