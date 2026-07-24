using System;
using System.Collections;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Original Match controller identity. Durable state remains owned by <see cref="LavaRushEngine"/>;
/// this component rebuilds the authored Match presentation and sequences visual milestones.
/// </summary>
public class UI_LavaRush_Match : LavaRushControllerView
{
    [Serializable]
    public sealed class Refs
    {
        public LavaRushBlockView lavaBlock;
        public UI_Rect content;
        public VerticalLayoutGroup verContent;
        public UI_Scroll scrollLavaBlock;
        public UI_Image imgBridge;
        public UI_Image imgShadow;
        public UI_Image imgReward;
        public MonoBehaviour profileGroup;
        public int introEnemyCount = 6;
        public int linearProfileSeats = 3;
        public MonoBehaviour enemyGroupPrefab;
        public float scrollSpeed = 2000f;
        public float rewardRevealHold = 0.6f;
        public float blockClearDuration = 0.6f;
        public float blockClearDropY = 150f;
        public float blockClearShakeAngle = 8f;
        public float winJumpPower = 250f;
        public float winJumpDuration = 0.6f;
        public float bridgeTopRatio = 0.4f;
        public float rewardTopRatio = 0.3f;
        public UI_Button btnClose;
        public UI_Button btnInfo;
        public UI_Button btnStart;
        public UI_Text txtTimer;
        public LavaRushRewardGridView gridRewardPanel;
        public UI_Rect rectStart;
        public UI_Rect rectMatchTutorial;
        public UI_Image imgTutorialPrefab;
        public UI_Rect tutorial1;
        public UI_Rect tutorial2;
        public UI_Rect tutorial3;
        public UI_Text txtTutorialDesc1;
        public UI_Text txtTutorialDesc2;
        public UI_Text txtTutorialDesc3;
        public float tutorialScrollDuration = 0.8f;
    }

    private const float LastBlockHeight = 100f;
    private const float LeftBlockX = -150f;
    private const float RightBlockX = 150f;
    private const float ProfileOffsetY = 80f;
    private const float ProfileScale = 1.7f;
    private const float RefreshInterval = 0.25f;
    private const float StartHiddenY = -500f;
    private const float StartRevealDuration = 0.3f;
    private const int StartPaddingShown = 400;
    private const int StartPaddingHidden = 50;
    private static readonly Vector2 FinalRewardSize = new(220f, 240f);

    [SerializeField] public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    private readonly List<LavaRushBlockView> _blocks = new();
    private readonly Dictionary<int, IReadOnlyList<ContentReward>> _rewardsByStage = new();
    private readonly Dictionary<int, MonoBehaviour> _enemyComponents = new();
    private readonly Dictionary<int, ILavaRushProfileGroupView> _enemyGroups = new();

    private IDisposable _updateSubscription;
    private IDisposable _lateUpdateSubscription;
    private UI_LavaRush_MatchTutorial _tutorial;
    private MonoBehaviour _userProfileComponent;
    private ILavaRushProfileGroupView _userProfileGroup;
    private Coroutine _introRoutine;
    private Coroutine _resultRoutine;
    private Coroutine _startRevealRoutine;
    private Coroutine _tutorialScrollRoutine;
    private Action _resultCanceled;
    private float _refreshElapsed;
    private float _lastBridgeTop = float.NaN;
    private float _lastContentHeight = float.NaN;
    private bool? _startShown;
    private bool _pendingDifficultyIntro;
    private bool _playingResult;
    private int _initializedDifficulty = LavaRushEngine.NoDifficulty;

    protected override LavaRushControllerRefs ControllerRefs => controller;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.Match;
    public bool IsInTutorial => ShouldShowTutorial();
    public bool IsDifficultyIntroPending => _pendingDifficultyIntro;
    public bool IsPlayingResult => _playingResult;

    protected override void OnBound()
    {
        refs ??= new Refs();
        refs.btnClose?.AddListener(HandleClose);
        refs.btnInfo?.AddListener(HandleInfo);
        refs.btnStart?.AddListener(HandleStart);

        _tutorial = new UI_LavaRush_MatchTutorial(this);
        _tutorial.OnStepShown += HandleTutorialStepShown;
        _tutorial.OnGuideCompleted += HandleTutorialCompleted;
        _tutorial.PrepareGuideRoot();

        _updateSubscription = Owner.FrameScheduler?.SubscribeUpdate(HandleUpdate);
        _lateUpdateSubscription = Owner.FrameScheduler?.SubscribeLateUpdate(HandleLateUpdate);

        if (Owner.SelectedDifficulty != LavaRushEngine.NoDifficulty)
            Initialize(Owner.SelectedDifficulty);
        RefreshDisplay();
    }

    protected override void OnWillOpen()
    {
        base.OnWillOpen();
        if (Owner?.SelectedDifficulty != LavaRushEngine.NoDifficulty)
            Initialize(Owner.SelectedDifficulty);
        RefreshDisplay();
    }

