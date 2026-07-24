using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.LavaRush;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Explicit product seams used by the restored production controller family.</summary>
    public sealed class LavaRushControllerContext
    {
        public LavaRushControllerContext(
            LavaRushEngine engine,
            ILavaRushFrameScheduler frameScheduler = null,
            ILavaRushCountdownScheduler countdownScheduler = null,
            ILavaRushAudio audio = null,
            ILavaRushUILocalizer localizer = null,
            ILavaRushUIRewardRenderer rewardRenderer = null,
            ILavaRushProfileRoster profiles = null,
            ILavaRushOrderProgressSource orderProgress = null,
            ILavaRushAccessService access = null,
            Func<Vector3, bool> claimPendingReward = null,
            Action refreshAccess = null,
            ILavaRushProfileGroupFactory profileGroupFactory = null,
            ILavaRushTutorialFocusSpriteProvider tutorialFocusSprites = null,
            ILavaRushRewardPresentationProvider rewardPresentation = null)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            FrameScheduler = frameScheduler;
            CountdownScheduler = countdownScheduler;
            Audio = audio ?? NullLavaRushAudio.Instance;
            Localizer = localizer ?? PassthroughLavaRushUILocalizer.Instance;
            RewardRenderer = rewardRenderer ?? TextLavaRushUIRewardRenderer.Instance;
            Profiles = profiles ?? DefaultLavaRushProfileRoster.Instance;
            ProfileGroupFactory = profileGroupFactory;
            TutorialFocusSprites =
                tutorialFocusSprites ?? NullLavaRushTutorialFocusSpriteProvider.Instance;
            RewardPresentation =
                rewardPresentation ?? DefaultLavaRushRewardPresentationProvider.Instance;
            OrderProgress = orderProgress;
            Access = access;
            ClaimPendingReward = claimPendingReward ?? (_ => Engine.ClaimPendingReward());
            RefreshAccess = refreshAccess;
        }

        public LavaRushEngine Engine { get; }
        public ILavaRushFrameScheduler FrameScheduler { get; }
        public ILavaRushCountdownScheduler CountdownScheduler { get; }
        public ILavaRushAudio Audio { get; }
        public ILavaRushUILocalizer Localizer { get; }
        public ILavaRushUIRewardRenderer RewardRenderer { get; }
        public ILavaRushProfileRoster Profiles { get; }
        public ILavaRushProfileGroupFactory ProfileGroupFactory { get; }
        public ILavaRushTutorialFocusSpriteProvider TutorialFocusSprites { get; }
        public ILavaRushRewardPresentationProvider RewardPresentation { get; }
        public ILavaRushOrderProgressSource OrderProgress { get; }
        public ILavaRushAccessService Access { get; }
        public Func<Vector3, bool> ClaimPendingReward { get; }
        public Action RefreshAccess { get; }
    }

    public enum LavaRushControllerScreen
    {
        EventStart,
        Difficulty,
        Tutorial,
        Match,
        MatchWin,
        MatchLose,
        MatchEnd,
        EventEnd,
    }

    /// <summary>Immutable, engine-derived input shared by the restored screen controllers.</summary>
    public sealed class LavaRushControllerSnapshot
    {
        public LavaRushControllerSnapshot(
            LavaRushControllerScreen screen,
            string title,
            string message,
            int difficulty,
            int stage,
            int stageCount,
            int progress,
            int requiredProgress,
            int occupiedSeats,
            int seatCapacity,
            int rank,
            TimeSpan eventRemaining,
            TimeSpan stageRemaining,
            LavaRushResult result,
            IReadOnlyList<ContentReward> rewards,
            LavaRushUIButtonModel primary,
            LavaRushUIButtonModel secondary,
            LavaRushUIButtonModel tertiary)
        {
            Screen = screen;
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            Difficulty = difficulty;
            Stage = stage;
            StageCount = stageCount;
            Progress = Math.Max(0, progress);
            RequiredProgress = Math.Max(0, requiredProgress);
            OccupiedSeats = Math.Max(0, occupiedSeats);
            SeatCapacity = Math.Max(0, seatCapacity);
            Rank = Math.Max(1, rank);
            EventRemaining = eventRemaining > TimeSpan.Zero ? eventRemaining : TimeSpan.Zero;
            StageRemaining = stageRemaining > TimeSpan.Zero ? stageRemaining : TimeSpan.Zero;
            Result = result;
            Rewards = rewards ?? Array.Empty<ContentReward>();
            Primary = primary ?? LavaRushUIButtonModel.Hidden;
            Secondary = secondary ?? LavaRushUIButtonModel.Hidden;
            Tertiary = tertiary ?? LavaRushUIButtonModel.Hidden;
        }

        public LavaRushControllerScreen Screen { get; }
        public string Title { get; }
        public string Message { get; }
        public int Difficulty { get; }
        public int Stage { get; }
        public int StageCount { get; }
        public int Progress { get; }
        public int RequiredProgress { get; }
        public int OccupiedSeats { get; }
        public int SeatCapacity { get; }
        public int Rank { get; }
        public TimeSpan EventRemaining { get; }
        public TimeSpan StageRemaining { get; }
        public LavaRushResult Result { get; }
        public IReadOnlyList<ContentReward> Rewards { get; }
        public LavaRushUIButtonModel Primary { get; }
        public LavaRushUIButtonModel Secondary { get; }
        public LavaRushUIButtonModel Tertiary { get; }
        public float ProgressRatio => RequiredProgress <= 0
            ? 0f
            : Math.Min(1f, Progress / (float)RequiredProgress);
    }
}
