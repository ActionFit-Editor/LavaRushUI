using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using UnityEngine;

/// <summary>Production Lava Rush controller family root. LavaRushEngine is its sole state authority.</summary>
[DisallowMultipleComponent]
public class UI_LavaRush : MonoBehaviour
{
    [Serializable]
    public sealed class Refs
    {
        public UI_LavaRush_EventStart uiEventStart;
        public UI_LavaRush_Difficulty uiDifficulty;
        public LavaRushTutorialView uiTutorial;
        public UI_LavaRush_Match uiMatch;
        public UI_LavaRush_MatchWin uiMatchWin;
        public UI_LavaRush_MatchLose uiMatchLose;
        public UI_LavaRush_MatchEnd uiMatchEnd;
        public UI_LavaRush_EventEnd uiEventEnd;
    }

    [SerializeField] public Refs refs = new();

    private readonly List<LavaRushControllerView> _screens = new(8);
    private LavaRushControllerContext _context;
    private IDisposable _frameSubscription;
    private IDisposable _orderSubscription;
    private ILavaRushLocalizationRefreshSource _localizationRefresh;
    private LavaRushControllerView _activeScreen;
    private float _refreshElapsed;
    private bool _resolvingResult;
    private bool _returnFromInfoTutorial;

    public LavaRushEngine Engine => _context?.Engine;
    public ViewController PackageFlow => refs?.uiEventStart;
    public bool IsInitialized => _context != null;
    public LavaRushProfileSnapshot PlayerProfile =>
        _context?.Profiles?.GetPlayer() ?? DefaultLavaRushProfileRoster.Instance.GetPlayer();

    public int SelectedDifficulty => Engine?.SelectedDifficulty ?? LavaRushEngine.NoDifficulty;
    public bool IsTutorialDone => Engine?.TutorialDone ?? false;
    public bool IsInTutorial =>
        _activeScreen == refs?.uiTutorial && _activeScreen != null && _activeScreen.gameObject.activeSelf;
    public int CurrentStage => Engine?.Stage ?? LavaRushEngine.MinStage;
    public int StageProgress => Engine?.StageProgress ?? 0;
    public bool AllStagesComplete => Engine?.AllStagesComplete ?? false;
    public string PendingResult
    {
        get => (Engine?.PendingResult ?? LavaRushResult.None) switch
        {
            LavaRushResult.Win => "win",
            LavaRushResult.Lose => "lose",
            _ => string.Empty,
        };
        set
        {
            if (Engine == null)
                return;
            if (string.IsNullOrEmpty(value))
            {
                Engine.ClearPendingResult();
                return;
            }

            throw new InvalidOperationException("Lava Rush results are owned by LavaRushEngine.");
        }
    }
    public int Difficulty => SelectedDifficulty;
    public int StageCount => Engine?.StageCount ?? 0;
    public int RemainLevel => Mathf.Max(0, StageCount - CurrentStage);
    public int RequiredProgress => Engine?.RequiredProgress ?? 0;
    public int SeatFoothold => CurrentStage + 1;
    public int SeatCapacity => Engine?.SeatCapacity ?? 0;
    public int FakeSeatCount => Engine?.FakeSeatCount ?? 0;
    public int WinRank => Engine?.WinRank ?? 1;
    public int SeatCountForDisplay => Engine?.PendingResult == LavaRushResult.None
        ? FakeSeatCount
        : Engine?.ResultSeatCount ?? 0;
    public int SeatCapacityForDisplay => Engine?.PendingResult == LavaRushResult.None
        ? SeatCapacity
        : Engine?.ResultSeatCapacity ?? 0;
    public float SeatRatioForDisplay => SeatCapacityForDisplay <= 0
        ? 0f
        : Mathf.Clamp01(SeatCountForDisplay / (float)SeatCapacityForDisplay);
    public bool IsStagePlaying => Engine?.IsStagePlaying ?? false;
    public bool IsStageGoalReached => Engine?.IsStageGoalReached ?? false;
    public bool IsFinalFoothold => Engine?.IsFinalFoothold ?? false;
    public bool IsResultPopupOpen => _activeScreen == refs?.uiMatchWin
        || _activeScreen == refs?.uiMatchLose
        || _activeScreen == refs?.uiMatchEnd;
    public TimeSpan StageRemainTime => Engine?.StageRemainingTime ?? TimeSpan.Zero;