    protected override void OnDestroy()
    {
        refs?.btnClose?.RemoveListener(HandleClose);
        refs?.btnInfo?.RemoveListener(HandleInfo);
        refs?.btnStart?.RemoveListener(HandleStart);

        if (_tutorial != null)
        {
            _tutorial.OnStepShown -= HandleTutorialStepShown;
            _tutorial.OnGuideCompleted -= HandleTutorialCompleted;
        }

        _updateSubscription?.Dispose();
        _lateUpdateSubscription?.Dispose();
        ClearBlocks();
        DestroyProfileComponent(ref _userProfileComponent);
        base.OnDestroy();
    }

    private void OnEnable()
    {
        _lastBridgeTop = float.NaN;
        _lastContentHeight = float.NaN;
        if (Owner == null)
            return;
        if (Owner.SelectedDifficulty != LavaRushEngine.NoDifficulty)
            Initialize(Owner.SelectedDifficulty);
        RefreshDisplay();
        StartTutorial();
    }

    private void OnDisable()
    {
        StopOwnedCoroutine(ref _introRoutine);
        CancelResultPresentation();
        StopOwnedCoroutine(ref _startRevealRoutine);
        StopOwnedCoroutine(ref _tutorialScrollRoutine);
        _userProfileGroup?.CancelAnimations();
        foreach (ILavaRushProfileGroupView group in _enemyGroups.Values)
            group?.CancelAnimations();
        _tutorial?.ForceStop();
    }

    private void Update()
    {
        if (Owner != null && Owner.FrameScheduler == null)
            HandleUpdate(Time.unscaledDeltaTime);
    }

    private void LateUpdate()
    {
        if (Owner != null && Owner.FrameScheduler == null)
            HandleLateUpdate(Time.unscaledDeltaTime);
    }

    /// <summary>Rebuilds the package-owned block presentation for the selected engine difficulty.</summary>
    public void Initialize(int difficulty)
    {
        LavaRushEngine engine = Owner?.Engine;
        if (engine == null || difficulty <= 0 || !engine.Catalog.ContainsDifficulty(difficulty))
            return;

        LavaRushDifficultyDefinition definition = engine.Catalog.GetDifficulty(difficulty);
        if (_initializedDifficulty == difficulty
            && _blocks.Count == definition.StageCount
            && BlocksAreAlive())
        {
            RefreshBlockState();
            return;
        }

        ClearBlocks();
        _initializedDifficulty = difficulty;
        if (refs?.lavaBlock == null || refs.content == null)
        {
            Debug.LogError("[UI_LavaRush_Match] lavaBlock/content references are required.", this);
            return;
        }

        RectTransform parent = refs.content.RectTransform;
        for (int index = 0; index < definition.StageCount; index++)
        {
            int stage = definition.StageCount - index;
            LavaRushStageDefinition stageDefinition = definition.GetStage(stage);
            LavaRushBlockView block = Instantiate(refs.lavaBlock, parent, false);
            block.name = $"LavaRush_Block_{stage}";
            block.SetStageCount(stage.ToString());
            block.CollapseSeatPanel();
            block.SetSeatCount(SeatText(0, stageDefinition.Capacity));

            if (index == 0)
            {
                block.SetBlockPosition(new Vector2(0f, LastBlockHeight));
                block.SetRewardCellSize(FinalRewardSize);
                block.SetFinalRewardPresentation();
                block.SetSeatCountVisible(false);
            }
            else
            {
                block.SetBlockPosition(new Vector2(index % 2 == 1 ? LeftBlockX : RightBlockX, 0f));
                PresentBlockReward(block, stageDefinition.Rewards);
            }

            _blocks.Add(block);
            _rewardsByStage.Add(stage, stageDefinition.Rewards);
        }

        IReadOnlyList<ContentReward> finalRewards = definition
            .GetStage(definition.StageCount)
            .Rewards;
        refs?.gridRewardPanel?.SetRewards(finalRewards, Owner.RewardPresentation);
        if (controller?.Production?.RewardText != null)
            controller.Production.RewardText.Text = Owner.RenderRewards(finalRewards);

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        RefreshBlockState();
        SyncBridgeTop();
        SnapToBottom();
    }

    private void PresentBlockReward(
        LavaRushBlockView block,
        IReadOnlyList<ContentReward> rewards)
    {
        if (rewards == null || rewards.Count == 0)
        {
            block.SetRewardVisible(false);
            return;
        }

        LavaRushRewardPresentation presentation =
            Owner.RewardPresentation.Resolve(rewards[0]);
        block.SetRewardPresentation(
            presentation.Icon,
            presentation.AmountText,
            presentation.ShowAmount,
            presentation.ShowInfo,
            presentation.InfoRequested);
    }

    public void Initialize<TDifficulty>(TDifficulty difficulty)
        where TDifficulty : struct, Enum =>
        Initialize(Convert.ToInt32(difficulty));

