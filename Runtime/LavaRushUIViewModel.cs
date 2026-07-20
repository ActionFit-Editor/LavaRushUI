using System;
using System.Collections.Generic;
using ActionFit.Content;

namespace ActionFit.LavaRush.UI
{
    public enum LavaRushUIScreen
    {
        EventStart = 0,
        Difficulty = 1,
        Tutorial = 2,
        Match = 3,
        Result = 4,
        Complete = 5,
        EventEnd = 6,
    }

    public enum LavaRushUIAction
    {
        None = 0,
        StartEvent = 1,
        SelectEasy = 2,
        SelectNormal = 3,
        SelectHard = 4,
        CompleteTutorial = 5,
        StartStage = 6,
        AddProgress = 7,
        EvaluateStage = 8,
        ConfirmResult = 9,
        EndEvent = 10,
        Close = 11,
    }

    /// <summary>Describes one visible action without allowing a view to mutate engine state directly.</summary>
    public sealed class LavaRushUIButtonModel
    {
        public LavaRushUIButtonModel(LavaRushUIAction action, string label, bool visible = true, bool interactable = true)
        {
            Action = action;
            Label = label ?? string.Empty;
            Visible = visible;
            Interactable = interactable;
        }

        public LavaRushUIAction Action { get; }
        public string Label { get; }
        public bool Visible { get; }
        public bool Interactable { get; }

        public static LavaRushUIButtonModel Hidden { get; } = new(LavaRushUIAction.None, string.Empty, false, false);
    }

    /// <summary>Immutable engine-derived snapshot consumed by the Lava Rush UGUI presentation.</summary>
    public sealed class LavaRushUIViewModel
    {
        public LavaRushUIViewModel(
            LavaRushUIScreen screen,
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
            Message = message ?? string.Empty;
            Difficulty = difficulty;
            Stage = stage;
            StageCount = stageCount;
            Progress = Math.Max(0, progress);
            RequiredProgress = Math.Max(0, requiredProgress);
            OccupiedSeats = Math.Max(0, occupiedSeats);
            SeatCapacity = Math.Max(0, seatCapacity);
            Rank = Math.Max(1, rank);
            EventRemaining = eventRemaining < TimeSpan.Zero ? TimeSpan.Zero : eventRemaining;
            StageRemaining = stageRemaining < TimeSpan.Zero ? TimeSpan.Zero : stageRemaining;
            Result = result;
            Rewards = rewards ?? Array.Empty<ContentReward>();
            Primary = primary ?? LavaRushUIButtonModel.Hidden;
            Secondary = secondary ?? LavaRushUIButtonModel.Hidden;
            Tertiary = tertiary ?? LavaRushUIButtonModel.Hidden;
        }

        public LavaRushUIScreen Screen { get; }
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
