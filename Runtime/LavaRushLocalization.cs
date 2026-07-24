using System;
using System.Collections.Generic;

namespace ActionFit.LavaRush.UI
{
    /// <summary>One package semantic key and its standalone-safe English fallback.</summary>
    public readonly struct LavaRushLocalizationMessage
    {
        public LavaRushLocalizationMessage(string key, string fallback)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Fallback = fallback ?? string.Empty;
        }

        public string Key { get; }
        public string Fallback { get; }
    }

    /// <summary>Optional locale-change signal for manually rendered package text.</summary>
    public interface ILavaRushLocalizationRefreshSource
    {
        event Action LocaleChanged;
    }

    /// <summary>Complete semantic coverage for the 18 operational Lava Rush strings.</summary>
    public static class LavaRushLocalizationKeys
    {
        public const string DifficultyDescription = "lava_rush.localization.difficulty_description";
        public const string EventEndDescription = "lava_rush.localization.event_end_description";
        public const string MatchLosePrimary = "lava_rush.localization.match_lose_primary";
        public const string MatchLoseRemaining = "lava_rush.localization.match_lose_remaining";
        public const string MatchLoseTertiary = "lava_rush.localization.match_lose_tertiary";
        public const string MatchDescription = "lava_rush.localization.match_description";
        public const string EventStartDescription = "lava_rush.localization.event_start_description";
        public const string Title = "lava_rush.localization.title";
        public const string TutorialStep1 = "lava_rush.localization.tutorial_step_1";
        public const string TutorialStep2 = "lava_rush.localization.tutorial_step_2";
        public const string TutorialStep3 = "lava_rush.localization.tutorial_step_3";
        public const string TutorialInfo1 = "lava_rush.localization.tutorial_info_1";
        public const string TutorialInfo2 = "lava_rush.localization.tutorial_info_2";
        public const string TutorialInfo3 = "lava_rush.localization.tutorial_info_3";
        public const string TutorialInfo4 = "lava_rush.localization.tutorial_info_4";
        public const string MatchWinPrimary = "lava_rush.localization.match_win_primary";
        public const string MatchWinSecondary = "lava_rush.localization.match_win_secondary";
        public const string MatchWinComplete = "lava_rush.localization.match_win_complete";

        private static readonly LavaRushLocalizationMessage[] Messages =
        {
            new(DifficultyDescription, "Choose your difficulty."),
            new(EventEndDescription, "The Lava Rush event has ended."),
            new(MatchLosePrimary, "Time is up."),
            new(MatchLoseRemaining, "{0} levels remaining"),
            new(MatchLoseTertiary, "Try the stage again."),
            new(MatchDescription, "Complete orders and merge items."),
            new(EventStartDescription, "The Lava Rush event has started."),
            new(Title, "Lava Rush"),
            new(TutorialStep1, "Complete orders to earn progress."),
            new(TutorialStep2, "Merge eligible items to earn progress."),
            new(TutorialStep3, "Reach the target before time runs out."),
            new(TutorialInfo1, "Select a difficulty."),
            new(TutorialInfo2, "Clear each stage before time runs out."),
            new(TutorialInfo3, "Earn progress from orders and merges."),
            new(TutorialInfo4, "Claim each reward after a win."),
            new(MatchWinPrimary, "Stage cleared!"),
            new(MatchWinSecondary, "Your reward is ready."),
            new(MatchWinComplete, "All stages complete!"),
        };

        public static IReadOnlyList<LavaRushLocalizationMessage> All => Messages;

        public static string GetFallback(string key)
        {
            for (int index = 0; index < Messages.Length; index++)
            {
                if (string.Equals(Messages[index].Key, key, StringComparison.Ordinal))
                    return Messages[index].Fallback;
            }

            return string.Empty;
        }
    }
}