    /// <summary>
    /// Arms the original difficulty-entry reveal. The next profile intro starts at the final reward
    /// before scrolling to the active foothold.
    /// </summary>
    public void MarkDifficultyIntroPending()
    {
        _pendingDifficultyIntro = true;
    }

    /// <summary>Plays the package-neutral opponent/player intro for the active engine stage.</summary>
    public void PlayProfileIntro()
    {
        if (Owner?.Engine == null)
            return;

        StopOwnedCoroutine(ref _introRoutine);
        RefreshOpponentsForStage(Owner.CurrentStage);
        RefreshOpponentsForStage(Owner.CurrentStage + 1);
        BindPlayerProfile();
        UpdateSeatProfiles(false);

        bool revealFinalReward = _pendingDifficultyIntro;
        _pendingDifficultyIntro = false;
        if (!isActiveAndEnabled)
        {
            BeginPlayerAppear();
            return;
        }

        _introRoutine = StartCoroutine(PlayProfileIntroRoutine(revealFinalReward));
    }

    /// <summary>
    /// Performs jump and block-collapse milestones before allowing the root to claim and route the
    /// result. The callback is never invoked before the visual sequence completes.
    /// </summary>
    public void PlayWinResult(Action onCompleted, Action onCanceled = null)
    {
        if (_playingResult)
            return;

        StopOwnedCoroutine(ref _resultRoutine);
        if (!isActiveAndEnabled)
        {
            onCompleted?.Invoke();
            return;
        }

        _playingResult = true;
        _resultCanceled = onCanceled;
        _resultRoutine = StartCoroutine(PlayWinResultRoutine(onCompleted));
    }

    /// <summary>Keeps the player on the current foothold for one rendered frame before routing loss.</summary>
    public void PlayLoseResult(Action onCompleted, Action onCanceled = null)
    {
        if (_playingResult)
            return;

        StopOwnedCoroutine(ref _resultRoutine);
        BindPlayerProfile();
        MovePlayerToStage(Owner?.CurrentStage ?? LavaRushEngine.MinStage);
        _userProfileGroup?.ShowPlayerOnly();
        if (!isActiveAndEnabled)
        {
            onCompleted?.Invoke();
            return;
        }

        _playingResult = true;
        _resultCanceled = onCanceled;
        _resultRoutine = StartCoroutine(PlayLoseResultRoutine(onCompleted));
    }

    internal void CancelResultPresentation()
    {
        StopOwnedCoroutine(ref _resultRoutine);
        Action canceled = _resultCanceled;
        _resultCanceled = null;
        bool wasPlayingResult = _playingResult;
        _playingResult = false;
        refs?.scrollLavaBlock?.SetUserScrollEnabled(true);
        if (wasPlayingResult)
            canceled?.Invoke();
    }

    /// <summary>Returns the authored reward cell for the exact stage; no generic-panel fallback.</summary>
    public Vector3 GetStageRewardWorldPos(int stage)
    {
        LavaRushBlockView block = BlockAtStage(stage);
        if (block?.RewardCellTransform != null)
            return block.RewardWorldPosition;

        Debug.LogError(
            $"[UI_LavaRush_Match] Reward cell is unavailable for stage {stage}.",
            this);
        return default;
    }

    public void StartTutorial()
    {
        if (_tutorial == null || _tutorial.IsActive || !ShouldShowTutorial())
            return;

        _tutorial.PrepareGuideRoot();
        SetTutorialGuideActive(true);
        if (!_tutorial.StartGuide())
            SetTutorialGuideActive(false);
    }

    internal void SetTutorialGuideActive(bool active)
    {
        refs?.rectMatchTutorial?.gameObject.SetActive(active);
        SetTutorialStepActive(-1);
        if (!active || Owner == null)
        {
            refs?.scrollLavaBlock?.SetUserScrollEnabled(true);
            return;
        }

        if (refs.txtTutorialDesc1 != null)
        {
            refs.txtTutorialDesc1.Text = Owner.LocalizeText(
                LavaRushLocalizationKeys.TutorialStep1,
                "Complete orders to earn progress.");
        }
        if (refs.txtTutorialDesc2 != null)
        {
            refs.txtTutorialDesc2.Text = Owner.LocalizeText(
                LavaRushLocalizationKeys.TutorialStep2,
                "Collect enough stones to advance.");
        }
        if (refs.txtTutorialDesc3 != null)
        {
            refs.txtTutorialDesc3.Text = Owner.LocalizeText(
                LavaRushLocalizationKeys.TutorialStep3,
                "Complete every stage to claim rewards.");
        }
    }

    internal UI_Image CreateTutorialFocus(
        RectTransform target,
        Sprite sprite,
        Vector2 size,
        Image.Type? imageType)
    {
        if (target == null)
            return null;
        if (refs?.imgTutorialPrefab == null)
        {
            Debug.LogError(
                "[UI_LavaRush_Match] imgTutorialPrefab reference is required.",
                this);
            return null;
        }

        UI_Image focus = Instantiate(refs.imgTutorialPrefab, target, false);
        RectTransform rect = focus.RectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.sizeDelta = size;
        focus.Sprite = sprite;
        if (imageType.HasValue && focus.Image != null)
            focus.Image.type = imageType.Value;
        AddOptionalTutorialMaskShape(focus.gameObject);
        focus.gameObject.SetActive(true);
        return focus;
    }

