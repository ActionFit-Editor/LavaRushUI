using System;
using System.Collections.Generic;
using System.Text;
using ActionFit.Content;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Resolves package-owned text while allowing a consuming project to supply localization.</summary>
    public interface ILavaRushUILocalizer
    {
        string Get(string key, string fallback);
    }

    /// <summary>Receives presentation-only audio cues without coupling the package to a sound system.</summary>
    public interface ILavaRushUIAudio
    {
        void Play(string cue);
    }

    /// <summary>Formats an engine-owned reward snapshot for the active presentation.</summary>
    public interface ILavaRushUIRewardRenderer
    {
        string Render(IReadOnlyList<ContentReward> rewards, ILavaRushUILocalizer localizer);
    }

    /// <summary>Supplies one presentation-only player identity without exposing project profile state.</summary>
    public interface ILavaRushUIProfileProvider
    {
        LavaRushUIProfile GetProfile();
    }

    /// <summary>Supplies optional tutorial focus sprites owned by the consuming project.</summary>
    public interface ILavaRushTutorialFocusSpriteProvider
    {
        Sprite Get(TutorialFocusSprite spriteType);
    }

    public sealed class NullLavaRushTutorialFocusSpriteProvider :
        ILavaRushTutorialFocusSpriteProvider
    {
        public static NullLavaRushTutorialFocusSpriteProvider Instance { get; } = new();

        private NullLavaRushTutorialFocusSpriteProvider()
        {
        }

        public Sprite Get(TutorialFocusSprite spriteType) => null;
    }

    public sealed class PassthroughLavaRushUILocalizer : ILavaRushUILocalizer
    {
        public static PassthroughLavaRushUILocalizer Instance { get; } = new();

        private PassthroughLavaRushUILocalizer()
        {
        }

        public string Get(string key, string fallback) => fallback ?? string.Empty;
    }

    public sealed class NullLavaRushUIAudio : ILavaRushUIAudio
    {
        public static NullLavaRushUIAudio Instance { get; } = new();

        private NullLavaRushUIAudio()
        {
        }

        public void Play(string cue)
        {
        }
    }

    public sealed class LavaRushUIProfile
    {
        public LavaRushUIProfile(string displayName, Color accentColor)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName;
            AccentColor = accentColor;
        }

        public string DisplayName { get; }
        public Color AccentColor { get; }
    }

    public sealed class DefaultLavaRushUIProfileProvider : ILavaRushUIProfileProvider
    {
        public static DefaultLavaRushUIProfileProvider Instance { get; } = new();

        private DefaultLavaRushUIProfileProvider()
        {
        }

        public LavaRushUIProfile GetProfile() => new("Player", new Color(1f, 0.58f, 0.18f, 1f));
    }

    public sealed class TextLavaRushUIRewardRenderer : ILavaRushUIRewardRenderer
    {
        public static TextLavaRushUIRewardRenderer Instance { get; } = new();

        private TextLavaRushUIRewardRenderer()
        {
        }

        public string Render(IReadOnlyList<ContentReward> rewards, ILavaRushUILocalizer localizer)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (int index = 0; index < rewards.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append("  |  ");
                }
                ContentReward reward = rewards[index];
                string format = localizer?.Get(LavaRushUIKeys.FormatReward, "{0} x{1}") ?? "{0} x{1}";
                try
                {
                    builder.AppendFormat(format, reward.RewardId, reward.Amount);
                }
                catch (FormatException)
                {
                    builder.AppendFormat("{0} x{1}", reward.RewardId, reward.Amount);
                }
            }
            return builder.ToString();
        }
    }

    public static class LavaRushUIKeys
    {
        public const string Title = "lava_rush.ui.title";
        public const string ScreenEventStart = "lava_rush.ui.screen.event_start";
        public const string ScreenDifficulty = "lava_rush.ui.screen.difficulty";
        public const string ScreenTutorial = "lava_rush.ui.screen.tutorial";
        public const string ScreenMatch = "lava_rush.ui.screen.match";
        public const string ScreenResult = "lava_rush.ui.screen.result";
        public const string ScreenComplete = "lava_rush.ui.screen.complete";
        public const string ScreenEventEnd = "lava_rush.ui.screen.event_end";
        public const string ActionStartEvent = "lava_rush.ui.action.start_event";
        public const string ActionEasy = "lava_rush.ui.action.easy";
        public const string ActionNormal = "lava_rush.ui.action.normal";
        public const string ActionHard = "lava_rush.ui.action.hard";
        public const string ActionContinue = "lava_rush.ui.action.continue";
        public const string ActionStartStage = "lava_rush.ui.action.start_stage";
        public const string ActionAddProgress = "lava_rush.ui.action.add_progress";
        public const string ActionEvaluateStage = "lava_rush.ui.action.evaluate_stage";
        public const string ActionClaim = "lava_rush.ui.action.claim";
        public const string ActionRetry = "lava_rush.ui.action.retry";
        public const string ActionEndEvent = "lava_rush.ui.action.end_event";
        public const string ActionClose = "lava_rush.ui.action.close";
        public const string StatusEventStarted = "lava_rush.ui.status.event_started";
        public const string StatusEventUnavailable = "lava_rush.ui.status.event_unavailable";
        public const string StatusDifficultySelected = "lava_rush.ui.status.difficulty_selected";
        public const string StatusDifficultyUnavailable = "lava_rush.ui.status.difficulty_unavailable";
        public const string StatusTutorialComplete = "lava_rush.ui.status.tutorial_complete";
        public const string StatusStageStarted = "lava_rush.ui.status.stage_started";
        public const string StatusStageUnavailable = "lava_rush.ui.status.stage_unavailable";
        public const string StatusProgressAdded = "lava_rush.ui.status.progress_added";
        public const string StatusProgressUnavailable = "lava_rush.ui.status.progress_unavailable";
        public const string StatusStagePending = "lava_rush.ui.status.stage_pending";
        public const string StatusRewardClaimed = "lava_rush.ui.status.reward_claimed";
        public const string StatusRewardUnavailable = "lava_rush.ui.status.reward_unavailable";
        public const string StatusResultCleared = "lava_rush.ui.status.result_cleared";
        public const string StatusEventEnded = "lava_rush.ui.status.event_ended";
        public const string MessageStart = "lava_rush.ui.message.start";
        public const string MessageDifficulty = "lava_rush.ui.message.difficulty";
        public const string MessageTutorial = "lava_rush.ui.message.tutorial";
        public const string MessageReady = "lava_rush.ui.message.ready";
        public const string MessagePlaying = "lava_rush.ui.message.playing";
        public const string MessageWin = "lava_rush.ui.message.win";
        public const string MessageLose = "lava_rush.ui.message.lose";
        public const string MessageComplete = "lava_rush.ui.message.complete";
        public const string MessageEventEnd = "lava_rush.ui.message.event_end";
        public const string FormatEventTime = "lava_rush.ui.format.event_time";
        public const string FormatStageTime = "lava_rush.ui.format.stage_time";
        public const string FormatStage = "lava_rush.ui.format.stage";
        public const string FormatProgress = "lava_rush.ui.format.progress";
        public const string FormatSeats = "lava_rush.ui.format.seats";
        public const string FormatRank = "lava_rush.ui.format.rank";
        public const string FormatReward = "lava_rush.ui.format.reward";
        public const string FormatProfile = "lava_rush.ui.format.profile";
        public const string AudioScreen = "lava_rush.ui.audio.screen";
        public const string AudioProgress = "lava_rush.ui.audio.progress";
        public const string AudioReward = "lava_rush.ui.audio.reward";
    }
}