    internal bool InitializePackageUI(LavaRushEngine engine)
    {
        if (engine == null)
            return false;

        Initialize(new LavaRushControllerContext(engine), false);
        return true;
    }

    public void Initialize(LavaRushControllerContext context, bool restoreEngine)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (_context != null)
            throw new InvalidOperationException("Lava Rush controllers are already initialized.");

        _context = context;
        ResolveScreens();
        for (int index = 0; index < _screens.Count; index++)
            _screens[index].Bind(this);

        Engine.StateChanged += HandleStateChanged;
        _frameSubscription = context.FrameScheduler?.SubscribeUpdate(Advance);
        _orderSubscription = context.OrderProgress?.Subscribe(AddProgress);
        _localizationRefresh = context.Localizer as ILavaRushLocalizationRefreshSource;
        if (_localizationRefresh != null)
            _localizationRefresh.LocaleChanged += RefreshActive;
        if (restoreEngine)
            Engine.Restore();

        HideAll();
    }

    private void Update()
    {
        if (_context?.FrameScheduler == null)
            Advance(Time.unscaledDeltaTime);
    }

    private void OnDestroy()
    {
        if (Engine != null)
            Engine.StateChanged -= HandleStateChanged;
        _frameSubscription?.Dispose();
        _orderSubscription?.Dispose();
        if (_localizationRefresh != null)
            _localizationRefresh.LocaleChanged -= RefreshActive;
    }

    public void ResetGameplay()
    {
        Engine?.ResetGameplay();
        RefreshOrderRewards();
    }

    public void RefreshOrderRewards() => _context?.RefreshAccess?.Invoke();

    public void OnTutorialComplete()
    {
        Engine?.SetTutorialDone(true);
    }

    public void StartMatch(int difficulty)
    {
        if (Engine?.SelectDifficulty(difficulty) != true)
            return;

        refs?.uiMatch?.Initialize(difficulty);
        Show(LavaRushControllerScreen.Match);
        if (!Engine.TutorialDone)
        {
            refs?.uiMatch?.StartTutorial();
            return;
        }

        refs?.uiMatch?.MarkDifficultyIntroPending();
        StartStage();
    }

    public void StartMatch<TDifficulty>(TDifficulty difficulty)
        where TDifficulty : struct, Enum =>
        StartMatch(Convert.ToInt32(difficulty));

    public void OpenMatchFlow()
    {
        if (Engine == null)
            return;

        Engine.EvaluateEventTimeout();
        Engine.EvaluateStageResult();
        if (Engine.PendingResult is LavaRushResult.Win or LavaRushResult.Lose)
        {
            BeginResolvedResult(Engine.PendingResult);
            return;
        }
        Show(DetermineScreen());
    }

    public void OpenTutorial()
    {
        _returnFromInfoTutorial = _activeScreen == refs?.uiMatch;
        Show(LavaRushControllerScreen.Tutorial);
    }

    public void OnMatchWin() => BeginResolvedResult(LavaRushResult.Win);
    public void OnMatchLose() => BeginResolvedResult(LavaRushResult.Lose);
    public void OpenEventEndCompleted() => OpenCompletedEventEnd();

    public bool TryRouteExpiredEvent(bool openOverlay = false)
    {
        if (Engine == null)
            return false;

        Engine.EvaluateEventTimeout();
        if (Engine.PendingResult is LavaRushResult.Win or LavaRushResult.Lose)
            return false;
        if (!Engine.PendingEnd)
            return false;

        Show(LavaRushControllerScreen.EventEnd);
        return true;
    }

    public void StartStage()
    {
        if (TryRouteExpiredEvent(true) || Engine?.StartStage() != true)
            return;

        _context.Audio.PlayPitched(LavaRushAudioCue.ProfileAppear, 0.5f, 0.85f, 1.2f);
        RefreshOrderRewards();
        Show(LavaRushControllerScreen.Match);
        refs?.uiMatch?.PlayProfileIntro();
    }

    public void AddProgress(int amount)
    {
        if (amount <= 0 || Engine == null)
            return;

        LavaRushResult result = Engine.AddProgress(amount);
        if (result == LavaRushResult.Win)
        {
            RefreshOrderRewards();
            UI_LavaRush_Cell.NotifyProgressArrived();
            BeginResolvedResult(result);
            return;
        }

        RefreshActive();
    }

    public void DevForceWin()
    {
        if (Engine?.ForceWin() == LavaRushResult.Win)
            BeginResolvedResult(LavaRushResult.Win);
    }

    public void DevForceLose()
    {
        if (Engine?.ForceLose() == LavaRushResult.Lose)
            BeginResolvedResult(LavaRushResult.Lose);
    }

    public bool GrantPendingReward()
    {
        if (Engine == null)
            return false;

        int rewardStage = AllStagesComplete ? StageCount : CurrentStage;
        Vector3 origin = refs?.uiMatch != null
            ? refs.uiMatch.GetStageRewardWorldPos(rewardStage)
            : default;
        bool claimed = _context.ClaimPendingReward(origin);
        if (claimed)
            _context.Audio.Play(AllStagesComplete
                ? LavaRushAudioCue.FinalRewardOpen
                : LavaRushAudioCue.RewardArrive);
        return claimed;
    }

    public bool IsStageRewardClaimed(int stage) => Engine?.IsStageRewardClaimed(stage) ?? false;

    public void TryOpenEndPopup()
    {
        if (Engine?.PendingEnd == true)
            Show(LavaRushControllerScreen.EventEnd);
    }

    internal void StartNextOrRetryStage()
    {
        if (TryRouteExpiredEvent(true))
            return;
        if (Engine?.PendingResult == LavaRushResult.Win)
        {
            ConfirmResult();
            return;
        }

        Engine?.ClearPendingResult();
        Show(LavaRushControllerScreen.Match);
        StartStage();
    }

    internal void ReturnToMatch()
    {
        if (TryRouteExpiredEvent(true))
            return;

        if (Engine?.PendingResult != LavaRushResult.Win)
            Engine?.ClearPendingResult();
        Show(LavaRushControllerScreen.Match);
    }

    internal void ConfirmPendingResult() => ConfirmResult();

    internal void ConfirmEventStart() => HandleAction(LavaRushUIAction.StartEvent);

    internal void ConfirmEventEnd()
    {
        Engine?.EndEvent();
        _context?.RefreshAccess?.Invoke();
        HideAll();
    }

    internal void CloseActiveScreen()
    {
        refs?.uiMatch?.CancelResultPresentation();
        HideAll();
    }

    internal string RenderRewards(IReadOnlyList<ContentReward> rewards) =>
        _context?.RewardRenderer.Render(rewards, _context.Localizer) ?? string.Empty;

    internal ILavaRushProfileRoster ProfileRoster => _context?.Profiles;
    internal ILavaRushProfileGroupFactory ProfileGroupFactory => _context?.ProfileGroupFactory;
    internal ILavaRushTutorialFocusSpriteProvider TutorialFocusSprites =>
        _context?.TutorialFocusSprites;
    internal ILavaRushRewardPresentationProvider RewardPresentation =>
        _context?.RewardPresentation;
    internal ILavaRushFrameScheduler FrameScheduler => _context?.FrameScheduler;

    internal void PlayAudio(LavaRushAudioCue cue) => _context?.Audio.Play(cue);

    internal string LocalizeText(string key, string fallback) => Localize(key, fallback);

    internal LavaRushControllerSnapshot CreateSnapshot(LavaRushControllerScreen screen)
    {
        if (Engine == null)
            throw new InvalidOperationException("Lava Rush controllers are not initialized.");

        IReadOnlyList<ContentReward> rewards = GetVisibleRewards(screen);
        BuildActions(screen, out LavaRushUIButtonModel primary, out LavaRushUIButtonModel secondary, out LavaRushUIButtonModel tertiary);
        return new LavaRushControllerSnapshot(
            screen,
            Localize(LavaRushLocalizationKeys.Title, "Lava Rush"),
            Message(screen),
            Engine.SelectedDifficulty,
            Engine.Stage,
            Engine.StageCount,
            Engine.StageProgress,
            Engine.RequiredProgress,
            Engine.PendingResult == LavaRushResult.None ? Engine.FakeSeatCount : Engine.ResultSeatCount,
            Engine.PendingResult == LavaRushResult.None ? Engine.SeatCapacity : Engine.ResultSeatCapacity,
            Engine.WinRank,
            Engine.IsEventStarted ? Engine.EventRemainingTime : Engine.ExpectedRemainingTime,
            Engine.StageRemainingTime,
            Engine.PendingResult,
            rewards,
            primary,
            secondary,
            tertiary);
    }

    internal void HandleAction(LavaRushUIAction action)
    {
        switch (action)
        {
            case LavaRushUIAction.StartEvent:
                if (Engine?.TryStartEvent() == true)
                {
                    if (refs?.uiEventStart != null && refs.uiEventStart.gameObject.activeSelf)
                        refs.uiEventStart.Close();
                    _context.RefreshAccess?.Invoke();
                    Show(LavaRushControllerScreen.Difficulty);
                }
                break;
            case LavaRushUIAction.SelectEasy:
                StartMatch(1);
                break;
            case LavaRushUIAction.SelectNormal:
                StartMatch(2);
                break;
            case LavaRushUIAction.SelectHard:
                StartMatch(3);
                break;
            case LavaRushUIAction.CompleteTutorial:
                CompleteInformationTutorial();
                break;
            case LavaRushUIAction.StartStage:
                StartStage();
                break;
            case LavaRushUIAction.AddProgress:
                AddProgress(1);
                break;
            case LavaRushUIAction.EvaluateStage:
                EvaluateStage();
                break;
            case LavaRushUIAction.ConfirmResult:
                ConfirmResult();
                break;
            case LavaRushUIAction.EndEvent:
                ConfirmEventEnd();
                break;
            case LavaRushUIAction.Close:
                CloseActiveScreen();
                break;
        }
    }

    private void ResolveScreens()
    {
        refs ??= new Refs();
        refs.uiEventStart ??= GetComponentInChildren<UI_LavaRush_EventStart>(true);
        refs.uiDifficulty ??= GetComponentInChildren<UI_LavaRush_Difficulty>(true);
        refs.uiTutorial ??= GetComponentInChildren<LavaRushTutorialView>(true);
        refs.uiMatch ??= GetComponentInChildren<UI_LavaRush_Match>(true);
        refs.uiMatchWin ??= GetComponentInChildren<UI_LavaRush_MatchWin>(true);
        refs.uiMatchLose ??= GetComponentInChildren<UI_LavaRush_MatchLose>(true);
        refs.uiMatchEnd ??= GetComponentInChildren<UI_LavaRush_MatchEnd>(true);
        refs.uiEventEnd ??= GetComponentInChildren<UI_LavaRush_EventEnd>(true);

        _screens.Clear();
        Add(refs.uiEventStart);
        Add(refs.uiDifficulty);
        Add(refs.uiTutorial);
        Add(refs.uiMatch);
        Add(refs.uiMatchWin);
        Add(refs.uiMatchLose);
        Add(refs.uiMatchEnd);
        Add(refs.uiEventEnd);
        if (_screens.Count != 8)
            throw new InvalidOperationException($"Lava Rush requires 8 controller screens, found {_screens.Count}.");
    }

    private void Add(LavaRushControllerView screen)
    {
        if (screen != null && !_screens.Contains(screen))
            _screens.Add(screen);
    }

    private void Show(LavaRushControllerScreen screen)
    {
        LavaRushControllerView previous = _activeScreen;
        LavaRushControllerView target = null;
        for (int index = 0; index < _screens.Count; index++)
        {
            LavaRushControllerView candidate = _screens[index];
            bool active = candidate.Screen == screen;
            candidate.gameObject.SetActive(active);
            if (active)
                target = candidate;
        }

        _activeScreen = target;
        bool screenChanged = previous != target;
        _activeScreen?.Activate(CreateSnapshot(screen), screenChanged);
        if (!screenChanged)
            return;
        if (screen == LavaRushControllerScreen.MatchWin)
            _context?.Audio.Play(LavaRushAudioCue.MatchWin);
        else if (screen == LavaRushControllerScreen.MatchLose)
            _context?.Audio.Play(LavaRushAudioCue.MatchLose);
    }

    private void HideAll()
    {
        for (int index = 0; index < _screens.Count; index++)
            _screens[index].gameObject.SetActive(false);
        _activeScreen = null;
    }

    private LavaRushControllerScreen DetermineScreen()
    {
        if (!Engine.IsEventStarted)
            return LavaRushControllerScreen.EventStart;
        if (Engine.SelectedDifficulty == LavaRushEngine.NoDifficulty)
            return LavaRushControllerScreen.Difficulty;
        if (Engine.PendingResult == LavaRushResult.Win)
            return LavaRushControllerScreen.MatchWin;
        if (Engine.PendingResult == LavaRushResult.Lose)
            return LavaRushControllerScreen.MatchLose;
        if (Engine.PendingEnd)
            return LavaRushControllerScreen.EventEnd;
        return Engine.AllStagesComplete
            ? LavaRushControllerScreen.MatchEnd
            : LavaRushControllerScreen.Match;
    }

    private IReadOnlyList<ContentReward> GetVisibleRewards(LavaRushControllerScreen screen)
    {
        if (Engine.SelectedDifficulty <= 0
            || screen is not (LavaRushControllerScreen.MatchWin or LavaRushControllerScreen.MatchEnd))
            return Array.Empty<ContentReward>();

        LavaRushDifficultyDefinition difficulty = Engine.Catalog.GetDifficulty(Engine.SelectedDifficulty);
        int stage = Math.Max(LavaRushEngine.MinStage, Math.Min(difficulty.StageCount, Engine.Stage));
        return difficulty.GetStage(stage).Rewards;
    }

    private void BuildActions(
        LavaRushControllerScreen screen,
        out LavaRushUIButtonModel primary,
        out LavaRushUIButtonModel secondary,
        out LavaRushUIButtonModel tertiary)
    {
        primary = LavaRushUIButtonModel.Hidden;
        secondary = LavaRushUIButtonModel.Hidden;
        tertiary = Button(LavaRushUIAction.Close, LavaRushUIKeys.ActionClose, "Close");
        switch (screen)
        {
            case LavaRushControllerScreen.EventStart:
                primary = Button(LavaRushUIAction.StartEvent, LavaRushUIKeys.ActionStartEvent, "Start");
                break;
            case LavaRushControllerScreen.Difficulty:
                primary = Button(LavaRushUIAction.SelectEasy, LavaRushUIKeys.ActionEasy, "Easy");
                secondary = Button(LavaRushUIAction.SelectNormal, LavaRushUIKeys.ActionNormal, "Normal");
                tertiary = Button(LavaRushUIAction.SelectHard, LavaRushUIKeys.ActionHard, "Hard");
                break;
            case LavaRushControllerScreen.Tutorial:
                primary = Button(LavaRushUIAction.CompleteTutorial, LavaRushUIKeys.ActionContinue, "Continue");
                break;
            case LavaRushControllerScreen.Match:
                primary = Engine.IsStagePlaying
                    ? LavaRushUIButtonModel.Hidden
                    : Button(LavaRushUIAction.StartStage, LavaRushUIKeys.ActionStartStage, "Start");
                break;
            case LavaRushControllerScreen.MatchWin:
            case LavaRushControllerScreen.MatchLose:
                primary = Button(
                    LavaRushUIAction.ConfirmResult,
                    Engine.PendingResult == LavaRushResult.Win
                        ? LavaRushUIKeys.ActionClaim
                        : LavaRushUIKeys.ActionRetry,
                    Engine.PendingResult == LavaRushResult.Win ? "Claim" : "Retry");
                break;
            case LavaRushControllerScreen.MatchEnd:
                primary = Button(LavaRushUIAction.Close, LavaRushUIKeys.ActionClose, "Close");
                break;
            case LavaRushControllerScreen.EventEnd:
                primary = Button(LavaRushUIAction.EndEvent, LavaRushUIKeys.ActionEndEvent, "Confirm");
                break;
        }
    }

    private LavaRushUIButtonModel Button(LavaRushUIAction action, string key, string fallback) =>
        new(action, Localize(key, fallback));

    private string Message(LavaRushControllerScreen screen)
    {
        (string key, string fallback) = screen switch
        {
            LavaRushControllerScreen.EventStart =>
                (LavaRushLocalizationKeys.EventStartDescription, "The Lava Rush event has started."),
            LavaRushControllerScreen.Difficulty =>
                (LavaRushLocalizationKeys.DifficultyDescription, "Choose your difficulty."),
            LavaRushControllerScreen.Tutorial =>
                (LavaRushLocalizationKeys.TutorialStep1, "Complete orders to earn progress."),
            LavaRushControllerScreen.Match =>
                (LavaRushLocalizationKeys.MatchDescription, "Complete orders and merge items."),
            LavaRushControllerScreen.MatchWin =>
                (LavaRushLocalizationKeys.MatchWinPrimary, "Stage cleared!"),
            LavaRushControllerScreen.MatchLose =>
                (LavaRushLocalizationKeys.MatchLosePrimary, "Time is up."),
            LavaRushControllerScreen.MatchEnd =>
                (LavaRushLocalizationKeys.MatchWinComplete, "All stages complete!"),
            LavaRushControllerScreen.EventEnd =>
                (LavaRushLocalizationKeys.EventEndDescription, "The Lava Rush event has ended."),
            _ => (string.Empty, string.Empty),
        };
        return Localize(key, fallback);
    }

    private string Localize(string key, string fallback) =>
        _context?.Localizer.Get(key, fallback) ?? fallback ?? string.Empty;

    private void Advance(float deltaTime)
    {
        if (Engine == null)
            return;

        _refreshElapsed += Mathf.Max(0f, deltaTime);
        if (_refreshElapsed < 0.25f)
            return;
        _refreshElapsed = 0f;

        Engine.EvaluateEventTimeout();
        LavaRushResult result = Engine.EvaluateStageResult();
        if (result is LavaRushResult.Win or LavaRushResult.Lose)
            BeginResolvedResult(result);
        else if (Engine.PendingResult is LavaRushResult.Win or LavaRushResult.Lose)
            BeginResolvedResult(Engine.PendingResult);
        else if (Engine.PendingEnd)
            Show(LavaRushControllerScreen.EventEnd);
        else
            RefreshActive();
    }

    private void EvaluateStage()
    {
        LavaRushResult result = Engine?.EvaluateStageResult() ?? LavaRushResult.None;
        if (result is LavaRushResult.Win or LavaRushResult.Lose)
            BeginResolvedResult(result);
    }

    private void ConfirmResult()
    {
        if (Engine == null)
            return;

        bool won = Engine.PendingResult == LavaRushResult.Win;
        if (won)
        {
            if (!GrantPendingReward())
                return;
            Engine.ClearPendingResult();
        }
        else
        {
            Engine.ClearPendingResult();
        }

        if (Engine.AllStagesComplete)
        {
            OpenCompletedEventEnd();
            return;
        }
        if (!TryRouteExpiredEvent(true))
            Show(won
                ? LavaRushControllerScreen.MatchWin
                : LavaRushControllerScreen.Match);
    }

    private void BeginResolvedResult(LavaRushResult result)
    {
        if (_resolvingResult || result == LavaRushResult.None || Engine == null)
            return;

        _resolvingResult = true;
        Show(LavaRushControllerScreen.Match);
        if (result == LavaRushResult.Win)
        {
            refs?.uiMatch?.PlayWinResult(FinishWinResult, CancelResolvedResult);
            if (refs?.uiMatch == null)
                FinishWinResult();
            return;
        }

        refs?.uiMatch?.PlayLoseResult(FinishLoseResult, CancelResolvedResult);
        if (refs?.uiMatch == null)
            FinishLoseResult();
    }

    private void FinishWinResult()
    {
        if (Engine == null || Engine.PendingResult != LavaRushResult.Win)
        {
            _resolvingResult = false;
            return;
        }

        if (!GrantPendingReward())
        {
            _resolvingResult = false;
            Show(LavaRushControllerScreen.MatchWin);
            return;
        }

        Engine.ClearPendingResult();
        _resolvingResult = false;
        if (Engine.AllStagesComplete)
        {
            OpenCompletedEventEnd();
        }
        else if (!TryRouteExpiredEvent(true))
        {
            Show(LavaRushControllerScreen.MatchWin);
        }
    }

    private void FinishLoseResult()
    {
        _resolvingResult = false;
        if (Engine?.PendingResult == LavaRushResult.Lose && !TryRouteExpiredEvent(true))
            Show(LavaRushControllerScreen.MatchLose);
    }

    private void CancelResolvedResult()
    {
        _resolvingResult = false;
    }

    private void CompleteInformationTutorial()
    {
        bool returnToMatch = _returnFromInfoTutorial;
        _returnFromInfoTutorial = false;
        if (!returnToMatch)
            OnTutorialComplete();
        Show(returnToMatch
            ? LavaRushControllerScreen.Match
            : DetermineScreen());
    }

    private void OpenCompletedEventEnd()
    {
        if (Engine == null)
            return;

        Engine.SetPendingEnd(true);
        _context?.RefreshAccess?.Invoke();
        Show(LavaRushControllerScreen.EventEnd);
    }

    private void HandleStateChanged(LavaRushState state) => RefreshActive();

    private void RefreshActive()
    {
        if (_activeScreen != null && _activeScreen.gameObject.activeSelf)
            _activeScreen.Present(CreateSnapshot(_activeScreen.Screen));
    }
}