    internal Sprite ResolveTutorialSprite(TutorialFocusSprite spriteType)
    {
        Sprite sprite = Owner?.TutorialFocusSprites?.Get(spriteType);
        return sprite != null
            ? sprite
            : refs?.imgTutorialPrefab != null
                ? refs.imgTutorialPrefab.Sprite
                : null;
    }

    private static void AddOptionalTutorialMaskShape(GameObject target)
    {
        const string TypeName =
            "Coffee.UISoftMask.MaskingShape, Coffee.SoftMaskForUGUI";
        Type maskingShape = Type.GetType(TypeName, throwOnError: false);
        if (maskingShape != null && target.GetComponent(maskingShape) == null)
            target.AddComponent(maskingShape);
    }

    private IEnumerator PlayProfileIntroRoutine(bool revealFinalReward)
    {
        refs?.scrollLavaBlock?.SetUserScrollEnabled(false);
        if (revealFinalReward && refs?.scrollLavaBlock != null)
        {
            refs.scrollLavaBlock.SnapToTop();
            yield return WaitUnscaled(Mathf.Max(0f, refs.rewardRevealHold));
        }

        bool scrolled = refs?.scrollLavaBlock == null;
        refs?.scrollLavaBlock?.AnimateToBottom(
            Mathf.Max(1f, refs.scrollSpeed),
            () => scrolled = true);
        while (!scrolled && isActiveAndEnabled)
            yield return null;

        if (isActiveAndEnabled)
            BeginPlayerAppear();
        _introRoutine = null;
    }

    private void BeginPlayerAppear()
    {
        MovePlayerToStage(Owner?.CurrentStage ?? LavaRushEngine.MinStage);
        if (_userProfileGroup == null)
        {
            refs?.scrollLavaBlock?.SetUserScrollEnabled(true);
            return;
        }

        _userProfileGroup.PlayPlayerAppear(
            0f,
            () => refs?.scrollLavaBlock?.SetUserScrollEnabled(true),
            () => Owner?.PlayAudio(LavaRushAudioCue.ProfileAppear));
    }

    private IEnumerator PlayWinResultRoutine(Action onCompleted)
    {
        refs?.scrollLavaBlock?.SetUserScrollEnabled(false);
        SnapToBottom();
        BindPlayerProfile();

        int targetStage = Owner?.CurrentStage ?? LavaRushEngine.MinStage;
        int previousStage = Mathf.Max(LavaRushEngine.MinStage, targetStage - 1);
        LavaRushBlockView previousBlock = BlockAtStage(previousStage);
        LavaRushBlockView targetBlock = BlockAtStage(targetStage);
        RectTransform profileRect = _userProfileComponent != null
            ? _userProfileComponent.transform as RectTransform
            : null;

        if (profileRect != null && previousBlock?.BlockRectTransform != null)
        {
            MovePlayerToStage(previousStage);
            _userProfileGroup?.ShowPlayerOnly();
            Vector3 startWorld = profileRect.position;
            Vector3 targetWorld = targetBlock?.BlockRectTransform != null
                ? targetBlock.BlockRectTransform.TransformPoint(new Vector3(0f, ProfileOffsetY, 0f))
                : startWorld;
            profileRect.SetParent(transform, true);
            profileRect.position = startWorld;

            Owner?.PlayAudio(LavaRushAudioCue.WinJump);
            float duration = Mathf.Max(0f, refs?.winJumpDuration ?? 0f);
            float elapsed = 0f;
            float arcWorld = Mathf.Max(0f, refs?.winJumpPower ?? 0f) * profileRect.lossyScale.y;
            while (elapsed < duration && isActiveAndEnabled)
            {
                elapsed += Time.unscaledDeltaTime;
                float ratio = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                Vector3 position = Vector3.Lerp(startWorld, targetWorld, ratio);
                position.y += arcWorld * 4f * ratio * (1f - ratio);
                profileRect.position = position;
                yield return null;
            }
            MovePlayerToStage(targetStage);
        }

        if (previousBlock != null && previousBlock.gameObject.activeSelf)
            yield return AnimateBlockClear(previousBlock);

        _playingResult = false;
        _resultCanceled = null;
        _resultRoutine = null;
        refs?.scrollLavaBlock?.SetUserScrollEnabled(true);
        onCompleted?.Invoke();
    }

    private IEnumerator PlayLoseResultRoutine(Action onCompleted)
    {
        yield return null;
        _playingResult = false;
        _resultCanceled = null;
        _resultRoutine = null;
        onCompleted?.Invoke();
    }

    private IEnumerator AnimateBlockClear(LavaRushBlockView block)
    {
        RectTransform rect = block.transform as RectTransform;
        if (rect == null)
        {
            block.gameObject.SetActive(false);
            yield break;
        }

        Owner?.PlayAudio(LavaRushAudioCue.BlockClear);
        CanvasGroup canvasGroup = block.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = block.gameObject.AddComponent<CanvasGroup>();

        Vector2 originalSize = rect.sizeDelta;
        Vector2 originalPosition = rect.anchoredPosition;
        Quaternion originalRotation = rect.localRotation;
        float duration = Mathf.Max(0f, refs?.blockClearDuration ?? 0f);
        float elapsed = 0f;
        while (elapsed < duration && isActiveAndEnabled)
        {
            elapsed += Time.unscaledDeltaTime;
            float ratio = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            float eased = ratio * ratio;

            Vector2 size = originalSize;
            size.y = Mathf.Lerp(originalSize.y, 0f, eased);
            rect.sizeDelta = size;
            rect.anchoredPosition = originalPosition
                + Vector2.down * (Mathf.Max(0f, refs.blockClearDropY) * eased);
            rect.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Sin(ratio * Mathf.PI * 8f)
                * refs.blockClearShakeAngle
                * (1f - ratio));
            canvasGroup.alpha = 1f - eased;
            RebuildAndKeepBottom();
            yield return null;
        }

        block.gameObject.SetActive(false);
        rect.sizeDelta = originalSize;
        rect.anchoredPosition = originalPosition;
        rect.localRotation = originalRotation;
        canvasGroup.alpha = 1f;
    }

    private void HandleUpdate(float deltaTime)
    {
        if (!isActiveAndEnabled || Owner?.Engine == null)
            return;

        if (_tutorial?.IsActive == true)
        {
            _tutorial.UpdateClick();
            return;
        }

        _refreshElapsed += Mathf.Max(0f, deltaTime);
        if (_refreshElapsed < RefreshInterval)
            return;
        _refreshElapsed = 0f;
        RefreshDisplay();
    }

    private void HandleLateUpdate(float deltaTime)
    {
        if (!isActiveAndEnabled)
            return;
        SyncBridgeTop();
        KeepContentAtBottom();
    }

    private void RefreshDisplay()
    {
        LavaRushEngine engine = Owner?.Engine;
        if (engine == null)
            return;

        if (engine.SelectedDifficulty != LavaRushEngine.NoDifficulty)
            Initialize(engine.SelectedDifficulty);

        if (refs?.txtTimer != null)
            refs.txtTimer.Text = LavaRushTimeText.FormatDefault(engine.EventRemainingTime);

        bool waiting = engine.IsEventStarted
            && engine.IsEventActive
            && !engine.PendingEnd
            && engine.SelectedDifficulty != LavaRushEngine.NoDifficulty
            && !engine.IsStagePlaying
            && !engine.AllStagesComplete
            && !engine.IsFinalFoothold
            && engine.PendingResult == LavaRushResult.None
            && !_playingResult
            && !ShouldShowTutorial();
        SetStartShown(waiting);
        RefreshBlockState();
        UpdateSeatProfiles(true);

        if (ShouldShowTutorial())
            StartTutorial();
    }

    private void RefreshBlockState()
    {
        LavaRushEngine engine = Owner?.Engine;
        if (engine == null)
            return;

        for (int index = 0; index < _blocks.Count; index++)
        {
            LavaRushBlockView block = _blocks[index];
            if (block == null)
                continue;

            int stage = _blocks.Count - index;
            bool keepForWin = engine.PendingResult == LavaRushResult.Win
                && stage == engine.Stage - 1;
            bool visible = stage >= engine.Stage || keepForWin;
            block.gameObject.SetActive(visible);
            if (!visible)
                continue;

            if (stage < engine.StageCount
                && engine.IsStageRewardClaimed(stage)
                && !(engine.PendingResult == LavaRushResult.Win && stage == engine.Stage))
            {
                block.SetRewardVisible(false);
            }

            if (engine.IsStagePlaying && stage == engine.Stage + 1 && stage < engine.StageCount)
            {
                block.ExpandSeatPanel(0f);
                block.SetSeatCount(SeatText(engine.FakeSeatCount, engine.SeatCapacity));
            }
        }
    }

    private void RefreshOpponentsForStage(int stage)
    {
        if (!IsPlayableStage(stage))
            return;

        int count = Mathf.Max(0, refs?.introEnemyCount ?? 0);
        ILavaRushProfileRoster roster = Owner?.ProfileRoster;
        roster?.DeleteOpponents(stage, count);
        ILavaRushProfileGroupView group = EnsureEnemyGroup(stage);
        BindOpponents(group, stage, count);
    }

    private void UpdateSeatProfiles(bool animate)
    {
        LavaRushEngine engine = Owner?.Engine;
        if (engine == null)
            return;

        bool resultPending = engine.PendingResult != LavaRushResult.None;
        if (!engine.IsStagePlaying && !resultPending)
        {
            foreach (ILavaRushProfileGroupView group in _enemyGroups.Values)
                group?.SetOpponentCount(0, animate);
            return;
        }

        int baseStage = engine.PendingResult == LavaRushResult.Win
            ? Mathf.Max(LavaRushEngine.MinStage, engine.Stage - 1)
            : engine.Stage;
        int nextStage = baseStage + 1;
        if (IsPlayableStage(nextStage) && nextStage < engine.StageCount)
        {
            ILavaRushProfileGroupView next = EnsureEnemyGroup(nextStage);
            int shown = SeatProfilesShown(
                engine.PendingResult == LavaRushResult.None
                    ? engine.FakeSeatCount
                    : engine.ResultSeatCount,
                engine.PendingResult == LavaRushResult.None
                    ? engine.SeatCapacity
                    : engine.ResultSeatCapacity);
            next?.SetOpponentCount(
                shown,
                animate,
                () => Owner?.PlayAudio(LavaRushAudioCue.ProfileAppear));
        }

        if (IsPlayableStage(baseStage))
        {
            ILavaRushProfileGroupView current = EnsureEnemyGroup(baseStage);
            float ratio = engine.PendingResult == LavaRushResult.None
                ? Owner.SeatRatioForDisplay
                : engine.ResultSeatCapacity <= 0
                    ? 0f
                    : Mathf.Clamp01(engine.ResultSeatCount / (float)engine.ResultSeatCapacity);
            int shown = engine.PendingResult == LavaRushResult.Lose
                ? 0
                : Mathf.RoundToInt((1f - ratio) * Mathf.Max(0, refs.introEnemyCount));
            current?.SetOpponentCount(
                shown,
                animate,
                () => Owner?.PlayAudio(LavaRushAudioCue.ProfileAppear));
        }

        _userProfileComponent?.transform.SetAsLastSibling();
    }

    private ILavaRushProfileGroupView EnsureEnemyGroup(int stage)
    {
        if (_enemyGroups.TryGetValue(stage, out ILavaRushProfileGroupView existing))
            return existing;

        LavaRushBlockView block = BlockAtStage(stage);
        if (block?.BlockRectTransform == null)
            return null;

        MonoBehaviour component = refs?.enemyGroupPrefab != null
            ? Instantiate(refs.enemyGroupPrefab, block.BlockRectTransform, false)
            : Owner?.ProfileGroupFactory?.CreateOpponentProfileGroup(
                stage,
                block.BlockRectTransform);
        if (component == null)
            return null;
        if (component is not ILavaRushProfileGroupView group)
        {
            Destroy(component.gameObject);
            return null;
        }

        if (component.transform is RectTransform rect)
        {
            rect.anchoredPosition = new Vector2(0f, ProfileOffsetY);
            rect.localScale = Vector3.one * ProfileScale;
            rect.SetAsFirstSibling();
        }

        group.HidePlayer();
        group.SetOpponentCount(0, false);
        _enemyComponents.Add(stage, component);
        _enemyGroups.Add(stage, group);
        BindOpponents(group, stage, Mathf.Max(0, refs.introEnemyCount));
        return group;
    }

    private void BindOpponents(ILavaRushProfileGroupView group, int stage, int count)
    {
        ILavaRushProfileRoster roster = Owner?.ProfileRoster;
        if (group == null || roster == null)
            return;

        for (int slot = 0; slot < count; slot++)
            group.BindOpponent(slot, roster.LoadOrGenerateOpponent(stage, slot));
    }

    private void BindPlayerProfile()
    {
        if (_userProfileGroup != null)
        {
            _userProfileGroup.BindPlayer(Owner.PlayerProfile);
            return;
        }

        MonoBehaviour component = refs?.profileGroup;
        bool authoredSceneInstance = component != null
            && component.gameObject.scene.IsValid()
            && component.transform.IsChildOf(transform);
        if (component != null && !authoredSceneInstance)
            component = Instantiate(component, transform, false);
        else if (component == null)
            component = Owner?.ProfileGroupFactory?.CreatePlayerProfileGroup(transform);
        if (component == null)
            return;
        if (component is not ILavaRushProfileGroupView group)
        {
            if (!authoredSceneInstance)
                Destroy(component.gameObject);
            return;
        }

        _userProfileComponent = component;
        _userProfileGroup = group;
        if (component.transform is RectTransform rect)
            rect.localScale = Vector3.one * ProfileScale;
        group.BindPlayer(Owner.PlayerProfile);
        group.HideAll();
    }

    private void MovePlayerToStage(int stage)
    {
        BindPlayerProfile();
        LavaRushBlockView block = BlockAtStage(stage);
        if (_userProfileComponent == null || block?.BlockRectTransform == null)
            return;

        RectTransform rect = _userProfileComponent.transform as RectTransform;
        if (rect == null)
            return;
        rect.SetParent(block.BlockRectTransform, false);
        rect.anchoredPosition = new Vector2(0f, ProfileOffsetY);
        rect.localScale = Vector3.one * ProfileScale;
        rect.SetAsLastSibling();
    }

    private void HandleTutorialStepShown(int step)
    {
        SetTutorialStepActive(step);
        switch (step)
        {
            case 0:
                refs?.scrollLavaBlock?.SetUserScrollEnabled(false);
                SnapToBottom();
                break;
            case 1:
                BindPlayerProfile();
                MovePlayerToStage(Owner?.CurrentStage ?? LavaRushEngine.MinStage);
                _userProfileGroup?.ShowPlayerOnly();
                if (_userProfileComponent?.transform is RectTransform profileRect)
                {
                    _tutorial.AddStepFocus(
                        1,
                        profileRect,
                        TutorialFocusSprite.Circle,
                        new Vector2(400f, 300f));
                }
                break;
            case 2:
                StopOwnedCoroutine(ref _tutorialScrollRoutine);
                if (isActiveAndEnabled)
                    _tutorialScrollRoutine = StartCoroutine(PlayTutorialFinalScroll());
                break;
        }
    }

    private IEnumerator PlayTutorialFinalScroll()
    {
        _tutorial.SetLocked(true);
        UI_Scroll scroll = refs?.scrollLavaBlock;
        if (scroll != null)
        {
            float start = scroll.VerticalNormalizedPosition;
            float duration = Mathf.Max(0f, refs.tutorialScrollDuration);
            float elapsed = 0f;
            while (elapsed < duration && isActiveAndEnabled)
            {
                elapsed += Time.unscaledDeltaTime;
                float ratio = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - ratio, 3f);
                scroll.VerticalNormalizedPosition = Mathf.Lerp(start, 1f, eased);
                yield return null;
            }
            scroll.VerticalNormalizedPosition = 1f;
        }

        LavaRushBlockView finalBlock = BlockAtStage(Owner?.StageCount ?? 0);
        if (finalBlock?.RewardCellTransform != null)
        {
            _tutorial.AddStepFocus(
                2,
                finalBlock.RewardCellTransform,
                TutorialFocusSprite.Circle,
                new Vector2(400f, 400f));
        }
        if (refs?.imgReward != null)
        {
            _tutorial.AddStepFocus(
                2,
                refs.imgReward.RectTransform,
                refs.imgReward.Sprite,
                refs.imgReward.RectTransform.rect.size);
        }

        _tutorial.SetLocked(false);
        _tutorialScrollRoutine = null;
    }

    private void HandleTutorialCompleted()
    {
        refs?.scrollLavaBlock?.SetUserScrollEnabled(true);
        SnapToBottom();
        Owner?.OnTutorialComplete();
        RefreshDisplay();
    }

    private void SetTutorialStepActive(int step)
    {
        refs?.tutorial1?.gameObject.SetActive(step == 0);
        refs?.tutorial2?.gameObject.SetActive(step == 1);
        refs?.tutorial3?.gameObject.SetActive(step == 2);
    }

    private bool ShouldShowTutorial()
    {
        LavaRushEngine engine = Owner?.Engine;
        return engine != null
            && engine.SelectedDifficulty != LavaRushEngine.NoDifficulty
            && !engine.TutorialDone
            && !engine.IsStagePlaying
            && !engine.AllStagesComplete
            && engine.PendingResult == LavaRushResult.None;
    }

    private int SeatProfilesShown(int seatCount, int capacity)
    {
        if (seatCount <= 0)
            return 0;
        int maximum = Mathf.Max(0, refs?.introEnemyCount ?? 0);
        int linear = Mathf.Clamp(refs?.linearProfileSeats ?? 0, 0, maximum);
        if (seatCount <= linear)
            return seatCount;
        if (capacity <= linear)
            return Mathf.Min(seatCount, maximum);

        float ratio = (seatCount - linear) / (float)(capacity - linear);
        return Mathf.Clamp(
            linear + Mathf.RoundToInt(ratio * (maximum - linear)),
            linear,
            maximum);
    }

    private void SetStartShown(bool shown)
    {
        if (refs?.rectStart == null || _startShown == shown)
            return;

        bool immediate = !_startShown.HasValue || !isActiveAndEnabled;
        _startShown = shown;
        refs.btnStart?.SetInteractable(shown);
        StopOwnedCoroutine(ref _startRevealRoutine);
        if (immediate)
        {
            ApplyStartReveal(shown ? 0f : StartHiddenY, shown ? StartPaddingShown : StartPaddingHidden);
            return;
        }

        _startRevealRoutine = StartCoroutine(AnimateStartReveal(shown));
    }

    private IEnumerator AnimateStartReveal(bool shown)
    {
        RectTransform rect = refs.rectStart.RectTransform;
        float startY = rect.anchoredPosition.y;
        int startPadding = refs.verContent?.padding.bottom ?? StartPaddingHidden;
        float targetY = shown ? 0f : StartHiddenY;
        int targetPadding = shown ? StartPaddingShown : StartPaddingHidden;
        float elapsed = 0f;
        while (elapsed < StartRevealDuration && isActiveAndEnabled)
        {
            elapsed += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(elapsed / StartRevealDuration);
            float eased = 1f - (1f - ratio) * (1f - ratio);
            ApplyStartReveal(
                Mathf.Lerp(startY, targetY, eased),
                Mathf.RoundToInt(Mathf.Lerp(startPadding, targetPadding, eased)));
            yield return null;
        }

        ApplyStartReveal(targetY, targetPadding);
        _startRevealRoutine = null;
    }

    private void ApplyStartReveal(float y, int bottomPadding)
    {
        RectTransform rect = refs.rectStart.RectTransform;
        Vector2 position = rect.anchoredPosition;
        position.y = y;
        rect.anchoredPosition = position;
        if (refs.verContent != null)
            refs.verContent.padding.bottom = bottomPadding;
        RebuildAndKeepBottom();
    }

    private void SyncBridgeTop()
    {
        if (refs?.imgBridge == null)
            return;

        float height = refs.imgBridge.RectTransform.rect.height;
        float top = height * refs.bridgeTopRatio;
        if (Mathf.Approximately(top, _lastBridgeTop))
            return;
        _lastBridgeTop = top;

        ApplyTop(refs.scrollLavaBlock?.RectTransform, top);
        ApplyTop(refs.imgShadow?.RectTransform, top);
        ApplyTopPosition(refs.imgReward?.RectTransform, height * refs.rewardTopRatio);
    }

    private void KeepContentAtBottom()
    {
        if (Owner?.IsStagePlaying != true
            || _playingResult
            || refs?.content == null
            || refs.scrollLavaBlock == null)
        {
            return;
        }

        float height = refs.content.RectTransform.rect.height;
        float normalized = refs.scrollLavaBlock.VerticalNormalizedPosition;
        if (!Mathf.Approximately(height, _lastContentHeight)
            || normalized < -1f
            || normalized > 2f)
        {
            _lastContentHeight = height;
            refs.scrollLavaBlock.SnapToBottom();
        }
    }

    private void RebuildAndKeepBottom()
    {
        if (refs?.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(refs.content.RectTransform);
        refs?.scrollLavaBlock?.SnapToBottom();
    }

    private void SnapToBottom()
    {
        SyncBridgeTop();
        RebuildAndKeepBottom();
    }

    private LavaRushBlockView BlockAtStage(int stage)
    {
        int index = _blocks.Count - stage;
        return index >= 0 && index < _blocks.Count ? _blocks[index] : null;
    }

    private bool IsPlayableStage(int stage) =>
        stage >= LavaRushEngine.MinStage && stage <= (Owner?.StageCount ?? 0);

    private bool BlocksAreAlive()
    {
        for (int index = 0; index < _blocks.Count; index++)
            if (_blocks[index] == null)
                return false;
        return true;
    }

    private void ClearBlocks()
    {
        foreach (ILavaRushProfileGroupView group in _enemyGroups.Values)
            group?.CancelAnimations();
        foreach (MonoBehaviour component in _enemyComponents.Values)
            if (component != null)
                DestroyRuntimeObject(component.gameObject);
        _enemyComponents.Clear();
        _enemyGroups.Clear();

        if (_userProfileComponent != null)
        {
            _userProfileComponent.transform.SetParent(transform, false);
            _userProfileGroup?.HideAll();
        }

        for (int index = 0; index < _blocks.Count; index++)
        {
            LavaRushBlockView block = _blocks[index];
            if (block == null)
                continue;
            block.gameObject.SetActive(false);
            DestroyRuntimeObject(block.gameObject);
        }
        _blocks.Clear();
        _rewardsByStage.Clear();
        _initializedDifficulty = LavaRushEngine.NoDifficulty;
    }

    private void HandleClose() => Close();
    private void HandleInfo() => Owner?.OpenTutorial();

    private void HandleStart()
    {
        Owner?.StartStage();
        RefreshDisplay();
    }

    private static string SeatText(int occupied, int capacity) =>
        $"<color=#ef1715>{Mathf.Max(0, occupied)}</color>/<color=#1bc00a>{Mathf.Max(0, capacity)}</color>";

    private static void ApplyTop(RectTransform rect, float top)
    {
        if (rect == null)
            return;
        Vector2 offset = rect.offsetMax;
        offset.y = -top;
        rect.offsetMax = offset;
    }

    private static void ApplyTopPosition(RectTransform rect, float top)
    {
        if (rect == null)
            return;
        Vector2 position = rect.anchoredPosition;
        position.y = -top;
        rect.anchoredPosition = position;
    }

    private static IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void StopOwnedCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null)
            return;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    private static void DestroyProfileComponent(ref MonoBehaviour component)
    {
        if (component != null)
            DestroyRuntimeObject(component.gameObject);
        component = null;
    }

    private static void DestroyRuntimeObject(GameObject value)
    {
        if (value == null)
            return;

        if (Application.isPlaying)
            Destroy(value);
        else
            DestroyImmediate(value);
    }
}
